using MediatR;
using patient.domain.Results;
using patient.domain.Shared;

namespace patient.application.Patients.CreatePatient;


public record CreatePatientCommand(Guid Id, string FirstName, string MiddleName, string LastName, BloodType BloodType, string DocumentNumber, DateOnly DateOfBirth, string Ocupation, string Religion, string Alergies) : IRequest<Result<Guid>>;
