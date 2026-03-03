namespace patient.infrastructure.Integration;

/// <summary>Opciones de RabbitMQ para publicar eventos de paciente (patient.created / patient.updated) hacia api_security.</summary>
public sealed class RabbitMqPatientPublisherOptions
{
    public const string SectionName = "RabbitMQ";

    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";

    public string PatientsExchange { get; set; } = "patients";
    public string PatientCreatedRoutingKey { get; set; } = "patient.created";
    public string PatientUpdatedRoutingKey { get; set; } = "patient.updated";
}
