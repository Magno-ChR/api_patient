using patient.domain.Abstractions;

namespace patient.domain.Entities.FoodPlans;

public class FoodPlan : AggregateRoot
{
    public Guid PatientId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private FoodPlan() : base() { }

    private FoodPlan(Guid id, Guid patientId, string name) : base(id)
    {
        PatientId = patientId;
        Name = name ?? string.Empty;
    }

    /// <summary>Crear plan alimenticio (p. ej. desde evento de integración foodplan.created).</summary>
    public static FoodPlan Create(Guid id, string name) =>
        Create(id, Guid.Empty, name);

    /// <summary>Crear plan alimenticio asociado a un paciente.</summary>
    public static FoodPlan Create(Guid id, Guid patientId, string name)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("El id del plan no puede estar vacío", nameof(id));

        return new(id, patientId, name ?? string.Empty);
    }

    /// <summary>Actualizar datos desde evento de integración foodplan.updated.</summary>
    public void UpdateDetails(string name) =>
        UpdateDetails(name, PatientId);

    /// <summary>Actualizar datos del plan y el paciente asociado desde integración.</summary>
    public void UpdateDetails(string name, Guid patientId)
    {
        PatientId = patientId;
        Name = name ?? string.Empty;
    }
}
