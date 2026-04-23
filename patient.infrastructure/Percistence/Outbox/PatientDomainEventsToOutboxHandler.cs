using Joseco.Outbox.Contracts.Model;
using MediatR;
using Microsoft.Extensions.Logging;
using patient.domain.Abstractions;
using patient.domain.Entities.Patients.Events;
using patient.infrastructure.Percistence.DomainModel;

namespace patient.infrastructure.Percistence.Outbox;

/// <summary>
/// Convierte eventos de dominio de paciente en mensajes de outbox.
/// Se ejecuta dentro del UnitOfWork.CommitAsync, en la misma transacción
/// que persiste los cambios de dominio.
/// </summary>
public sealed class PatientDomainEventsToOutboxHandler :
    INotificationHandler<PatientCreatedEvent>,
    INotificationHandler<PatientUpdatedEvent>
{
    private readonly DomainDbContext _dbContext;
    private readonly ILogger<PatientDomainEventsToOutboxHandler> _logger;

    public PatientDomainEventsToOutboxHandler(
        DomainDbContext dbContext,
        ILogger<PatientDomainEventsToOutboxHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public Task Handle(PatientCreatedEvent notification, CancellationToken cancellationToken)
    {
        _dbContext.OutboxMessages.Add(new OutboxMessage<DomainEvent>(notification));
        _logger.LogInformation(
            "PatientCreatedEvent persisted to outbox. EventId: {EventId}, PatientId: {PatientId}",
            notification.Id,
            notification.Patient.Id);
        return Task.CompletedTask;
    }

    public Task Handle(PatientUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _dbContext.OutboxMessages.Add(new OutboxMessage<DomainEvent>(notification));
        _logger.LogInformation(
            "PatientUpdatedEvent persisted to outbox. EventId: {EventId}, PatientId: {PatientId}",
            notification.Id,
            notification.Patient.Id);
        return Task.CompletedTask;
    }
}

