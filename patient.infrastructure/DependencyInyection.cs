using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using patient.application;

namespace patient.infrastructure;

public static class DependencyInyection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();
        return services;
    }
}
