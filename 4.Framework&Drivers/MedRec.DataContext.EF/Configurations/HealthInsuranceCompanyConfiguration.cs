using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.EF.Configurations;
public class HealthInsuranceCompanyConfiguration : IEntityTypeConfiguration<HealthInsuranceCompany>
{
    public void Configure(EntityTypeBuilder<HealthInsuranceCompany> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id)
            .HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.Acronym)
            .HasMaxLength(19);

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasMany<Patient>()
            .WithOne()
            .HasForeignKey(p => p.HealthInsuranceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
