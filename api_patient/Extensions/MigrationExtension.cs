using Microsoft.EntityFrameworkCore;
using patient.infrastructure.Percistence;
using patient.infrastructure.Percistence.DomainModel;

namespace api_patient.Extensions;

public static class MigrationExtension
{
    public static void ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
        db.Migrate();

        var domainDb = scope.ServiceProvider.GetRequiredService<DomainDbContext>();
        domainDb.Database.Migrate();
    }
}
