using MediatR;
using patient.application.Patients.CreatePatient;
using patient.domain.Abstractions;
using patient.domain.Entities.Patients;
using patient.domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.application.Patients.DeletePatient;

public class DeletePatientHandler : IRequestHandler<DeletePatientContactCommand, Result<Guid>>
{

    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePatientHandler(IPatientRepository patientRepository, IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Result<Guid>> Handle(DeletePatientContactCommand request, CancellationToken cancellationToken)
    {
        // Cargar el agregado (no readonly porque vamos a modificarlo)
        var patient = await _patientRepository.GetByIdAsync(request.PatientId, readOnly: false);
        if (patient is null)
            return Result.Failure<Guid>(Error.NotFound("Patient.NotFound", "Paciente no encontrado"));

        // Delegar la eliminación del contacto al agregado para mantener invariantes
        patient.RemoveContact(request.ContactId, request.PatientId);

        // Persistir el agregado
        await _patientRepository.UpdateAsync(patient);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(request.ContactId);
    }
}
