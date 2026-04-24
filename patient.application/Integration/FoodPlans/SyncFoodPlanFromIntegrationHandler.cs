using MediatR;
using Microsoft.Extensions.Logging;
using patient.domain.Abstractions;
using patient.domain.Entities.FoodPlans;

namespace patient.application.Integration.FoodPlans;

internal sealed class SyncFoodPlanFromIntegrationHandler : IRequestHandler<SyncFoodPlanFromIntegrationCommand, Unit>
{
    private readonly IFoodPlanRepository _foodPlanRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SyncFoodPlanFromIntegrationHandler> _logger;

    public SyncFoodPlanFromIntegrationHandler(
        IFoodPlanRepository foodPlanRepository,
        IUnitOfWork unitOfWork,
        ILogger<SyncFoodPlanFromIntegrationHandler> logger)
    {
        _foodPlanRepository = foodPlanRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(SyncFoodPlanFromIntegrationCommand request, CancellationToken cancellationToken)
    {
        var name = request.Name ?? string.Empty;
        var patientId = request.PatientId;

        if (request.IsCreated)
        {
            var existing = await _foodPlanRepository.GetByIdAsync(request.FoodPlanId, readOnly: true);
            if (existing is not null)
            {
                _logger.LogInformation("FoodPlan {FoodPlanId} already exists, skipping create", request.FoodPlanId);
                return Unit.Value;
            }
            var foodPlan = FoodPlan.Create(request.FoodPlanId, patientId, name);
            await _foodPlanRepository.AddAsync(foodPlan);
            _logger.LogInformation("FoodPlan created from integration: {FoodPlanId}, PatientId: {PatientId}", request.FoodPlanId, patientId);
        }
        else
        {
            var foodPlan = await _foodPlanRepository.GetByIdAsync(request.FoodPlanId);
            if (foodPlan is null)
            {
                _logger.LogWarning("FoodPlan {FoodPlanId} not found for update, creating as new", request.FoodPlanId);
                foodPlan = FoodPlan.Create(request.FoodPlanId, patientId, name);
                await _foodPlanRepository.AddAsync(foodPlan);
            }
            else
            {
                foodPlan.UpdateDetails(name, patientId);
            }
            _logger.LogInformation("FoodPlan updated from integration: {FoodPlanId}, PatientId: {PatientId}", request.FoodPlanId, patientId);
        }

        await _unitOfWork.CommitAsync(cancellationToken);
        return Unit.Value;
    }
}
