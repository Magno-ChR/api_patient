using Microsoft.Extensions.DependencyInjection;
using patient.domain;
using System.Reflection;


namespace patient.application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddDomain();
        services.AddMediatR(config =>{ 
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        return services;
    }
}
