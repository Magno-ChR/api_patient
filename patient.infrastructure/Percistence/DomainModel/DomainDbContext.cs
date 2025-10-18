using Microsoft.EntityFrameworkCore;
using patient.domain.Abstractions;
using patient.domain.Entities.Backgrounds;
using patient.domain.Entities.Contacts;
using patient.domain.Entities.Evolutions;
using patient.domain.Entities.FoodPlans;
using patient.domain.Entities.Histories;
using patient.domain.Entities.Patients;
using System.Reflection;


namespace patient.infrastructure.Percistence.DomainModel;

internal class DomainDbContext : DbContext
{
    public DbSet<Background> Backgrounds { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Evolution> Evolutions { get; set; }
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
