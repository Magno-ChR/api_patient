using patient.domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.domain.Entities.Histories.Events;

public record EvolutionCreateEvent(Guid EvolutionId) : DomainEvent;
