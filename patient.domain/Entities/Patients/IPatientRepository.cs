using patient.domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.domain.Entities.Patients
{
    internal interface IPatientRepository : IRepository<Patient>
    {
        Task UpdateAsync(Patient patient);
    }
}
