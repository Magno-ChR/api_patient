using System.Text.Json.Serialization;

namespace patient.infrastructure.Integration;

/// <summary>Payload publicado a RabbitMQ para patient.created / patient.updated (contrato con api_security). PascalCase.</summary>
public sealed class PatientEventPublishedDto
{
    [JsonPropertyName("PatientId")]
    public Guid PatientId { get; set; }

    [JsonPropertyName("Id")]
    public Guid Id { get; set; }

    [JsonPropertyName("OccurredOn")]
    public DateTime OccurredOn { get; set; }

    [JsonPropertyName("FirstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("MiddleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("LastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("DocumentNumber")]
    public string? DocumentNumber { get; set; }
}
