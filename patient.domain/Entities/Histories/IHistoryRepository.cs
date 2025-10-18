using patient.domain.Abstractions;
using System.Threading.Tasks;

namespace patient.domain.Entities.Histories
{
    public interface IHistoryRepository : IRepository<History>
    {
        Task UpdateAsync(History history);
    }
}