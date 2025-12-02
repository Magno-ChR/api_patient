using MediatR;
using patient.domain.Entities.Patients;
using patient.domain.Results;
using patient.domain.Shared;


namespace patient.application.Patients.GetPatient;

public record GetPatientCommand(Guid PatientId) : IRequest<Result<Patient>>;

public record GetPatientListCommand(int page, int pageSize, string? search = null) : IRequest<Result<PagedResult<Patient>>>;