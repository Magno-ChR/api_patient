using MediatR;
using patient.domain.Entities.Histories;
using patient.domain.Results;


namespace patient.application.Histories.GetHistory;

public record GetHistoryCommand(Guid HistoryId) : IRequest<Result<History>>;
