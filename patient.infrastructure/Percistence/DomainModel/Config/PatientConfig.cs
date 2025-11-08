using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using patient.domain.Entities.Contacts;
using patient.domain.Entities.Patients;
using patient.domain.Shared;


namespace patient.infrastructure.Percistence.DomainModel.Config;

internal class PatientConfig : IEntityTypeConfiguration<Patient>,
    IEntityTypeConfiguration<Contact>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patient");
        builder.HasKey(x => x.Id).HasName("PatientId");

        builder.Property(p => p.Id)
            .HasColumnName("PatientId");

        var bloodTypeConverter = new ValueConverter<BloodType, string>(
            bloodTypeEnumValue => bloodTypeEnumValue.ToString(),
            bloodType => (BloodType)Enum.Parse(typeof(BloodType), bloodType)
            );

        builder.Property(p => p.BloodType)
            .HasConversion(bloodTypeConverter)
            .HasColumnName("BloodType");

        builder.HasMany("_contacts");


        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.Contacts);
    }

    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contact");
        builder.HasKey(x => x.Id).HasName("ContactId");

        builder.Property(p => p.Id)
            .HasColumnName("ContactId");

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
    }
}
