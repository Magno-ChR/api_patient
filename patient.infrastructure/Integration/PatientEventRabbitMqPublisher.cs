using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace patient.infrastructure.Integration;

/// <summary>Publica eventos patient.created y patient.updated al exchange "patients" (Topic, durable) para api_security.</summary>
internal sealed class PatientEventRabbitMqPublisher : IPatientEventPublisher
{
    // PascalCase; DateTime se serializa en ISO 8601 por defecto (ej. "2026-02-28T14:27:24.0952892Z")
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = null,
        WriteIndented = false
    };

    private readonly RabbitMqPatientPublisherOptions _options;
    private readonly ILogger<PatientEventRabbitMqPublisher> _logger;

    public PatientEventRabbitMqPublisher(
        IOptions<RabbitMqPatientPublisherOptions> options,
        ILogger<PatientEventRabbitMqPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task PublishCreatedAsync(PatientEventPublishedDto payload, CancellationToken cancellationToken = default) =>
        PublishAsync(_options.PatientCreatedRoutingKey, payload, cancellationToken);

    public Task PublishUpdatedAsync(PatientEventPublishedDto payload, CancellationToken cancellationToken = default) =>
        PublishAsync(_options.PatientUpdatedRoutingKey, payload, cancellationToken);

    private Task PublishAsync(string routingKey, PatientEventPublishedDto payload, CancellationToken cancellationToken)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            // Exchange "patients" ya existe en ms-infrastructure (definitions.json); no declarar.

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(
                exchange: _options.PatientsExchange,
                routingKey: routingKey,
                basicProperties: null,
                body: body);

            _logger.LogInformation(
                "Patient event published to RabbitMQ. Exchange: {Exchange}, RoutingKey: {RoutingKey}, PatientId: {PatientId}, OccurredOn: {OccurredOn:O}",
                _options.PatientsExchange, routingKey, payload.PatientId, payload.OccurredOn);
            _logger.LogDebug("Patient event payload (JSON): {Json}", json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to publish patient event. RoutingKey: {RoutingKey}, PatientId: {PatientId}",
                routingKey, payload.PatientId);
            throw;
        }

        return Task.CompletedTask;
    }
}
