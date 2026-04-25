using AspNetCore.Swagger.Themes;
using api_patient.Consul;
using api_patient.Middleware;
using api_patient.Extensions;
using api_patient.Observability;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using patient.infrastructure;
using patient.infrastructure.Logging;
using patient.infrastructure.Observability;
using Prometheus;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;
using System.Security.Claims;
using System.Text.Json;

namespace api_patient
{
	public class Program
	{
		private const string CorsPolicyName = "ApiPatientCors";

		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			var serviceName =
				builder.Configuration["Telemetry:ServiceName"] ??
				builder.Configuration["OTEL_SERVICE_NAME"] ??
				"api-patient";
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
			builder.Host.UseSerilog((context, services, loggerConfiguration) =>
			{
				loggerConfiguration
					.ReadFrom.Configuration(context.Configuration)
					.ReadFrom.Services(services)
					.Enrich.FromLogContext()
					.WriteTo.Console(new ReadableJsonConsoleFormatter());

				var lokiUri = context.Configuration["Loki:Uri"];
				if (!string.IsNullOrWhiteSpace(lokiUri) &&
				    Uri.TryCreate(lokiUri.Trim(), UriKind.Absolute, out var loki) &&
				    (loki.Scheme == Uri.UriSchemeHttp || loki.Scheme == Uri.UriSchemeHttps))
				{
					loggerConfiguration.WriteTo.GrafanaLoki(
						lokiUri.Trim(),
						[new LokiLabel { Key = "service_name", Value = serviceName }]);
				}
			});

			builder.Services.AddControllers();
			builder.Services.AddCors(options =>
			{
				options.AddPolicy(CorsPolicyName, policy =>
				{
					if (builder.Environment.IsDevelopment())
					{
						policy
							.AllowAnyOrigin()
							.AllowAnyHeader()
							.AllowAnyMethod();
						return;
					}

					policy
						.WithOrigins("http://localhost:5003", "https://localhost:7134")
						.AllowAnyHeader()
						.AllowAnyMethod();
				});
			});
			builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, ResultFormatMiddleware>();
			builder.Services.AddInfrastructure(builder.Configuration);

			builder.Services.AddOpenTelemetry()
				.ConfigureResource(resource => resource.AddService(serviceName))
				.WithTracing(tracing =>
				{
					tracing
						.AddAspNetCoreInstrumentation(options => options.RecordException = true)
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
					metrics.AddAspNetCoreInstrumentation();
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

			builder.Services
				.AddHealthChecks()
				.AddCheck("self", () => HealthCheckResult.Healthy("API running"), tags: ["live"])
				.AddCheck<DatabaseHealthCheck>("postgres", tags: ["ready"]);

			builder.Services.AddConsulServiceDiscovery(builder.Configuration);

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

			app.UseSwagger();
			app.UseSwaggerUI(ModernStyle.Dark, x =>
			{
				x.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
			});

			if (!app.Environment.IsDevelopment())
			{
				app.UseHttpsRedirection();
			}

			app.UseSerilogRequestLogging(options =>
			{
				options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} -> {StatusCode} ({Elapsed:0.0} ms)";
				options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
				{
					diagnosticContext.Set("ClientIp", httpContext.Connection.RemoteIpAddress?.ToString());

					var userId =
						httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
						httpContext.User.FindFirst("uid")?.Value;

					if (!string.IsNullOrWhiteSpace(userId))
						diagnosticContext.Set("UserId", userId);
				};
				options.GetLevel = (httpContext, elapsed, ex) =>
				{
					if (ex != null || httpContext.Response.StatusCode >= 500)
					{
						return LogEventLevel.Error;
					}

					string path = httpContext.Request.Path.Value ?? string.Empty;
					if (path.StartsWith("/metrics", StringComparison.OrdinalIgnoreCase) ||
					    path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
					{
						return LogEventLevel.Verbose;
					}

					return LogEventLevel.Information;
				};
			});
			app.UseHttpMetrics();
			app.UseCors(CorsPolicyName);
			app.ApplyMigrations();
			app.UseAuthentication();
			app.UseAuthorization();

			app.Logger.LogInformation("API Patient started with Serilog and OpenTelemetry");
			LogTelemetryConfiguration(app.Logger, serviceName, tracesOtlpEndpoint, tracesOtlpProtocol, metricsOtlpEndpoint, metricsOtlpProtocol);

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

		private static OtlpExportProtocol ResolveOtlpProtocol(string? configuredProtocol, string? endpoint)
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

		private static void LogTelemetryConfiguration(
			Microsoft.Extensions.Logging.ILogger logger,
			string serviceName,
			string? tracesOtlpEndpoint,
			OtlpExportProtocol tracesOtlpProtocol,
			string? metricsOtlpEndpoint,
			OtlpExportProtocol metricsOtlpProtocol)
		{
			if (!string.IsNullOrWhiteSpace(tracesOtlpEndpoint))
			{
				logger.LogInformation(
					"OpenTelemetry traces exporter enabled. ServiceName: {ServiceName}, Endpoint: {Endpoint}, Protocol: {Protocol}",
					serviceName,
					tracesOtlpEndpoint,
					tracesOtlpProtocol);
			}
			else
			{
				logger.LogWarning(
					"OpenTelemetry traces exporter disabled because Telemetry:OtlpEndpoint is not configured. ServiceName: {ServiceName}",
					serviceName);
			}

			if (!string.IsNullOrWhiteSpace(metricsOtlpEndpoint))
			{
				logger.LogInformation(
					"OpenTelemetry metrics exporter enabled. ServiceName: {ServiceName}, Endpoint: {Endpoint}, Protocol: {Protocol}",
					serviceName,
					metricsOtlpEndpoint,
					metricsOtlpProtocol);
				return;
			}

			logger.LogInformation(
				"OpenTelemetry metrics exporter disabled. Prometheus scraping remains available on /metrics. ServiceName: {ServiceName}",
				serviceName);
		}
	}
}
