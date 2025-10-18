using patient.domain.Abstractions;
using System.Threading.Tasks;

namespace patient.domain.Entities.FoodPlans
{
    public interface IFoodPlanRepository : IRepository<FoodPlan>
    {
    }
}