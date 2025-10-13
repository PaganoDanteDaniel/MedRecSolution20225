using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;
public class LaboratoryResultTypeConfiguration : IEntityTypeConfiguration<LaboratoryResultType>
{
    public void Configure(EntityTypeBuilder<LaboratoryResultType> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ResultName).HasMaxLength(60).IsRequired();
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.Property(e => e.RowVersion).IsRowVersion();
    }
}