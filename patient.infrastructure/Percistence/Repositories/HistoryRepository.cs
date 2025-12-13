using Microsoft.EntityFrameworkCore;
using patient.domain.Abstractions;
using patient.domain.Entities.Backgrounds;
using patient.domain.Entities.Evolutions;
using patient.domain.Entities.Histories;
using patient.domain.Entities.Histories.Events;
using patient.domain.Entities.Patients;
using patient.domain.Entities.Patients.Events;
using patient.infrastructure.Percistence.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.infrastructure.Percistence.Repositories;

internal class HistoryRepository : IHistoryRepository
{
    private readonly DomainDbContext context;

    public HistoryRepository(DomainDbContext context)
    {
        this.context = context;
    }

    public async Task AddAsync(History entity)
    {
        await context.Histories.AddAsync(entity);
    }


    public async Task<History?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        if (readOnly)
        {
            return await context.Histories
                .AsNoTracking()
                .Include("_backgrounds")
                .Include("_evolutions")
                .FirstOrDefaultAsync(i => i.Id == id);
        }
        else
        {
            return await context.Histories
                .Include("_backgrounds")
                .Include("_evolutions")
                .FirstOrDefaultAsync(i => i.Id == id);
        }
    }

    public Task UpdateAsync(History entity)
    {
        var addedBackgrounds = entity.DomainEvents.Where(x => x is BackgroundCreateEvent)
           .Select(e => (BackgroundCreateEvent)e).ToList();

        var addedEvolutions = entity.DomainEvents.Where(x => x is EvolutionCreateEvent)
           .Select(e => (EvolutionCreateEvent)e).ToList();

        foreach (var domainEvent in addedBackgrounds)
        {
            var itemToAdd = entity.Backgrounds.First(c => c.Id == domainEvent.BackgroundId);
            context.Backgrounds.Add(itemToAdd);
        }

        foreach (var domainEvent in addedEvolutions)
        {
            var itemToAdd = entity.Evolutions.First(c => c.Id == domainEvent.EvolutionId);
            context.Evolutions.Add(itemToAdd);
        }

        context.Histories.Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Background entity)
    {
        context.Backgrounds.Remove(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Evolution entity)
    {
        context.Evolutions.Remove(entity);
        return Task.CompletedTask;
    }
}
