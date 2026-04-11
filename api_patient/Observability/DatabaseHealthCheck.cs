using Microsoft.Extensions.Diagnostics.HealthChecks;
using patient.infrastructure.Percistence.DomainModel;

namespace api_patient.Observability;

public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly DomainDbContext _dbContext;

    public DatabaseHealthCheck(DomainDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? HealthCheckResult.Healthy("PostgreSQL connection OK")
            : HealthCheckResult.Unhealthy("PostgreSQL connection failed");
    }
}
