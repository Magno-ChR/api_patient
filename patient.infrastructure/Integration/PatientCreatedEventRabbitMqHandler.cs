using MediatR;
using Microsoft.Extensions.Logging;
using patient.domain.Entities.Patients.Events;

namespace patient.infrastructure.Integration;

/// <summary>Cuando el outbox despacha PatientCreatedEvent, publica a RabbitMQ (exchange patients, routing key patient.created).</summary>
internal sealed class PatientCreatedEventRabbitMqHandler : INotificationHandler<PatientCreatedEvent>
{
    private readonly IPatientEventPublisher _publisher;
    private readonly ILogger<PatientCreatedEventRabbitMqHandler> _logger;

    public PatientCreatedEventRabbitMqHandler(
        IPatientEventPublisher publisher,
        ILogger<PatientCreatedEventRabbitMqHandler> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Handle(PatientCreatedEvent notification, CancellationToken cancellationToken)
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
        _logger.LogInformation("PatientCreatedEvent received by RabbitMQ handler, publishing to exchange 'patients' with key 'patient.created'. PatientId: {PatientId}", dto.PatientId);
        await _publisher.PublishCreatedAsync(dto, cancellationToken);
    }
}
