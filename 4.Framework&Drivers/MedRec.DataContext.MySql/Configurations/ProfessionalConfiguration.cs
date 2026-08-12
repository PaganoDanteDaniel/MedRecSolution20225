using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;

public class ProfessionalConfiguration : IEntityTypeConfiguration<Professional>
{
    public void Configure(EntityTypeBuilder<Professional> builder)
    {
        builder.ToTable("Professionals");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
                .ValueGeneratedNever()
                .HasColumnType("char(36)")
                .IsRequired();

        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);

        builder.Property(p => p.Type).IsRequired();

        builder.Property(p => p.LicenseNumber)
               .HasMaxLength(50)
               .IsUnicode(false);

        builder.HasIndex(p => p.LicenseNumber).IsUnique();

        builder.Property(p => p.SpecialtyId)
            .HasColumnType("char(36)");

        builder.Property(p => p.Phone).HasMaxLength(20);

        builder.Property(p => p.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(p => p.Email);

        builder.Property(p => p.HireDate).IsRequired().HasColumnType("date");

        builder.Property(p => p.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.HasIndex(p => p.IsDeleted);

        builder.Property(p => p.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();

        builder.HasOne<MedicalSpecialty>()
            .WithMany()
            .HasForeignKey(p => p.SpecialtyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.SpecialtyId)
            .HasDatabaseName("idx_professional_specialty");
    }
}
