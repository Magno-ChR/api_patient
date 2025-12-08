using MediatR;
using patient.domain.Abstractions;
using patient.domain.Entities.Contacts;
using patient.domain.Entities.Patients;
using patient.domain.Results;



namespace patient.application.Patients.CreatePatient;

public class CreatePatientHandler : IRequestHandler<CreatePatientCommand, Result<Guid>>,
    IRequestHandler<CreatePatientContactCommand, Result<Guid>>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePatientHandler(IPatientRepository patientRepository ,IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var item = new Patient(
            Guid.NewGuid(),
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

        await _patientRepository.AddAsync(item);

        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(item.Id);

    }

    public async Task<Result<Guid>> Handle(CreatePatientContactCommand request, CancellationToken cancellationToken)
    {
        // Cargar el agregado (no readonly porque vamos a modificarlo)
        var patient = await _patientRepository.GetByIdAsync(request.PatientId, readOnly: false);
        if (patient is null)
            return Result.Failure<Guid>(Error.NotFound("Patient.NotFound", "Paciente no encontrado"));


        // Delegar la creación del contact al agregado para mantener invariantes
        patient.AddContact(request.Direction, request.Reference, request.PhoneNumber, request.Floor, request.Coords);

        // Persistir el agregado (si el repositorio devuelve una entidad trackeada, UpdateAsync puede ser redundante)
        await _patientRepository.UpdateAsync(patient);
        await _unitOfWork.CommitAsync(cancellationToken);   

        // Obtener el id del contacto recién creado (si AddContact no lo devuelve)
        var contactId = patient.Contacts.Last().Id;

        return Result.Success(contactId);
    }


}
