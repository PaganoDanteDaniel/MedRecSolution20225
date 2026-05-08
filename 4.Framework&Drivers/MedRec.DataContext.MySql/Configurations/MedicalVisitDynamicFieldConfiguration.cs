using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;

public class MedicalVisitDynamicFieldConfiguration : IEntityTypeConfiguration<MedicalVisitDynamicField>
{
    public void Configure(EntityTypeBuilder<MedicalVisitDynamicField> builder)
    {
        builder.ToTable("MedicalVisitDynamicFields");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.PatientMedicalVisitId).IsRequired();

        builder.Property(e => e.FieldDefinitionId).IsRequired();

        builder.Property(e => e.FieldValue)
            .HasColumnType("text")
            .IsRequired(false);

        builder.Property(e => e.NumericValue)
            .HasColumnType("decimal(18,4)")
            .IsRequired(false);

        builder.Property(e => e.DateValue)
            .IsRequired(false);

        builder.Property(e => e.BooleanValue)
            .IsRequired(false);

        builder.Property(e => e.CreatedAt).IsRequired();

        builder.Property(e => e.UpdatedAt).IsRequired(false);

        builder.Property(e => e.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        // Relaciones
        builder.HasOne<PatientMedicalVisit>()
            .WithMany()
            .HasForeignKey(e => e.PatientMedicalVisitId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<TemplateFieldDefinition>()
            .WithMany()
            .HasForeignKey(e => e.FieldDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(e => new { e.PatientMedicalVisitId, e.FieldDefinitionId })
            .HasDatabaseName("uk_visit_field")
            .IsUnique();

        builder.HasIndex(e => e.PatientMedicalVisitId)
            .HasDatabaseName("idx_dynamicfield_visit");

        builder.HasIndex(e => e.FieldDefinitionId)
            .HasDatabaseName("idx_dynamicfield_definition");

        builder.HasIndex(e => e.NumericValue)
            .HasDatabaseName("idx_dynamicfield_numeric");

        builder.HasIndex(e => e.DateValue)
            .HasDatabaseName("idx_dynamicfield_date");
    }
}
