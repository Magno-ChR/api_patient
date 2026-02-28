using Joseco.Outbox.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using patient.application;
using patient.application.Patients.CreatePatient;
using patient.domain.Abstractions;
using patient.domain.Entities.FoodPlans;
using patient.domain.Entities.Histories;
using patient.domain.Entities.Patients;
using patient.infrastructure.Extensions;
using patient.infrastructure.Percistence;
using patient.infrastructure.Percistence.DomainModel;
using patient.infrastructure.Percistence.Outbox;
using patient.infrastructure.Percistence.PersistenceModel;
using patient.infrastructure.Percistence.Repositories;
using patient.infrastructure.Security;

namespace patient.infrastructure;

public static class DependencyInyection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddJwtAuthentication(configuration);

        services.AddApplication();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CreatePatientHandler).Assembly);
            cfg.RegisterServicesFromAssembly(typeof(OutboxMessageHandler).Assembly);
        });
        services.AddPersistence(configuration);
        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<DomainDbContext>(context =>
                context.UseNpgsql(connectionString));
        services.AddDbContext<PersistenceDbContext>(context =>
                context.UseNpgsql(connectionString));

        services.AddScoped<IDatabase, PersistenceDbContext>();

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IHistoryRepository, HistoryRepository>();
        services.AddScoped<IFoodPlanRepository, FoodPlanRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<Joseco.Outbox.EFCore.Persistence.IOutboxDatabase<DomainEvent>, OutboxDatabase>();
        services.AddOutbox<DomainEvent>();
    }
}
