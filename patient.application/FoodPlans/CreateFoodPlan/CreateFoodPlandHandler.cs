using MediatR;
using patient.domain.Abstractions;
using patient.domain.Entities.FoodPlans;
using patient.domain.Entities.Patients;
using patient.domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.application.FoodPlans.CreateFoodPlan;

internal class CreateFoodPlandHandler : IRequestHandler<CreateFoodPlanCommand, Result<Guid>>
{
    private readonly IFoodPlanRepository _foodPlanRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFoodPlandHandler(IFoodPlanRepository foodPlanRepository, IUnitOfWork unitOfWork)
    {
        _foodPlanRepository = foodPlanRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateFoodPlanCommand request, CancellationToken cancellationToken)
    {
        var item = new FoodPlan(
            Guid.NewGuid(),
            request.Name
        );
        await _foodPlanRepository.AddAsync(item);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(item.Id);
    }
}
