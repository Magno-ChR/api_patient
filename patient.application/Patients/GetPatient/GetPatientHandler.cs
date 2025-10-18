using MediatR;
using patient.domain.Abstractions;
using patient.domain.Entities.Patients;
using patient.domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.application.Patients.GetPatient;

internal class GetPatientHandler : IRequestHandler<GetPatientCommand, Result<Patient>>
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
}
