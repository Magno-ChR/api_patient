using Microsoft.EntityFrameworkCore;
using patient.domain.Abstractions;
using patient.domain.Entities.Backgrounds;
using patient.domain.Entities.Evolutions;
using patient.domain.Entities.Histories;
using patient.domain.Entities.Patients;
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

    public async Task AddAsync(Background entity)
    {
        await context.Backgrounds.AddAsync(entity);
    }

    public async Task AddAsync(Evolution entity)
    {
        await context.Evolutions.AddAsync(entity);
    }

    public async Task<History?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        if (readOnly)
        {
            return await context.Histories.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        }
        else
        {
            return await context.Histories.FindAsync(id);
        }
    }

    public Task UpdateAsync(History entity)
    {
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
