using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using patient.application.Integration.FoodPlans;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace patient.infrastructure.Integration;

/// <summary>Consume los eventos foodplan.created y foodplan.updated desde RabbitMQ y sincroniza en la tabla FoodPlan.</summary>
internal sealed class FoodPlanEventConsumerHostedService : BackgroundService
{
    private readonly ILogger<FoodPlanEventConsumerHostedService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqFoodPlanOptions _options;
    private IConnection? _connection;
    private IModel? _channel;

    public FoodPlanEventConsumerHostedService(
        ILogger<FoodPlanEventConsumerHostedService> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqFoodPlanOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                DispatchConsumersAsync = true
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(
                _options.FoodPlansExchange,
                ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _channel.QueueDeclare(
                _options.FoodPlansQueue,
                durable: true,
                exclusive: false,
                autoDelete: false);

            _channel.QueueBind(
                _options.FoodPlansQueue,
                _options.FoodPlansExchange,
                _options.FoodPlanCreatedRoutingKey);
            _channel.QueueBind(
                _options.FoodPlansQueue,
                _options.FoodPlansExchange,
                _options.FoodPlanUpdatedRoutingKey);

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += (_, ea) => ProcessMessageAsync(ea);

            _channel.BasicConsume(
                _options.FoodPlansQueue,
                autoAck: false,
                consumer);

            _logger.LogInformation("FoodPlan event consumer started. Queue: {Queue}", _options.FoodPlansQueue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start RabbitMQ FoodPlan consumer");
            throw;
        }

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs ea)
    {
        var routingKey = ea.RoutingKey;
        var isCreated = string.Equals(routingKey, _options.FoodPlanCreatedRoutingKey, StringComparison.Ordinal);
        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);

        try
        {
            var dto = JsonSerializer.Deserialize<FoodPlanIntegrationEventDto>(json);
            if (dto is null)
            {
                _logger.LogWarning("Invalid message body, skipping: {Body}", json);
                Ack(ea);
                return;
            }

            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var command = new SyncFoodPlanFromIntegrationCommand
            {
                FoodPlanId = dto.FoodPlanId,
                IsCreated = isCreated,
                Name = dto.Name
            };
            await mediator.Send(command);
            Ack(ea);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message. RoutingKey: {RoutingKey}, Body: {Body}", routingKey, json);
            Nack(ea, requeue: false);
        }
    }

    private void Ack(BasicDeliverEventArgs ea)
    {
        _channel?.BasicAck(ea.DeliveryTag, false);
    }

    private void Nack(BasicDeliverEventArgs ea, bool requeue)
    {
        _channel?.BasicNack(ea.DeliveryTag, false, requeue);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
