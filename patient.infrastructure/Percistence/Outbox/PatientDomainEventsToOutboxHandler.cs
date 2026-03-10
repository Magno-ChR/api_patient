using Joseco.Outbox.Contracts.Model;
using MediatR;
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

    public PatientDomainEventsToOutboxHandler(DomainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task Handle(PatientCreatedEvent notification, CancellationToken cancellationToken)
    {
        _dbContext.OutboxMessages.Add(new OutboxMessage<DomainEvent>(notification));
        return Task.CompletedTask;
    }

    public Task Handle(PatientUpdatedEvent notification, CancellationToken cancellationToken)
    {
        _dbContext.OutboxMessages.Add(new OutboxMessage<DomainEvent>(notification));
        return Task.CompletedTask;
    }
}

