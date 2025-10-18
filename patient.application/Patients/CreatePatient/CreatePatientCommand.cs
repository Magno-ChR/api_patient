using MediatR;
using patient.domain.Results;
using patient.domain.Shared;

namespace patient.application.Patients.CreatePatient;


public record CreatePatientCommand(string FirstName, string MiddleName, string LastName, BloodType BloodType, string DocumentNumber, 
    DateOnly DateOfBirth, string Ocupation, string Religion, string Alergies) : IRequest<Result<Guid>>;

public record CreatePatientContactCommand(Guid PatientId, string Direction, string PhoneNumber, 
    string Floor, string Coords, string? Reference) : IRequest<Result<Guid>>;

