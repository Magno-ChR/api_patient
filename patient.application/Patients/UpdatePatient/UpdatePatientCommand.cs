using MediatR;
using patient.domain.Results;
using patient.domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.application.Patients.UpdatePatient;

public record UpdatePatientCommand(Guid patientId,string FirstName, string MiddleName, string LastName, BloodType BloodType, string DocumentNumber,
    DateOnly DateOfBirth, string Ocupation, string Religion, string Alergies) : IRequest<Result<Guid>>;

public record UpdatePatientContactCommand(Guid PatientId, Guid ContactId, string Direction, string Reference, string PhoneNumber, string Floor, string Coords) : IRequest<Result<Guid>>;