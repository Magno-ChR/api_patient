using MediatR;
using Microsoft.EntityFrameworkCore;
using patient.domain.Abstractions;
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
    private readonly ILogger<UnitOfWork> logger;

    public UnitOfWork(DomainDbContext context, IMediator mediator, ILogger<UnitOfWork> logger)
    {
        this.context = context;
        this.mediator = mediator;
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

        //Publish Domain Events
        foreach (var domainEvent in domainEvents)
        {
            logger.LogInformation(
                "Publishing domain event through MediatR. EventType: {EventType}, EventId: {EventId}",
                domainEvent.GetType().FullName,
                domainEvent.Id);
            await mediator.Publish(domainEvent, cancellationToken);
        }


        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("UnitOfWork commit completed successfully");
    }
}
