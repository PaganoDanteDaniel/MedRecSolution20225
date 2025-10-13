using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;

public class MedicalConditionTypeConfiguration : IEntityTypeConfiguration<MedicalConditionType>
{
    public void Configure(EntityTypeBuilder<MedicalConditionType> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.Property(e => e.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();
    }
}
