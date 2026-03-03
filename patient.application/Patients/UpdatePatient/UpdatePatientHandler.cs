using Joseco.Outbox.Contracts.Model;
using Joseco.Outbox.Contracts.Service;
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
    private readonly IOutboxService<DomainEvent> _outboxService;
    private readonly IMediator _mediator;

    public UpdatePatientHandler(
        IPatientRepository patientRepository,
        IUnitOfWork unitOfWork,
        IOutboxService<DomainEvent> outboxService,
        IMediator mediator)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
        _outboxService = outboxService;
        _mediator = mediator;
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

        var updatedEvent = new PatientUpdatedEvent(patient.ToOutboxPayload());
        var outboxMessage = new OutboxMessage<DomainEvent>(updatedEvent);
        await _outboxService.AddAsync(outboxMessage);

        await _unitOfWork.CommitAsync(cancellationToken);

        // Publicar a RabbitMQ tras commit (Joseco.Outbox no despacha a MediatR al procesar el outbox)
        await _mediator.Publish(updatedEvent, cancellationToken);

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
