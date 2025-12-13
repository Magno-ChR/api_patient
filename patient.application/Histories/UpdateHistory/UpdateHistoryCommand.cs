using MediatR;
using patient.domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.application.Histories.UpdateHistory;

public record UpdateHistoryCommand(Guid HistoryId, string Reason, string? Diagnostic, string? Treatment) : IRequest<Result<Guid>>;

public record UpdateBackgroundCommand(Guid HistoryId, Guid BackgroudId, string Description) : IRequest<Result<Guid>>;