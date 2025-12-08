using MediatR;
using patient.domain.Abstractions;
using patient.domain.Entities.Histories;
using patient.domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.application.Histories.GetHistory;

internal class GetHistoryHandler : IRequestHandler<GetHistoryCommand, Result<History>>
{
    private readonly IHistoryRepository _historyRepository;
    private readonly IUnitOfWork _unitOfWork;
    public GetHistoryHandler(IHistoryRepository historyRepository, IUnitOfWork unitOfWork)
    {
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
    }
    public async Task<Result<History>> Handle(GetHistoryCommand request, CancellationToken cancellationToken)
    {
        var history = await _historyRepository.GetByIdAsync(request.HistoryId, readOnly: true);
        if (history is null)
            return Result.Failure<History>(Error.NotFound("History.NotFound", "Historia no encontrada"));

        return Result.Success(history);
    }
}
