using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using patient.application;
using patient.domain.Abstractions;
using patient.domain.Entities.Patients;
using patient.infrastructure.Percistence;
using patient.infrastructure.Percistence.Repositories;

namespace patient.infrastructure;

public static class DependencyInyection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication()
            .AddPersistence(configuration);
        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        // Aquí se agregarían los servicios relacionados con la persistencia de datos,
        // como el contexto de la base de datos, repositorios, etc.

        services.AddScoped<IPatientRepository, PatientRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>(); 
    }
}
