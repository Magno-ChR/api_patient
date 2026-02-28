using Joseco.Outbox.EFCore;
using patient.application;
using patient.domain.Abstractions;
using patient.infrastructure;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddOutboxBackgroundService<DomainEvent>(5000);

var host = builder.Build();
host.Run();
