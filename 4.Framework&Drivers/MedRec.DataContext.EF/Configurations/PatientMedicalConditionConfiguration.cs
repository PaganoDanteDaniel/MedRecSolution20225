using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.EF.Configurations;
public class PatientMedicalConditionConfiguration : IEntityTypeConfiguration<PatientMedicalCondition>
{
    public void Configure(EntityTypeBuilder<PatientMedicalCondition> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PatientMedicalHistoryId)
               .IsRequired();

        builder.Property(e => e.MedicalConditionId)
               .IsRequired();

        builder.Property(e => e.Description)
               .HasMaxLength(1000);

        builder.Property(e => e.IsDeleted)
               .HasDefaultValue(false);

        builder.Property(e => e.RowVersion)
               .IsRowVersion();

        builder.HasIndex(e => e.PatientMedicalHistoryId);
        builder.HasIndex(e => e.MedicalConditionId);

        builder.HasOne<MedicalCondition>()
            .WithMany()
            .HasForeignKey(e => e.MedicalConditionId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}
