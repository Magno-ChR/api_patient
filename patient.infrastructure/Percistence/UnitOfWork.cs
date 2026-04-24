using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using patient.domain.Abstractions;
using patient.domain.Entities.Patients.Events;
using patient.infrastructure.Integration;
using patient.infrastructure.Percistence.DomainModel;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.infrastructure.Percistence;

internal class UnitOfWork : IUnitOfWork
{
    private readonly DomainDbContext context;
    private readonly IMediator mediator;
    private readonly IPatientEventPublisher patientEventPublisher;
    private readonly ILogger<UnitOfWork> logger;

    public UnitOfWork(
        DomainDbContext context,
        IMediator mediator,
        IPatientEventPublisher patientEventPublisher,
        ILogger<UnitOfWork> logger)
    {
        this.context = context;
        this.mediator = mediator;
        this.patientEventPublisher = patientEventPublisher;
        this.logger = logger;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = context.ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x =>
            {
                var domainEvents = x.Entity
                                .DomainEvents
                                .ToImmutableArray();
                x.Entity.ClearDomainEvents();

                return domainEvents;
            })
            .SelectMany(domainEvents => domainEvents)
            .ToList();

        logger.LogInformation("UnitOfWork collected {DomainEventsCount} domain events before commit", domainEvents.Count);
        var patientEventsToPublish = new List<(PatientEventPublishedDto Payload, bool IsCreated)>();

        // Publish internal domain events. Patient integration events are sent to RabbitMQ
        // only after the DB transaction succeeds.
        foreach (var domainEvent in domainEvents)
        {
            if (domainEvent is PatientCreatedEvent created)
            {
                patientEventsToPublish.Add((ToPublishedDto(created), true));
                logger.LogInformation(
                    "PatientCreatedEvent queued for direct RabbitMQ publication. EventId: {EventId}, PatientId: {PatientId}",
                    created.Id,
                    created.Patient.Id);
                continue;
            }

            if (domainEvent is PatientUpdatedEvent updated)
            {
                patientEventsToPublish.Add((ToPublishedDto(updated), false));
                logger.LogInformation(
                    "PatientUpdatedEvent queued for direct RabbitMQ publication. EventId: {EventId}, PatientId: {PatientId}",
                    updated.Id,
                    updated.Patient.Id);
                continue;
            }

            logger.LogInformation(
                "Publishing domain event through MediatR. EventType: {EventType}, EventId: {EventId}",
                domainEvent.GetType().FullName,
                domainEvent.Id);
            await mediator.Publish(domainEvent, cancellationToken);
        }


        await context.SaveChangesAsync(cancellationToken);

        foreach (var patientEvent in patientEventsToPublish)
        {
            if (patientEvent.IsCreated)
            {
                await patientEventPublisher.PublishCreatedAsync(patientEvent.Payload, cancellationToken);
                continue;
            }

            await patientEventPublisher.PublishUpdatedAsync(patientEvent.Payload, cancellationToken);
        }

        logger.LogInformation("UnitOfWork commit completed successfully");
    }

    private static PatientEventPublishedDto ToPublishedDto(DomainEvent domainEvent)
    {
        if (domainEvent is not PatientCreatedEvent and not PatientUpdatedEvent)
            throw new InvalidOperationException($"Unsupported patient integration event type: {domainEvent.GetType().FullName}");

        var patient = domainEvent switch
        {
            PatientCreatedEvent created => created.Patient,
            PatientUpdatedEvent updated => updated.Patient,
            _ => throw new InvalidOperationException($"Unsupported patient integration event type: {domainEvent.GetType().FullName}")
        };

        return new PatientEventPublishedDto
        {
            PatientId = patient.Id,
            Id = domainEvent.Id,
            OccurredOn = domainEvent.OccurredOn,
            FirstName = patient.FirstName,
            MiddleName = patient.MiddleName,
            LastName = patient.LastName,
            DocumentNumber = patient.DocumentNumber
        };
    }
}
