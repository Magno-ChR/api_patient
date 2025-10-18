using Microsoft.EntityFrameworkCore;
using patient.domain.Abstractions;
using patient.domain.Entities.FoodPlans;
using patient.domain.Entities.Histories;
using patient.infrastructure.Percistence.PersistenceModel.Entities;
using System.Reflection;


namespace patient.infrastructure.Percistence.DomainModel;

internal class DomainDbContext : DbContext
{
    public DbSet<BackgroundPM> Backgrounds { get; set; }
    public DbSet<PatientPM> Patients { get; set; }
    public DbSet<ContactPM> Contacts { get; set; }
    public DbSet<EvolutionPM> Evolutions { get; set; }
    public DbSet<FoodPlan> FoodPlans { get; set; }
    public DbSet<History> Histories { get; set; }

    public DomainDbContext(DbContextOptions<DomainDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<DomainEvent>();
    }
}
