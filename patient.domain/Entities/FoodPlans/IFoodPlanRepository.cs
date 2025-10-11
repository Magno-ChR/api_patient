using patient.domain.Abstractions;
using System.Threading.Tasks;

namespace patient.domain.Entities.FoodPlans
{
    internal interface IFoodPlanRepository : IRepository<FoodPlan>
    {
    }
}