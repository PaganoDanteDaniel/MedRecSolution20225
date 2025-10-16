using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;

public class MedicalConditionConfiguration : IEntityTypeConfiguration<MedicalCondition>
{
    public void Configure(EntityTypeBuilder<MedicalCondition> builder)
    {
        builder.HasKey(mc => mc.Id);
        builder.Property(mc => mc.Id).ValueGeneratedNever();

        builder.Property(mc => mc.ConditionTypeId).IsRequired();
        builder.Property(mc => mc.Name).IsRequired().HasMaxLength(100);
        builder.Property(mc => mc.IsDeleted).HasDefaultValue(false);
        builder.Property(mc => mc.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne<MedicalConditionType>()
               .WithMany()
               .HasForeignKey(mc => mc.ConditionTypeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(mc => mc.ConditionTypeId);
    }
}
