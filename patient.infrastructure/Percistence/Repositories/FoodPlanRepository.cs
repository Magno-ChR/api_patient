using patient.domain.Entities.FoodPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.infrastructure.Percistence.Repositories;

internal class FoodPlanRepository : IFoodPlanRepository
{
    public Task AddAsync(FoodPlan entity)
    {
       return Task.CompletedTask;
    }
    public Task<FoodPlan?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        return null;
    }

}
