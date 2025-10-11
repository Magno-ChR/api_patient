using patient.domain.Abstractions;
using System.Threading.Tasks;

namespace patient.domain.Entities.Histories
{
    internal interface IHistoryRepository : IRepository<History>
    {
        Task UpdateAsync(History history);
    }
}