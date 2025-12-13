using Microsoft.EntityFrameworkCore;
using patient.domain.Entities.Backgrounds;
using patient.domain.Entities.Evolutions;
using patient.domain.Entities.Histories;


namespace patient.infrastructure.Percistence.DomainModel.Config;

internal class HistoryConfig : IEntityTypeConfiguration<History>,
    IEntityTypeConfiguration<Background>,
    IEntityTypeConfiguration<Evolution>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<History> builder)
    {
        builder.ToTable("History");
        builder.HasKey(x => x.Id).HasName("HistoryId");

        builder.Property(p => p.Id)
            .HasColumnName("HistoryId");

        // --- CONFIGURACIÓN DE RELACIONES (CLAVE PARA SOLUCIONAR HistoryId1) ---

        // 1. Configuración de Backgrounds
        builder.HasMany("_backgrounds") // Colección privada en History
            .WithOne() // Relación One-to-Many con la entidad Background (no necesitas la propiedad de navegación en History)
                       // **ESTO ES LO QUE ARREGLA HistoryId1:** Especifica la FK en la entidad Background/Evolution
            .HasForeignKey("HistoryId") // Nombre de la columna FK en la tabla Background (debe ser el nombre real)
            .IsRequired(); // Es obligatoria

        // 2. Configuración de Evolutions
        builder.HasMany("_evolutions") // Colección privada en History
            .WithOne() // Relación One-to-Many con la entidad Evolution
            .HasForeignKey("HistoryId") // Nombre de la columna FK en la tabla Evolution
            .IsRequired(); // Es obligatoria

        // ----------------------------------------------------------------------

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.Backgrounds);
        builder.Ignore(x => x.Evolutions);

    }

    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Background> builder)
    {
        builder.ToTable("Background");
        builder.HasKey(x => x.Id).HasName("BackgroundId");

        builder.Property(p => p.Id)
            .HasColumnName("BackgroundId");

        // Mapear la columna de clave externa que existe en la tabla Evolution
        builder.Property<Guid>("HistoryId") // Define la propiedad HistoryId como Guid
            .HasColumnName("HistoryId")      // Mapea a la columna física "HistoryId"
            .IsRequired();                   // Es NOT NULL

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
    }

    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Evolution> builder)
    {
        builder.ToTable("Evolution");
        builder.HasKey(x => x.Id).HasName("EvolutionId");

        builder.Property(p => p.Id)
            .HasColumnName("EvolutionId");

        // Mapear la columna de clave externa que existe en la tabla Evolution
        builder.Property<Guid>("HistoryId") // Define la propiedad HistoryId como Guid
            .HasColumnName("HistoryId")      // Mapea a la columna física "HistoryId"
            .IsRequired();                   // Es NOT NULL

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
    }

}
