using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;

public class MedicalAppointmentConfiguration : IEntityTypeConfiguration<MedicalAppointment>
{
    public void Configure(EntityTypeBuilder<MedicalAppointment> builder)
    {
        builder.HasKey(ma => ma.Id);

        builder.Property(ma => ma.Id)
            .ValueGeneratedNever()
            .HasColumnType("char(36)") // MySQL: GUID se almacena eficientemente como CHAR(36)
            .IsRequired();

        builder.Property(ma => ma.DateTime).IsRequired();
        builder.Property(ma => ma.PatientId).IsRequired();
        builder.Property(ma => ma.DoctorId).IsRequired();
        builder.Property(ma => ma.Reason).HasMaxLength(50).IsRequired(false);

        builder.Property(ma => ma.IsDeleted).HasDefaultValue(false);

        builder.Property(ma => ma.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();

        builder.HasIndex(ma => ma.PatientId);
        builder.HasIndex(ma => ma.DoctorId);

        builder.HasOne<Patient>().WithMany().HasForeignKey(ma => ma.PatientId);
        builder.HasOne<Doctor>().WithMany().HasForeignKey(ma => ma.DoctorId);
    }
}
