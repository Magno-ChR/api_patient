using Microsoft.EntityFrameworkCore;
using patient.infrastructure.Percistence;
using patient.infrastructure.Percistence.DomainModel;

namespace api_patient.Extensions;

public static class MigrationExtension
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        try
        {
            var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
            db.Migrate();
            app.Logger.LogInformation("Primary database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Primary database migration skipped: database unavailable or not ready.");
        }

        try
        {
            var domainDb = scope.ServiceProvider.GetRequiredService<DomainDbContext>();
            domainDb.Database.Migrate();
            app.Logger.LogInformation("Domain database migrations applied successfully.");
        }
        catch (Exception ex)
        {
            app.Logger.LogWarning(ex, "Domain database migration skipped: database unavailable or not ready.");
        }
    }
}
