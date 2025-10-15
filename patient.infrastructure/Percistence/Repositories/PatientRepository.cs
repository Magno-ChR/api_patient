using patient.domain.Entities.Patients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.infrastructure.Percistence.Repositories;

//No se necesario que sea publico solo se usará aqui
internal class PatientRepository : IPatientRepository
{
    public Task AddAsync(Patient entity)
    {
       return Task.CompletedTask;
    }

    public Task<Patient?> GetByIdAsync(Guid id, bool readOnly = false)
    {
        return null;
    }

    public Task UpdateAsync(Patient patient)
    {
        return Task.CompletedTask;
    }
}
