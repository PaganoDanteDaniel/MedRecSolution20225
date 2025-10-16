using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.EF.Configurations;
internal class CityConfiguration : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CityName)
            .HasMaxLength(60)
            .IsRequired();

        builder.Property(c => c.CityCode)
            .HasMaxLength(10)
            .IsRequired(false);

        builder.Property(c => c.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        builder.HasMany<Patient>()
            .WithOne()
            .HasForeignKey(p => p.CityId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
