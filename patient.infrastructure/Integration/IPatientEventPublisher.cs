namespace patient.infrastructure.Integration;

/// <summary>Publica eventos de paciente (created/updated) a RabbitMQ para que api_security los consuma.</summary>
public interface IPatientEventPublisher
{
    Task PublishCreatedAsync(PatientEventPublishedDto payload, CancellationToken cancellationToken = default);
    Task PublishUpdatedAsync(PatientEventPublishedDto payload, CancellationToken cancellationToken = default);
}
