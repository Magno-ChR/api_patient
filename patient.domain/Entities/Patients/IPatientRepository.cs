using patient.domain.Abstractions;
using patient.domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.domain.Entities.Patients;

public interface IPatientRepository : IRepository<Patient>
{
    Task UpdateAsync(Patient patient);

    Task<PagedResult<Patient>> GetPagedAsync(int page, int pageSize, string? search = null);

}
