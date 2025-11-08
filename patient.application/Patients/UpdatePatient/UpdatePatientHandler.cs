using MediatR;
using patient.domain.Abstractions;
using patient.domain.Entities.Patients;
using patient.domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.application.Patients.UpdatePatient;

internal class UpdatePatientHandler : IRequestHandler<UpdatePatientCommand, Result<Guid>>,
    IRequestHandler<UpdatePatientContactCommand, Result<Guid>>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePatientHandler(IPatientRepository patientRepository, IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        // Cargar el agregado (no readonly porque vamos a modificarlo)
        var patient = await _patientRepository.GetByIdAsync(request.patientId, readOnly: false);
        if (patient is null)
            return Result.Failure<Guid>(Error.NotFound("Patient.NotFound", "Paciente no encontrado"));
        // Actualizar los datos del paciente
        patient.Update(
            patient,
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.BloodType,
            request.DocumentNumber,
            request.DateOfBirth,
            request.Ocupation,
            request.Religion,
            request.Alergies
        );

        // Persistir el agregado
        await _patientRepository.UpdateAsync(patient);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(patient.Id);
    }

    public async Task<Result<Guid>> Handle(UpdatePatientContactCommand request, CancellationToken cancellationToken)
    {
        // Cargar el agregado (no readonly porque vamos a modificarlo)
        var patient = await _patientRepository.GetByIdAsync(request.PatientId, readOnly: false);
        if (patient is null)
            return Result.Failure<Guid>(Error.NotFound("Patient.NotFound", "Paciente no encontrado"));

        // Actualizar los datos del contacto
        patient.UpdateContact(
            request.ContactId,
            request.PatientId,
            request.Direction,
            request.Reference,
            request.PhoneNumber,
            request.Floor,
            request.Coords
        );

        await _patientRepository.UpdateAsync(patient);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(request.ContactId);

    }
}
