using System.Text.Json.Serialization;

namespace patient.infrastructure.Integration;

/// <summary>DTO del evento de integración (meal-plan.created / meal-plan.updated).
/// Soporta tanto el contrato anterior como el payload real publicado por el servicio de planes.</summary>
internal sealed class FoodPlanIntegrationEventDto
{
    [JsonPropertyName("PlanId")]
    public Guid PlanId { get; set; }

    [JsonPropertyName("FoodPlanId")]
    public Guid FoodPlanId { get; set; }

    [JsonPropertyName("PacienteId")]
    public Guid PatientId { get; set; }

    [JsonPropertyName("NutricionistaId")]
    public Guid NutritionistId { get; set; }

    [JsonPropertyName("FechaInicio")]
    public DateTime? StartDate { get; set; }

    [JsonPropertyName("Duracion")]
    public int? DurationDays { get; set; }

    [JsonPropertyName("Dietas")]
    public List<object>? Diets { get; set; }

    [JsonPropertyName("Id")]
    public Guid Id { get; set; }

    [JsonPropertyName("OccurredOn")]
    public DateTime? OccurredOn { get; set; }

    [JsonPropertyName("CreatedAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("CorrelationId")]
    public string? CorrelationId { get; set; }

    [JsonPropertyName("Source")]
    public string? Source { get; set; }

    public Guid EffectiveFoodPlanId => PlanId != Guid.Empty ? PlanId : FoodPlanId;

    public int DietCount => Diets?.Count ?? 0;

    public string ToFoodPlanName()
    {
        if (!string.IsNullOrWhiteSpace(Name))
            return TrimToMaxLength(Name.Trim());

        var parts = new List<string>();
        if (StartDate.HasValue)
            parts.Add($"Inicio {StartDate:yyyy-MM-dd}");

        if (DurationDays is > 0)
            parts.Add($"{DurationDays} dias");

        parts.Add($"{DietCount} dietas");

        if (parts.Count == 0)
            parts.Add($"Plan {EffectiveFoodPlanId}");

        return TrimToMaxLength(string.Join(" | ", parts));
    }

    private static string TrimToMaxLength(string value, int maxLength = 100) =>
        value.Length <= maxLength ? value : value[..maxLength];
}
