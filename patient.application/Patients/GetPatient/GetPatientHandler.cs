using MediatR;
using patient.domain.Abstractions;
using patient.domain.Entities.Patients;
using patient.domain.Results;
using patient.domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.application.Patients.GetPatient;

public class GetPatientHandler : IRequestHandler<GetPatientCommand, Result<Patient>>,
    IRequestHandler<GetPatientListCommand, Result<PagedResult<Patient>>>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public GetPatientHandler(IPatientRepository patientRepository, IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Patient>> Handle(GetPatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.PatientId, readOnly: true);
        if (patient is null)
            return Result.Failure<Patient>(Error.NotFound("Patient.NotFound", "Paciente no encontrado"));

        return Result.Success(patient);
    }

    public async Task<Result<PagedResult<Patient>>> Handle(GetPatientListCommand request, CancellationToken cancellationToken)
    {

        var results = await _patientRepository.GetPagedAsync(request.page, request.pageSize, request.search);

        if (results.Items.Count() < 1)
            return Result.Failure<PagedResult<Patient>>(Error.NotFound("Patient.NotFound", "Paciente no encontrado"));

        return Result.Success(results);
    }
}
