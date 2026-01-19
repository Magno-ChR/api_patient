using MediatR;
using patient.domain.Abstractions;
using patient.domain.Entities.Backgrounds;
using patient.domain.Entities.Evolutions;
using patient.domain.Entities.Histories;
using patient.domain.Entities.Patients;
using patient.domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.application.Histories.CreateHistory;

public class CreateHistoryHandler : IRequestHandler<CreateHistoryCommand, Result<Guid>>,
    IRequestHandler<CreateBackgroudCommand, Result<Guid>>,
    IRequestHandler<CreateEvolutionCommand, Result<Guid>>
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateHistoryHandler(IHistoryRepository historyRepository, IUnitOfWork unitOfWork)
    {
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Result<Guid>> Handle(CreateHistoryCommand request, CancellationToken cancellationToken)
    {
        var item = new History(
            Guid.NewGuid(),
            request.patientId,
            request.foodPlanId,
            request.reason,
            request.diagnostic,
            request.treatment
        );

        await _historyRepository.AddAsync(item);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(item.Id);
    }

    public async Task<Result<Guid>> Handle(CreateBackgroudCommand request, CancellationToken cancellationToken)
    {
        var history = await _historyRepository.GetByIdAsync(request.historyId, readOnly: false);
        if (history is null)
            return Result.Failure<Guid>(Error.NotFound("History.NotFound", "Historia no encontrada"));

        history.AddBackground(
            new Background(
                Guid.NewGuid(),
                request.historyId,
                request.description
            )
        );

        await _historyRepository.UpdateAsync(history);
        await _unitOfWork.CommitAsync(cancellationToken);

        Guid backgroundId = history.Backgrounds.Last().Id;

        return Result.Success(backgroundId);
    }

    public async Task<Result<Guid>> Handle(CreateEvolutionCommand request, CancellationToken cancellationToken)
    {
        var history = await _historyRepository.GetByIdAsync(request.historyId, readOnly: false);
        if (history is null)
            return Result.Failure<Guid>(Error.NotFound("History.NotFound", "Historia no encontrada"));

        history.AddEvolution(
            new Evolution(
                Guid.NewGuid(),
                request.historyId,
                request.description,
                request.observation,
                request.medicOrder
            )
        );

        await _historyRepository.UpdateAsync(history);
        await _unitOfWork.CommitAsync(cancellationToken);

        Guid backgroundId = history.Evolutions.Last().Id;

        return Result.Success(backgroundId);
    }
}
