using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;
public class PatientLaboratoryResultConfiguration
    : IEntityTypeConfiguration<PatientLaboratoryResult>
{
    public void Configure(EntityTypeBuilder<PatientLaboratoryResult> builder)
    {
        builder.HasKey(plr => plr.Id);

        builder.Property(plr => plr.LaboratoryResultId).IsRequired();
        builder.Property(plr => plr.MedicalVisitId).IsRequired();
        builder.Property(plr => plr.ResultDate).IsRequired();
        builder.Property(plr => plr.ResultValue).HasMaxLength(50).IsRequired();
        builder.Property(plr => plr.ResultNotes).HasMaxLength(500).IsRequired(false);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);

        // Relación con LaboratoryResultType SIN navegación
        builder.HasOne<LaboratoryResultType>()
               .WithMany()
               .HasForeignKey(plr => plr.LaboratoryResultId)
               .OnDelete(DeleteBehavior.Restrict);

        // Relación con PatientMedicalVisit SIN navegación
        builder.HasOne<PatientMedicalVisit>()
               .WithMany()
               .HasForeignKey(plr => plr.MedicalVisitId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();
    }
}
