using System.Threading.Tasks;

namespace patient.domain.Entities.Evolutions
{
    internal interface IEvolutionRepository
    {
        Task UpdateAsync(Evolution evolution);
        Task DeleteAsync(Evolution evolution);
    }
}