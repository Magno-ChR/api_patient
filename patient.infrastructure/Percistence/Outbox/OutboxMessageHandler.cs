using Joseco.Outbox.Contracts.Model;
using MediatR;
using Microsoft.Extensions.Logging;
using patient.domain.Abstractions;
using patient.domain.Entities.Patients.Events;

namespace patient.infrastructure.Percistence.Outbox;

public class OutboxMessageHandler : INotificationHandler<OutboxMessage<DomainEvent>>
{
    private readonly ILogger<OutboxMessageHandler> _logger;

    public OutboxMessageHandler(ILogger<OutboxMessageHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OutboxMessage<DomainEvent> notification, CancellationToken cancellationToken)
    {
        var content = notification.Content;
        switch (content)
        {
            case PatientCreatedEvent created:
                _logger.LogInformation(
                    "Outbox: Paciente creado. Id: {Id}, DocumentNumber: {DocumentNumber}, Nombre: {FirstName} {LastName}",
                    created.Patient.Id, created.Patient.DocumentNumber, created.Patient.FirstName, created.Patient.LastName);
                break;
            case PatientUpdatedEvent updated:
                _logger.LogInformation(
                    "Outbox: Paciente actualizado. Id: {Id}, DocumentNumber: {DocumentNumber}, Nombre: {FirstName} {LastName}",
                    updated.Patient.Id, updated.Patient.DocumentNumber, updated.Patient.FirstName, updated.Patient.LastName);
                break;
            default:
                _logger.LogInformation("Outbox: Evento de dominio procesado. Tipo: {Type}", content?.GetType().Name ?? "null");
                break;
        }

        return Task.CompletedTask;
    }
}
