using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;

public class TemplateFieldDefinitionConfiguration : IEntityTypeConfiguration<TemplateFieldDefinition>
{
    public void Configure(EntityTypeBuilder<TemplateFieldDefinition> builder)
    {
        builder.ToTable("TemplateFieldDefinitions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();

        builder.Property(e => e.SpecialtyId).IsRequired();

        builder.Property(e => e.FieldName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.FieldLabel)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(e => e.FieldType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Category)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(e => e.IsRequired)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(e => e.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.SelectOptions)
            .HasColumnType("json")
            .IsRequired(false);

        builder.Property(e => e.DefaultValue)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(e => e.Unit)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(e => e.MinimumValue)
            .HasColumnType("decimal(18,4)")
            .IsRequired(false);

        builder.Property(e => e.MaximumValue)
            .HasColumnType("decimal(18,4)")
            .IsRequired(false);

        builder.Property(e => e.HelpText)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(e => e.IsVisible)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(p => p.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();

        builder.Property(e => e.CreatedAt).IsRequired();

        builder.Property(e => e.UpdatedAt).IsRequired(false);

        // Relación con MedicalSpecialty
        builder.HasOne<MedicalSpecialty>()
            .WithMany()
            .HasForeignKey(e => e.SpecialtyId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices
        builder.HasIndex(e => new { e.SpecialtyId, e.FieldName })
            .HasDatabaseName("uk_field_specialty")
            .IsUnique();

        builder.HasIndex(e => new { e.SpecialtyId, e.DisplayOrder })
            .HasDatabaseName("idx_templatefield_specialty_order");
    }
}
