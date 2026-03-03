using Joseco.Outbox.EFCore;
using patient.application;
using patient.domain.Abstractions;
using patient.infrastructure;

var builder = Host.CreateApplicationBuilder(args);

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

builder.Services.AddOutboxBackgroundService<DomainEvent>(5000);

var host = builder.Build();
host.Run();
