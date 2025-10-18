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

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);

    }

    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Background> builder)
    {
        builder.ToTable("Background");
        builder.HasKey(x => x.Id).HasName("BackgroundId");

        builder.Property(p => p.Id)
            .HasColumnName("BackgroundId");
    }

    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Evolution> builder)
    {
        builder.ToTable("Evolution");
        builder.HasKey(x => x.Id).HasName("EvolutionId");

        builder.Property(p => p.Id)
            .HasColumnName("EvolutionId");
    }

}
