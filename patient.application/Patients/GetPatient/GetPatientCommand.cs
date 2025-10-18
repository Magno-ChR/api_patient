using MediatR;
using patient.domain.Entities.Patients;
using patient.domain.Results;


namespace patient.application.Patients.GetPatient;

public record GetPatientCommand(Guid PatientId) : IRequest<Result<Patient>>;
