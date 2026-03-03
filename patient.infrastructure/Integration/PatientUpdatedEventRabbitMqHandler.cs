using MediatR;
using Microsoft.Extensions.Logging;
using patient.domain.Entities.Patients.Events;

namespace patient.infrastructure.Integration;

/// <summary>Cuando el outbox despacha PatientUpdatedEvent, publica a RabbitMQ (exchange patients, routing key patient.updated).</summary>
internal sealed class PatientUpdatedEventRabbitMqHandler : INotificationHandler<PatientUpdatedEvent>
{
    private readonly IPatientEventPublisher _publisher;
    private readonly ILogger<PatientUpdatedEventRabbitMqHandler> _logger;

    public PatientUpdatedEventRabbitMqHandler(
        IPatientEventPublisher publisher,
        ILogger<PatientUpdatedEventRabbitMqHandler> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(PatientUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var dto = new PatientEventPublishedDto
        {
            PatientId = notification.Patient.Id,
            Id = notification.Id,
            OccurredOn = notification.OccurredOn,
            FirstName = notification.Patient.FirstName,
            MiddleName = notification.Patient.MiddleName,
            LastName = notification.Patient.LastName,
            DocumentNumber = notification.Patient.DocumentNumber
        };
        _logger.LogInformation("PatientUpdatedEvent received by RabbitMQ handler, publishing to exchange 'patients' with key 'patient.updated'. PatientId: {PatientId}", dto.PatientId);
        await _publisher.PublishUpdatedAsync(dto, cancellationToken);
    }
}
