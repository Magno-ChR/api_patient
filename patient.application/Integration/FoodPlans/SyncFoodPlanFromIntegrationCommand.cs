using MediatR;

namespace patient.application.Integration.FoodPlans;

/// <summary>Comando para sincronizar un plan alimenticio desde eventos de integración (foodplan.created / foodplan.updated).</summary>
public sealed class SyncFoodPlanFromIntegrationCommand : IRequest<Unit>
{
    public Guid FoodPlanId { get; init; }
    public bool IsCreated { get; init; }
    public string? Name { get; init; }
}
