using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;

public class PatientMedicalHistoryConfiguration : IEntityTypeConfiguration<PatientMedicalHistory>
{
    public void Configure(EntityTypeBuilder<PatientMedicalHistory> builder)
    {
        builder.ToTable("PatientMedicalHistories");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.PatientId).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.Property(e => e.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(e => e.PatientId).IsUnique();

        builder.HasOne<PatientMedicalCondition>()
            .WithOne()
            .HasForeignKey<PatientMedicalCondition>(mc => mc.PatientMedicalHistoryId);

        builder.HasMany<PatientMedicalVisit>()
            .WithOne()
            .HasForeignKey(pv => pv.MedicalHistoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
