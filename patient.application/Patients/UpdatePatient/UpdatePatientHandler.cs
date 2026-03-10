using MediatR;
using patient.domain.Abstractions;
using patient.domain.Entities.Patients;
using patient.domain.Entities.Patients.Events;
using patient.domain.Results;

namespace patient.application.Patients.UpdatePatient;

public class UpdatePatientHandler : IRequestHandler<UpdatePatientCommand, Result<Guid>>,
    IRequestHandler<UpdatePatientContactCommand, Result<Guid>>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdatePatientHandler(
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(UpdatePatientCommand request, CancellationToken cancellationToken)
    {
        var patient = await _patientRepository.GetByIdAsync(request.patientId, readOnly: false);
        if (patient is null)
            return Result.Failure<Guid>(Error.NotFound("Patient.NotFound", "Paciente no encontrado"));

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

        await _patientRepository.UpdateAsync(patient);

        // Patrón Outbox al estilo ms-logistic:
        // solo se agrega un DomainEvent y la infraestructura lo persiste en outbox
        // y el Worker lo publicará posteriormente a RabbitMQ.
        patient.AddDomainEvent(new PatientUpdatedEvent(patient.ToOutboxPayload()));

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
