using System.Text;
using System.Text.Json;
using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using patient.infrastructure.Observability;
using patient.application.Integration.FoodPlans;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

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
        var delayMs = _options.ReconnectDelaySeconds * 1000;
        var attempt = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_options.MaxReconnectAttempts > 0 && attempt >= _options.MaxReconnectAttempts)
                {
                    _logger.LogError("Max RabbitMQ reconnect attempts ({Max}) reached. Stopping consumer.", _options.MaxReconnectAttempts);
                    return;
                }

                if (attempt > 0)
                    _logger.LogInformation("RabbitMQ reconnect attempt {Attempt}…", attempt + 1);

                var factory = new ConnectionFactory
                {
                    HostName = _options.HostName,
                    Port = _options.Port,
                    UserName = _options.UserName,
                    Password = _options.Password,
                    VirtualHost = _options.VirtualHost,
                    DispatchConsumersAsync = true,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(_options.ReconnectDelaySeconds)
                };
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                // Exchange "meal-plans" y cola "ms-patients-queue" (y bindings) ya existen en ms-infrastructure; no declarar.

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += (_, ea) => ProcessMessageAsync(ea);

                _channel.BasicConsume(
                    _options.FoodPlansQueue,
                    autoAck: false,
                    consumer);

                _logger.LogInformation(
                    "Meal-plan event consumer started (ms-infrastructure). Queue: {Queue}, Exchange: {Exchange}",
                    _options.FoodPlansQueue, _options.FoodPlansExchange);
                await Task.Delay(Timeout.Infinite, stoppingToken);
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                if (ex is BrokerUnreachableException)
                {
                    _logger.LogWarning(
                        "RabbitMQ is unreachable on {Host}:{Port} (attempt {Attempt}). Consumer will retry in {Delay}s.",
                        _options.HostName,
                        _options.Port,
                        attempt + 1,
                        _options.ReconnectDelaySeconds);
                }
                else
                {
                    _logger.LogError(ex, "Failed to start RabbitMQ FoodPlan consumer (attempt {Attempt})", attempt + 1);
                }
                _channel?.Dispose();
                _connection?.Dispose();
                _channel = null;
                _connection = null;
                attempt++;
                await Task.Delay(delayMs, stoppingToken);
            }
        }
    }

    private async Task ProcessMessageAsync(BasicDeliverEventArgs ea)
    {
        var routingKey = ea.RoutingKey;
        var isCreated = string.Equals(routingKey, _options.FoodPlanCreatedRoutingKey, StringComparison.Ordinal);
        var body = ea.Body.ToArray();
        var json = Encoding.UTF8.GetString(body);
        var parentContext = PatientTelemetry.Propagator.Extract(default, ea.BasicProperties, ExtractTraceContextFromBasicProperties);
        Baggage.Current = parentContext.Baggage;

        using var activity = PatientTelemetry.ActivitySource.StartActivity(
            "rabbitmq consume foodplan event",
            ActivityKind.Consumer,
            parentContext.ActivityContext);
        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination.name", _options.FoodPlansExchange);
        activity?.SetTag("messaging.rabbitmq.routing_key", routingKey);

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

    private static IEnumerable<string> ExtractTraceContextFromBasicProperties(IBasicProperties? properties, string key)
    {
        if (properties?.Headers is null || !properties.Headers.TryGetValue(key, out var value) || value is null)
            return [];

        return value switch
        {
            byte[] bytes => [Encoding.UTF8.GetString(bytes)],
            string text => [text],
            _ => [value.ToString() ?? string.Empty]
        };
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
