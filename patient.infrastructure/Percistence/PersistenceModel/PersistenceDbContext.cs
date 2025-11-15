using Microsoft.EntityFrameworkCore;
using patient.domain.Abstractions;
using patient.domain.Entities.FoodPlans;
using patient.domain.Entities.Histories;

using patient.infrastructure.Percistence.PersistenceModel.Entities;


namespace patient.infrastructure.Percistence.PersistenceModel;

public class PersistenceDbContext : DbContext, IDatabase
{
    public DbSet<BackgroundPM> Backgrounds { get; set; }
    public DbSet<PatientPM> Patients { get; set; }
    public DbSet<ContactPM> Contacts { get; set; }
    public DbSet<EvolutionPM> Evolutions { get; set; }
    public DbSet<FoodPlanPM> FoodPlans { get; set; }
    public DbSet<HistoryPM> Histories { get; set; }

    public PersistenceDbContext(DbContextOptions<PersistenceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ignorar los DomainEvent para que EF no intente mapearlos como entidades
        modelBuilder.Ignore<DomainEvent>();

        base.OnModelCreating(modelBuilder);
    }

    public void Migrate()
    {
        Database.Migrate();
    }

}
