using patient.domain.Abstractions;

namespace patient.domain.Entities.FoodPlans;

public class FoodPlan : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;

    private FoodPlan() : base() { }

    private FoodPlan(Guid id, string name) : base(id)
    {
        Name = name ?? string.Empty;
    }

    /// <summary>Crear plan alimenticio (p. ej. desde evento de integración foodplan.created).</summary>
    public static FoodPlan Create(Guid id, string name) =>
        new(id, name ?? string.Empty);

    /// <summary>Actualizar datos desde evento de integración foodplan.updated.</summary>
    public void UpdateDetails(string name)
    {
        Name = name ?? string.Empty;
    }
}
