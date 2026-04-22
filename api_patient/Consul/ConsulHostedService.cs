using Consul;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace api_patient.Consul;

/// <summary>Registra el servicio en Consul al arrancar y lo da de baja al apagar.</summary>
public sealed class ConsulHostedService : IHostedService
{
	private readonly IConsulClient _consul;
	private readonly ConsulOptions _options;
	private readonly ILogger<ConsulHostedService> _logger;
	private string? _registrationId;

	public ConsulHostedService(
		IConsulClient consul,
		IOptions<ConsulOptions> options,
		ILogger<ConsulHostedService> logger)
	{
		_consul = consul;
		_options = options.Value;
		_logger = logger;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(_options.Host))
		{
			_logger.LogInformation("Consul:Host vacío; se omite el registro de service discovery.");
			return;
		}

		if (string.IsNullOrWhiteSpace(_options.ServiceName) ||
		    string.IsNullOrWhiteSpace(_options.ServiceAddress) ||
		    _options.ServicePort <= 0)
		{
			_logger.LogWarning("Consul: configuración de servicio incompleta; no se registra.");
			return;
		}

		_registrationId = $"{_options.ServiceName}-{Environment.MachineName}-{Guid.NewGuid():N}".Trim();

		var healthUrl = $"http://{_options.ServiceAddress}:{_options.ServicePort}/health/live";
		var registration = new AgentServiceRegistration
		{
			ID = _registrationId,
			Name = _options.ServiceName,
			Address = _options.ServiceAddress,
			Port = _options.ServicePort,
			Check = new AgentServiceCheck
			{
				HTTP = healthUrl,
				Interval = TimeSpan.FromSeconds(10),
				Timeout = TimeSpan.FromSeconds(5),
				DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1)
			}
		};

		await _consul.Agent.ServiceRegister(registration, cancellationToken);
		_logger.LogInformation(
			"Consul: servicio registrado. Id={RegistrationId}, Name={ServiceName}, Address={Address}:{Port}, Health={HealthUrl}",
			_registrationId,
			_options.ServiceName,
			_options.ServiceAddress,
			_options.ServicePort,
			healthUrl);
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(_registrationId))
			return;

		try
		{
			await _consul.Agent.ServiceDeregister(_registrationId, cancellationToken);
			_logger.LogInformation("Consul: servicio dado de baja. Id={RegistrationId}", _registrationId);
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Consul: error al deregister Id={RegistrationId}", _registrationId);
		}
	}
}
