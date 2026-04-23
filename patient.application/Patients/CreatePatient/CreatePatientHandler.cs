using MediatR;
using patient.domain.Abstractions;
using patient.domain.Entities.Contacts;
using patient.domain.Entities.Patients;
using patient.domain.Entities.Patients.Events;
using patient.domain.Results;
using Microsoft.Extensions.Logging;

namespace patient.application.Patients.CreatePatient;

public class CreatePatientHandler : IRequestHandler<CreatePatientCommand, Result<Guid>>,
    IRequestHandler<CreatePatientContactCommand, Result<Guid>>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreatePatientHandler> _logger;

    public CreatePatientHandler(
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreatePatientHandler> logger)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
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

        _logger.LogInformation(
            "Creating patient aggregate. PatientId: {PatientId}, DocumentNumber: {DocumentNumber}",
            item.Id,
            item.DocumentNumber);

        await _patientRepository.AddAsync(item);

        // Patrón Outbox al estilo ms-logistic:
        // el agregado solo agrega un DomainEvent; la infraestructura se encarga de
        // convertirlo en un mensaje de outbox y el Worker lo publicará a RabbitMQ.
        item.AddDomainEvent(new PatientCreatedEvent(item.ToOutboxPayload()));
        _logger.LogInformation(
            "PatientCreatedEvent appended to aggregate. PatientId: {PatientId}, PendingDomainEvents: {DomainEventsCount}",
            item.Id,
            item.DomainEvents.Count);

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation("Patient committed successfully. PatientId: {PatientId}", item.Id);

        return Result.Success(item.Id);
    }

    public async Task<Result<Guid>> Handle(CreatePatientContactCommand request, CancellationToken cancellationToken)
    {
        // Cargar el agregado (no readonly porque vamos a modificarlo)
        var patient = await _patientRepository.GetByIdAsync(request.PatientId, readOnly: false);
        if (patient is null)
            return Result.Failure<Guid>(Error.Problem("Patient.NotFound", $"El paciente con ID {request.PatientId} no existe"));


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
