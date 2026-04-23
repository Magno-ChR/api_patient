using patient.domain.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace patient.domain.Entities.Histories
{
    public interface IHistoryRepository : IRepository<History>
    {
        Task<List<History>> GetByPatientIdAsync(Guid patientId, bool readOnly = false);
        Task UpdateAsync(History history);
    }
}