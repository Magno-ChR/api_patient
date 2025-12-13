using MediatR;
using patient.domain.Abstractions;
using patient.domain.Entities.Histories;
using patient.domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.application.Histories.UpdateHistory;

internal class UpdateHistoryHandler : IRequestHandler<UpdateHistoryCommand, Result<Guid>>,
    IRequestHandler<UpdateBackgroundCommand, Result<Guid>>
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateHistoryHandler(IHistoryRepository historyRepository, IUnitOfWork unitOfWork)
    {
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(UpdateHistoryCommand request, CancellationToken cancellationToken)
    {
        var history = await _historyRepository.GetByIdAsync(request.HistoryId, readOnly: false);

        if (history is null)
            return Result<Guid>.ValidationFailure(Error.NotFound("History.NotFound", "Historia no encontrada"));


        History.Update(history, request.Reason, request.Diagnostic ?? string.Empty, request.Treatment ?? string.Empty);

        await _historyRepository.UpdateAsync(history);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Result.Success(history.Id);
    }

    public async Task<Result<Guid>> Handle(UpdateBackgroundCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var history = await _historyRepository.GetByIdAsync(request.HistoryId, readOnly: false);
            if (history is null)
                return Result<Guid>.ValidationFailure(Error.NotFound("Background.History.Not.Found", "Historia no encontrada"));

            history.UpdateBackground(request.HistoryId, request.BackgroudId, request.Description);

            await _historyRepository.UpdateAsync(history);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success(history.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(new Error("Background.Update.Error", ex.Message,ErrorType.Validation));
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(Error.Problem("Background.Update.Error", "Error al actualizar el antecedente: {0}", ex.Message));
        }
    }
}
