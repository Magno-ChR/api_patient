using Joseco.Outbox.EFCore;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using patient.application;
using patient.domain.Abstractions;
using patient.infrastructure;
using patient.infrastructure.Observability;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = Host.CreateApplicationBuilder(args);
var serviceName = builder.Configuration["OTEL_SERVICE_NAME"] ?? "patient-worker";

builder.Logging.Configure(options =>
{
	options.ActivityTrackingOptions =
		ActivityTrackingOptions.TraceId |
		ActivityTrackingOptions.SpanId |
		ActivityTrackingOptions.ParentId;
});

// appsettings de api_patient (p. ej. Rabbit) se cargan antes que variables de entorno del contenedor,
// para que Loki__Uri / Telemetry__OtlpEndpoint de Docker no queden anulados por "Loki:Uri": "" en JSON.
builder.Configuration.AddJsonFile(
	Path.Combine(builder.Environment.ContentRootPath, "..", "api_patient", "appsettings.json"),
	optional: true,
	reloadOnChange: false);
builder.Configuration.AddJsonFile(
	Path.Combine(builder.Environment.ContentRootPath, "..", "api_patient", $"appsettings.{builder.Environment.EnvironmentName}.json"),
	optional: true,
	reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSerilog((services, loggerConfiguration) =>
{
	loggerConfiguration
		.ReadFrom.Configuration(builder.Configuration)
		.ReadFrom.Services(services)
		.Enrich.FromLogContext();

	var lokiUri = builder.Configuration["Loki:Uri"];
	if (!string.IsNullOrWhiteSpace(lokiUri) &&
	    Uri.TryCreate(lokiUri.Trim(), UriKind.Absolute, out var loki) &&
	    (loki.Scheme == Uri.UriSchemeHttp || loki.Scheme == Uri.UriSchemeHttps))
	{
		loggerConfiguration.WriteTo.GrafanaLoki(
			lokiUri.Trim(),
			[new LokiLabel { Key = "service_name", Value = serviceName }]);
	}
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRabbitMqFoodPlanConsumer(builder.Configuration);

var otlpEndpoint = builder.Configuration["Telemetry:OtlpEndpoint"];
builder.Services.AddOpenTelemetry()
	.ConfigureResource(resource => resource.AddService(serviceName))
	.WithTracing(tracing =>
	{
		tracing
			.AddHttpClientInstrumentation()
			.AddSource(PatientTelemetry.ActivitySourceName);
		if (!string.IsNullOrWhiteSpace(otlpEndpoint) &&
		    Uri.TryCreate(otlpEndpoint.Trim(), UriKind.Absolute, out var otlpUri))
			tracing.AddOtlpExporter(o => o.Endpoint = otlpUri);
	})
	.WithMetrics(metrics =>
	{
		metrics.AddHttpClientInstrumentation();
		metrics.AddRuntimeInstrumentation();
		if (!string.IsNullOrWhiteSpace(otlpEndpoint) &&
		    Uri.TryCreate(otlpEndpoint.Trim(), UriKind.Absolute, out var otlpUri))
			metrics.AddOtlpExporter(o => o.Endpoint = otlpUri);
	});

builder.Services.AddOutboxBackgroundService<DomainEvent>(5000);

var host = builder.Build();

try
{
	Log.Information("Patient Worker iniciado (Serilog + OpenTelemetry)");
	host.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
	Log.Fatal(ex, "Patient Worker: fallo fatal al ejecutar");
	throw;
}
finally
{
	Log.CloseAndFlush();
}
