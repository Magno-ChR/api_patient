using Consul;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace api_patient.Consul;

public static class ServiceCollectionExtensions
{
	/// <summary>Si <see cref="ConsulOptions.Host"/> está vacío, no registra nada.</summary>
	public static IServiceCollection AddConsulServiceDiscovery(this IServiceCollection services, IConfiguration configuration)
	{
		var host = configuration[$"{ConsulOptions.SectionName}:Host"];
		if (string.IsNullOrWhiteSpace(host))
			return services;

		services.Configure<ConsulOptions>(configuration.GetSection(ConsulOptions.SectionName));
		services.AddSingleton<IConsulClient>(_ => new ConsulClient(c => c.Address = new Uri(host)));
		services.AddHostedService<ConsulHostedService>();
		return services;
	}
}
