using Microsoft.EntityFrameworkCore;
using patient.domain.Entities.FoodPlans;
using patient.domain.Entities.Patients;
using patient.infrastructure.Percistence.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.infrastructure.Percistence.Repositories;

internal class FoodPlanRepository : IFoodPlanRepository
{
    private readonly DomainDbContext context;

    public FoodPlanRepository(DomainDbContext context)
    {
        this.context = context;
    }

    public async Task AddAsync(FoodPlan entity)
    {
        await context.FoodPlans.AddAsync(entity);
    }

    public async Task<FoodPlan?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        if (readOnly)
        {
            return await context.FoodPlans.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        }
        else
        {
            return await context.FoodPlans.FindAsync(id);
        }
    }

}
