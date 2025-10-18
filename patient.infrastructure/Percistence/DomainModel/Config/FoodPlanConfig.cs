using Microsoft.EntityFrameworkCore;
using patient.domain.Entities.FoodPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace patient.infrastructure.Percistence.DomainModel.Config;

internal class FoodPlanConfig : IEntityTypeConfiguration<FoodPlan>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<FoodPlan> builder)
    {
        builder.ToTable("FoodPlan");
        builder.HasKey(x => x.Id).HasName("FoodPlanId");

        builder.Property(p => p.Id)
            .HasColumnName("FoodPlanId");

        builder.Ignore("_domainEvents");
        builder.Ignore(x => x.DomainEvents);
    }
}
