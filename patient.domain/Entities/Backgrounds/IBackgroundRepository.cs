using System.Threading.Tasks;

namespace patient.domain.Entities.Backgrounds
{
    internal interface IBackgroundRepository
    {
        Task UpdateAsync(Background background);
        Task DeleteAsync(Background background);
    }
}