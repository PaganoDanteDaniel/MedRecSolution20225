using MedRec.MedicalAppointments.BusinessObjects.EntityView;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;
public class MedicalAppointmentsViewConfiguration : IEntityTypeConfiguration<MedicalAppointmentView>
{
    public void Configure(EntityTypeBuilder<MedicalAppointmentView> builder)
    {
        builder.HasNoKey(); // ¡Esencial! No tiene clave primaria

        builder.ToView("medicalappointmentsview"); // Nombre exacto de la vista en MySQL

        // Opcional: si quieres especificar tipos explícitos (aunque EF lo infiere bien)
        builder.Property(v => v.Id).HasColumnType("char(36)");

        builder.Property(v => v.AppointmentDateTime).HasColumnType("datetime(6)");

        builder.Property(v => v.PatientFirstName).HasMaxLength(50);

        builder.Property(v => v.PatientLastName).HasMaxLength(50);

        builder.Property(v => v.PatientPhoneNumber).HasMaxLength(20);

        builder.Property(v => v.Reason).HasMaxLength(50).IsRequired(false);

        builder.Property(v => v.DoctorFirstName).HasMaxLength(100);

        builder.Property(v => v.DoctorLastName).HasMaxLength(100);

        builder.Property(v => v.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
    }
}
