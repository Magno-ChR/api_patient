using Joseco.Outbox.Contracts.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using patient.domain.Abstractions;
using patient.domain.Entities.Backgrounds;
using patient.domain.Entities.Contacts;
using patient.domain.Entities.Evolutions;
using patient.domain.Entities.FoodPlans;
using patient.domain.Entities.Histories;
using patient.domain.Entities.Patients;
using System.Reflection;


namespace patient.infrastructure.Percistence.DomainModel;

public class DomainDbContext : DbContext
{
    public DbSet<Background> Backgrounds { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Contact> Contacts { get; set; }
    public DbSet<Evolution> Evolutions { get; set; }
    public DbSet<FoodPlan> FoodPlans { get; set; }
    public DbSet<History> Histories { get; set; }
    public DbSet<OutboxMessage<DomainEvent>> OutboxMessages { get; set; }

    public DomainDbContext(DbContextOptions<DomainDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        ConfigureOutboxModel(modelBuilder);

        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<DomainEvent>();
    }

    private static void ConfigureOutboxModel(ModelBuilder modelBuilder)
    {
        const string schema = "outbox";
        var builder = modelBuilder.Entity<OutboxMessage<DomainEvent>>();
        builder.ToTable("outboxMessage", schema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("outboxId");
        builder.Property(x => x.Created).HasColumnName("created");
        builder.Property(x => x.Type).HasColumnName("type");
        builder.Property(x => x.Processed).HasColumnName("processed");
        builder.Property(x => x.ProcessedOn).HasColumnName("processedOn");
        builder.Property(x => x.CorrelationId).HasColumnName("correlationId");
        builder.Property(x => x.TraceId).HasColumnName("traceId");
        builder.Property(x => x.SpanId).HasColumnName("spanId");
        var jsonSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        var contentConverter = new ValueConverter<DomainEvent, string>(
            obj => JsonConvert.SerializeObject(obj, jsonSettings),
            s => JsonConvert.DeserializeObject<DomainEvent>(s, jsonSettings)!);
        builder.Property(x => x.Content).HasConversion(contentConverter).HasColumnName("content");
    }
}
