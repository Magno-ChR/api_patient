using patient.domain.Entities.Histories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.infrastructure.Percistence.Repositories;

internal class HistoryRepository : IHistoryRepository
{
    public Task AddAsync(History entity)
    {
       return Task.CompletedTask;
    }
    public Task<History?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        return null;
    }
    public Task UpdateAsync(History history)
    {
        return Task.CompletedTask;
    }
}
