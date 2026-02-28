using patient.domain.Shared;

namespace patient.domain.Entities.Patients;

/// <summary>
/// Datos del paciente para el outbox (sin la lista de contactos).
/// </summary>
public record PatientOutboxPayload(
    Guid Id,
    string FirstName,
    string MiddleName,
    string LastName,
    BloodType BloodType,
    string DocumentNumber,
    DateOnly DateOfBirth,
    string Ocupation,
    string Religion,
    string Alergies);
