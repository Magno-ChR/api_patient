using MediatR;
using patient.domain.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.application.Patients.DeletePatient;

public record DeletePatientContactCommand(Guid PatientId, Guid ContactId) : IRequest<Result<Guid>>;
