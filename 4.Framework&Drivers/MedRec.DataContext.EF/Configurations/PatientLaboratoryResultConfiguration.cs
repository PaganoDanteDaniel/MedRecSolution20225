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
        builder.Property(plr => plr.IsDeleted).HasDefaultValue(false);
        builder.Property(plr => plr.RowVersion).IsRowVersion();


        builder.HasOne<LaboratoryResultType>().WithMany().HasForeignKey(plr => plr.LaboratoryResultId).OnDelete(DeleteBehavior.Restrict);

        // Relación con PatientMedicalVisitAdderDto (sin navegación)
        builder.HasOne<PatientMedicalVisit>() // No se usa propiedad de navegación
               .WithMany()                    // Sin propiedad de navegación inversa
               .HasForeignKey(plr => plr.MedicalVisitId)
               .OnDelete(DeleteBehavior.Restrict);

    }
}
