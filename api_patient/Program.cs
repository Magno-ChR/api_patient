using AspNetCore.Swagger.Themes;
using api_patient.Middleware;
using api_patient.Extensions;
using api_patient.Observability;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using patient.infrastructure;
using patient.infrastructure.Observability;
using Prometheus;
using Serilog;
using Serilog.Sinks.Grafana.Loki;
using System.Text.Json;

namespace api_patient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var serviceName = builder.Configuration["OTEL_SERVICE_NAME"] ?? "api-patient";

            // Add services to the container.

            builder.Logging.Configure(options =>
            {
                options.ActivityTrackingOptions =
                    ActivityTrackingOptions.TraceId |
                    ActivityTrackingOptions.SpanId |
                    ActivityTrackingOptions.ParentId;
            });
            builder.Host.UseSerilog((context, services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();
                var lokiUrl = context.Configuration["LOKI_URL"]
                    ?? Environment.GetEnvironmentVariable("LOKI_URL");
                if (!string.IsNullOrWhiteSpace(lokiUrl))
                    loggerConfiguration.WriteTo.GrafanaLoki(lokiUrl.Trim());
                var seqUrl = context.Configuration["SEQ_SERVER_URL"]
                    ?? Environment.GetEnvironmentVariable("SEQ_SERVER_URL");
                if (!string.IsNullOrWhiteSpace(seqUrl))
                    loggerConfiguration.WriteTo.Seq(seqUrl.Trim());
            });
            builder.Services.AddControllers();
            builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ResultFormatMiddleware>();
            builder.Services.AddInfrastructure(builder.Configuration);
            var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
                ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
                ?? builder.Configuration["Telemetry:OtlpEndpoint"];
            builder.Services.AddOpenTelemetry()
                .ConfigureResource(resource => resource.AddService(serviceName))
                .WithTracing(tracing =>
                {
                    tracing
                        .AddAspNetCoreInstrumentation()
                        .AddHttpClientInstrumentation()
                        .AddSource(PatientTelemetry.ActivitySourceName);
                    if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                        tracing.AddOtlpExporter();
                });
            builder.Services
                .AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy("API running"), tags: ["live"])
                .AddCheck<DatabaseHealthCheck>("postgres", tags: ["ready"]);
            // Outbox: solo el Worker procesa la tabla outbox (OutboxBackgroundService + OutboxMessageHandler) y publica a RabbitMQ.
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API_PATIENT",
                    Version = "v1.0.0"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT. Pegar el token (con \"Bearer \"). Ejemplo: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(ModernStyle.Dark, x =>
            {
                x.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            });

            app.UseHttpsRedirection();
            app.UseSerilogRequestLogging();
            app.UseHttpMetrics();
            app.ApplyMigrations();
            app.UseAuthentication();
            app.UseAuthorization();

            app.Logger.LogInformation("API Patient started with Serilog and OpenTelemetry");

            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live")
            });
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = WriteHealthResponseAsync
            });
            app.MapMetrics("/metrics");
            app.MapControllers();

            app.Run();
        }

        private static Task WriteHealthResponseAsync(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var payload = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.ToDictionary(
                    entry => entry.Key,
                    entry => new
                    {
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                        durationMs = entry.Value.Duration.TotalMilliseconds
                    })
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }
}
