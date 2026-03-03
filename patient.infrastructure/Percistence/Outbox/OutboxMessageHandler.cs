using Joseco.Outbox.Contracts.Model;
using MediatR;
using Microsoft.Extensions.Logging;
using patient.domain.Abstractions;
using patient.domain.Entities.Patients.Events;
using patient.infrastructure.Integration;

namespace patient.infrastructure.Percistence.Outbox;

public class OutboxMessageHandler : INotificationHandler<OutboxMessage<DomainEvent>>
{
    private readonly ILogger<OutboxMessageHandler> _logger;
    private readonly IPatientEventPublisher _patientEventPublisher;

    public OutboxMessageHandler(
        ILogger<OutboxMessageHandler> logger,
        IPatientEventPublisher patientEventPublisher)
    {
        _logger = logger;
        _patientEventPublisher = patientEventPublisher;
    }

    public async Task Handle(OutboxMessage<DomainEvent> notification, CancellationToken cancellationToken)
    {
        var content = notification.Content;
        _logger.LogInformation(
            "OutboxMessageHandler.Handle invoked. Content type: {ContentType}, Content is null: {ContentIsNull}",
            content?.GetType().FullName ?? "(null)",
            content is null);

        switch (content)
        {
            case PatientCreatedEvent created:
                _logger.LogInformation(
                    "Outbox: Paciente creado. Id: {Id}, DocumentNumber: {DocumentNumber}, Nombre: {FirstName} {LastName}",
                    created.Patient.Id, created.Patient.DocumentNumber, created.Patient.FirstName, created.Patient.LastName);
                await PublishPatientEventAsyncSafe(created, created.Patient, isCreated: true, cancellationToken);
                break;
            case PatientUpdatedEvent updated:
                _logger.LogInformation(
                    "Outbox: Paciente actualizado. Id: {Id}, DocumentNumber: {DocumentNumber}, Nombre: {FirstName} {LastName}",
                    updated.Patient.Id, updated.Patient.DocumentNumber, updated.Patient.FirstName, updated.Patient.LastName);
                await PublishPatientEventAsyncSafe(updated, updated.Patient, isCreated: false, cancellationToken);
                break;
            default:
                _logger.LogWarning(
                    "Outbox: Evento no es PatientCreatedEvent ni PatientUpdatedEvent (no se publica a RabbitMQ). Tipo: {Type}",
                    content?.GetType().Name ?? "null");
                break;
        }
    }

    private static PatientEventPublishedDto ToPublishedDto(DomainEvent evt, patient.domain.Entities.Patients.PatientOutboxPayload patient)
    {
        return new PatientEventPublishedDto
        {
            PatientId = patient.Id,
            Id = evt.Id,
            OccurredOn = evt.OccurredOn,
            FirstName = patient.FirstName,
            MiddleName = patient.MiddleName,
            LastName = patient.LastName,
            DocumentNumber = patient.DocumentNumber
        };
    }

    private async Task PublishPatientEventAsyncSafe(DomainEvent evt, patient.domain.Entities.Patients.PatientOutboxPayload patient, bool isCreated, CancellationToken cancellationToken)
    {
        try
        {
            var dto = ToPublishedDto(evt, patient);
            if (isCreated)
                await _patientEventPublisher.PublishCreatedAsync(dto, cancellationToken);
            else
                await _patientEventPublisher.PublishUpdatedAsync(dto, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al publicar evento de paciente a RabbitMQ. PatientId: {PatientId}, IsCreated: {IsCreated}", patient.Id, isCreated);
            throw;
        }
    }
}
