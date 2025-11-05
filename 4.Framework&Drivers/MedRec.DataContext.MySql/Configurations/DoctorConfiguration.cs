using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        // Tabla
        builder.ToTable("Doctors");

        // Clave primaria
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
                .ValueGeneratedNever()
                .HasColumnType("char(36)") // MySQL: GUID se almacena eficientemente como CHAR(36)
                .IsRequired();

        // Propiedades obligatorias
        builder.Property(d => d.FirstName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(d => d.LastName)
               .IsRequired()
               .HasMaxLength(100);

        // LicenseNumber: único y con longitud razonable
        builder.Property(d => d.LicenseNumber)
               .IsRequired()
               .HasMaxLength(50)
               .IsUnicode(false); // Evita UTF8 innecesario para códigos alfanuméricos

        builder.HasIndex(d => d.LicenseNumber)
               .IsUnique();

        // Specialty
        builder.Property(d => d.Specialty)
               .HasMaxLength(100);

        // Phone
        builder.Property(d => d.Phone)
               .HasMaxLength(20);

        // Email
        builder.Property(d => d.Email)
               .IsRequired()
               .HasMaxLength(255);

        builder.HasIndex(d => d.Email); // Útil para búsquedas, pero no necesariamente único a menos que lo requieras

        // HireDate
        builder.Property(d => d.HireDate)
               .IsRequired()
               .HasColumnType("date"); // Solo fecha, sin hora, si es coherente con tu dominio

        // Soft delete
        builder.Property(d => d.IsDeleted)
               .IsRequired()
               .HasDefaultValue(false); // Corregido: por defecto debe ser false

        builder.Property(ma => ma.RowVersion)
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();

        // Índice para soft delete (útil en consultas filtradas)
        builder.HasIndex(d => d.IsDeleted);
    }
}
