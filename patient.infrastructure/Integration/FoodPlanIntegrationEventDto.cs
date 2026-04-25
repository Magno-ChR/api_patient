using System.Text.Json.Serialization;

namespace patient.infrastructure.Integration;

/// <summary>DTO del evento de integración del servicio de planes.
/// Soporta el payload real publicado por meal-plans y un formato legado con FoodPlanId/Name.</summary>
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

    [JsonPropertyName("Requerido")]
    public bool? IsRequired { get; set; }

    [JsonPropertyName("Dietas")]
    public List<FoodPlanDietDto>? Diets { get; set; }

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

    public int RecipeCount => Diets?.Sum(diet => diet.Recipes?.Count ?? 0) ?? 0;

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
        if (RecipeCount > 0)
            parts.Add($"{RecipeCount} recetas");
        if (IsRequired.HasValue)
            parts.Add(IsRequired.Value ? "requerido" : "opcional");

        if (parts.Count == 0)
            parts.Add($"Plan {EffectiveFoodPlanId}");

        return TrimToMaxLength(string.Join(" | ", parts));
    }

    private static string TrimToMaxLength(string value, int maxLength = 100) =>
        value.Length <= maxLength ? value : value[..maxLength];
}

internal sealed class FoodPlanDietDto
{
    [JsonPropertyName("DietaId")]
    public Guid DietId { get; set; }

    [JsonPropertyName("FechaConsumo")]
    public DateTime? ConsumptionDate { get; set; }

    [JsonPropertyName("Recetas")]
    public List<FoodPlanRecipeDto>? Recipes { get; set; }
}

internal sealed class FoodPlanRecipeDto
{
    [JsonPropertyName("RecetaId")]
    public Guid RecipeId { get; set; }

    [JsonPropertyName("Orden")]
    public int? Order { get; set; }

    [JsonPropertyName("TiempoId")]
    public int? TimeId { get; set; }
}
