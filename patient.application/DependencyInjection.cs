using Microsoft.Extensions.DependencyInjection;
using patient.domain;

namespace patient.application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddDomain();
        return services;
    }
}
