using Microsoft.Extensions.DependencyInjection;
using patient.domain.Entities.Patients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        //services.AddSingleton<IPatientStrategy, Strategy>
        return services;
    }
}
