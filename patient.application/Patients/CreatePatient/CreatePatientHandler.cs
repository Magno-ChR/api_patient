using MediatR;
using patient.domain.Abstractions;
using patient.domain.Entities.Patients;
using patient.domain.Results;



namespace patient.application.Patients.CreatePatient;

internal class CreatePatientHandler : IRequestHandler<CreatePatientCommand, Result<Guid>>
{
    private readonly IPatientRepository _patientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePatientHandler(IPatientRepository patientRepository, IUnitOfWork unitOfWork)
    {
        _patientRepository = patientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
    {
        var item = new Patient(
            request.Id,
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
}
