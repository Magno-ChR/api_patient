using patient.domain.Abstractions;
using patient.domain.Entities.Patients;

namespace patient.domain.Entities.Patients.Events;

public record PatientUpdatedEvent(PatientOutboxPayload Patient) : DomainEvent;
