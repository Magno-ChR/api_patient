using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using patient.application;
using patient.infrastructure;
using patient.infrastructure.Logging;
using patient.infrastructure.Observability;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = Host.CreateApplicationBuilder(args);
var serviceName =
	builder.Configuration["Telemetry:ServiceName"] ??
	builder.Configuration["OTEL_SERVICE_NAME"] ??
	"patient-worker";
var tracesOtlpEndpoint = builder.Configuration["Telemetry:OtlpEndpoint"];
var tracesOtlpProtocol = ResolveOtlpProtocol(
	builder.Configuration["Telemetry:OtlpProtocol"],
	tracesOtlpEndpoint);
var metricsOtlpEndpoint = builder.Configuration["Telemetry:MetricsOtlpEndpoint"];
var metricsOtlpProtocol = ResolveOtlpProtocol(
	builder.Configuration["Telemetry:MetricsOtlpProtocol"],
	metricsOtlpEndpoint);

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
		.Enrich.FromLogContext()
		.WriteTo.Console(new ReadableJsonConsoleFormatter());

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

builder.Services.AddOpenTelemetry()
	.ConfigureResource(resource => resource.AddService(serviceName))
	.WithTracing(tracing =>
	{
		tracing
			.AddHttpClientInstrumentation(options => options.RecordException = true)
			.AddSource(PatientTelemetry.ActivitySourceName);
		if (!string.IsNullOrWhiteSpace(tracesOtlpEndpoint) &&
		    Uri.TryCreate(tracesOtlpEndpoint.Trim(), UriKind.Absolute, out var tracesOtlpUri))
		{
			tracing.AddOtlpExporter(options =>
			{
				options.Endpoint = tracesOtlpUri;
				options.Protocol = tracesOtlpProtocol;
			});
		}
	})
	.WithMetrics(metrics =>
	{
		metrics.AddHttpClientInstrumentation();
		metrics.AddRuntimeInstrumentation();
		if (!string.IsNullOrWhiteSpace(metricsOtlpEndpoint) &&
		    Uri.TryCreate(metricsOtlpEndpoint.Trim(), UriKind.Absolute, out var metricsOtlpUri))
		{
			metrics.AddOtlpExporter(options =>
			{
				options.Endpoint = metricsOtlpUri;
				options.Protocol = metricsOtlpProtocol;
			});
		}
	});

var host = builder.Build();

try
{
	Log.Information("Patient Worker iniciado (Serilog + OpenTelemetry)");
	LogTelemetryConfiguration(serviceName, tracesOtlpEndpoint, tracesOtlpProtocol, metricsOtlpEndpoint, metricsOtlpProtocol);
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

static OtlpExportProtocol ResolveOtlpProtocol(string? configuredProtocol, string? endpoint)
{
	if (!string.IsNullOrWhiteSpace(configuredProtocol) &&
	    Enum.TryParse<OtlpExportProtocol>(configuredProtocol, ignoreCase: true, out var protocol))
	{
		return protocol;
	}

	if (Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) && uri.Port == 4318)
		return OtlpExportProtocol.HttpProtobuf;

	return OtlpExportProtocol.Grpc;
}

static void LogTelemetryConfiguration(
	string serviceName,
	string? tracesOtlpEndpoint,
	OtlpExportProtocol tracesOtlpProtocol,
	string? metricsOtlpEndpoint,
	OtlpExportProtocol metricsOtlpProtocol)
{
	if (!string.IsNullOrWhiteSpace(tracesOtlpEndpoint))
	{
		Log.Information(
			"OpenTelemetry traces exporter enabled. ServiceName: {ServiceName}, Endpoint: {Endpoint}, Protocol: {Protocol}",
			serviceName,
			tracesOtlpEndpoint,
			tracesOtlpProtocol);
	}
	else
	{
		Log.Warning(
			"OpenTelemetry traces exporter disabled because Telemetry:OtlpEndpoint is not configured. ServiceName: {ServiceName}",
			serviceName);
	}

	if (!string.IsNullOrWhiteSpace(metricsOtlpEndpoint))
	{
		Log.Information(
			"OpenTelemetry metrics exporter enabled. ServiceName: {ServiceName}, Endpoint: {Endpoint}, Protocol: {Protocol}",
			serviceName,
			metricsOtlpEndpoint,
			metricsOtlpProtocol);
		return;
	}

	Log.Information(
		"OpenTelemetry metrics exporter disabled. ServiceName: {ServiceName}",
		serviceName);
}
