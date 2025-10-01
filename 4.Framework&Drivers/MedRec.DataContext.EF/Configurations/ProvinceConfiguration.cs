using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.EF.Configurations;
internal class ProvinceConfiguration : IEntityTypeConfiguration<Province>
{
    public void Configure(EntityTypeBuilder<Province> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(p => p.Name).HasMaxLength(53).IsRequired();

        builder.Property(p => p.IsDeleted).HasDefaultValue(false);

        builder.Property(p => p.RowVersion).IsRowVersion();

        builder.HasMany<City>()
            .WithOne()
            .HasForeignKey(c => c.ProvinceId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
