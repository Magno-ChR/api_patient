using MediatR;
using patient.domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.application.FoodPlans.CreateFoodPlan;

public record CreateFoodPlanCommand(string Name): IRequest<Result<Guid>>;
