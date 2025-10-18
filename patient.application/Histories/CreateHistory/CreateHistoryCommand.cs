using MediatR;
using patient.domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.application.Histories.CreateHistory;

public record CreateHistoryCommand(Guid patientId, Guid foodPlanId, string reason, string diagnostic, string treatment) : IRequest<Result<Guid>>;

public record CreateBackgroudCommand(Guid historyId, string description) : IRequest<Result<Guid>>;

public record CreateEvolutionCommand(Guid historyId, string description, string observation, string medicOrder) : IRequest<Result<Guid>>;
