using System.Text.Json.Serialization;

namespace patient.infrastructure.Integration;

/// <summary>DTO del evento de integración (foodplan.created / foodplan.updated). Compatible con el payload del otro servicio.</summary>
internal sealed class FoodPlanIntegrationEventDto
{
    [JsonPropertyName("FoodPlanId")]
    public Guid FoodPlanId { get; set; }

    [JsonPropertyName("Id")]
    public Guid Id { get; set; }

    [JsonPropertyName("OccurredOn")]
    public DateTime OccurredOn { get; set; }

    [JsonPropertyName("Name")]
    public string? Name { get; set; }
}
