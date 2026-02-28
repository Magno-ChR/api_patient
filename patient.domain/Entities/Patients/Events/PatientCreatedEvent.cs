using patient.domain.Abstractions;
using patient.domain.Entities.Patients;

namespace patient.domain.Entities.Patients.Events;

public record PatientCreatedEvent(PatientOutboxPayload Patient) : DomainEvent;
