using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.EF.Configurations;
public class PatientMedicalVisitConfiguration : IEntityTypeConfiguration<PatientMedicalVisit>
{
    public void Configure(EntityTypeBuilder<PatientMedicalVisit> builder)
    {
        builder.ToTable("PatientMedicalVisits");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.MedicalHistoryId).IsRequired();
        builder.Property(e => e.VisitDate).IsRequired();
        builder.Property(e => e.Reason).HasMaxLength(500);
        builder.Property(e => e.Diagnosis).HasMaxLength(1000).IsRequired(false);
        builder.Property(e => e.Treatment).HasMaxLength(1000).IsRequired(false);
        builder.Property(e => e.SystolicPressure).IsRequired(false);
        builder.Property(e => e.DiastolicPressure).IsRequired(false);
        builder.Property(e => e.PulsePerMinute).IsRequired(false);
        builder.Property(e => e.Temperature).IsRequired(false);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        builder.Property(e => e.RowVersion)
               .IsRowVersion();

        builder.HasIndex(e => e.MedicalHistoryId);
        builder.HasIndex(e => e.VisitDate);
    }
}
