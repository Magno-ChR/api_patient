using Joseco.Outbox.EFCore;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using patient.application;
using patient.domain.Abstractions;
using patient.infrastructure;
using patient.infrastructure.Observability;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);
var serviceName = builder.Configuration["OTEL_SERVICE_NAME"] ?? "patient-worker";

builder.Logging.Configure(options =>
{
    options.ActivityTrackingOptions =
        ActivityTrackingOptions.TraceId |
        ActivityTrackingOptions.SpanId |
        ActivityTrackingOptions.ParentId;
});
builder.Services.AddSerilog((services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

// Cargar appsettings de la API para que RabbitMQ (y ConnectionString/JWT) se configure en un solo lugar
builder.Configuration.AddJsonFile(
    Path.Combine(builder.Environment.ContentRootPath, "..", "api_patient", "appsettings.json"),
    optional: true,
    reloadOnChange: false);
builder.Configuration.AddJsonFile(
    Path.Combine(builder.Environment.ContentRootPath, "..", "api_patient", $"appsettings.{builder.Environment.EnvironmentName}.json"),
    optional: true,
    reloadOnChange: false);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRabbitMqFoodPlanConsumer(builder.Configuration);
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddHttpClientInstrumentation()
        .AddSource(PatientTelemetry.ActivitySourceName)
        .AddOtlpExporter());

builder.Services.AddOutboxBackgroundService<DomainEvent>(5000);

var host = builder.Build();
host.Services.GetRequiredService<ILoggerFactory>()
    .CreateLogger("Startup")
    .LogInformation("Patient Worker started with Serilog and OpenTelemetry");
host.Run();
