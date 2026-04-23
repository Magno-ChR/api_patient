using MediatR;
using patient.domain.Abstractions;
using patient.domain.Entities.Histories;
using patient.domain.Results;
using System.Collections.Generic;

namespace patient.application.Histories.GetHistory;

public class GetHistoryHandler : IRequestHandler<GetHistoryCommand, Result<History>>,
    IRequestHandler<GetHistoryListByPatientIdCommand, Result<List<History>>>
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

    public async Task<Result<List<History>>> Handle(GetHistoryListByPatientIdCommand request, CancellationToken cancellationToken)
    {
        var histories = await _historyRepository.GetByPatientIdAsync(request.PatientId, readOnly: true);
        if (histories.Count < 1)
            return Result.Failure<List<History>>(Error.NotFound("History.NotFound", "No se encontraron historias para el paciente"));

        return Result.Success(histories);
    }
}
