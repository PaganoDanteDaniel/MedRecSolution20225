using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.EF.Configurations;
public class PatientMedicalHistoryConfiguration : IEntityTypeConfiguration<PatientMedicalHistory>
{
    public void Configure(EntityTypeBuilder<PatientMedicalHistory> builder)
    {
        builder.ToTable("PatientMedicalHistories");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.PatientId).IsRequired();
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
        builder.Property(e => e.RowVersion).IsRowVersion();
        builder.HasIndex(e => e.PatientId);

        // PatientMedicalHistory -> (1...1) <- PatientMedicalCondition
        builder.HasOne<PatientMedicalCondition>()
            .WithOne()
            .HasForeignKey<PatientMedicalCondition>(mc => mc.PatientMedicalHistoryId);

        // PatientMedicalHistory -> (1...N) <- PatientMedicalVisit
        builder.HasMany<PatientMedicalVisit>()
            .WithOne()
            .HasForeignKey(pv => pv.MedicalHistoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
