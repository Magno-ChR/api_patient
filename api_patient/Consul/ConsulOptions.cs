namespace api_patient.Consul;

public sealed class ConsulOptions
{
	public const string SectionName = "Consul";

	public string Host { get; set; } = string.Empty;

	public string ServiceName { get; set; } = string.Empty;

	public string ServiceAddress { get; set; } = string.Empty;

	public int ServicePort { get; set; }
}
