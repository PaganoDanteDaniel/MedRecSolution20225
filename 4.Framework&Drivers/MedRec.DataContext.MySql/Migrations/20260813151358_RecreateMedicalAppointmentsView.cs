using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedRec.DataContext.MySql.Migrations
{
    /// <inheritdoc />
    public partial class RecreateMedicalAppointmentsView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // La vista `medicalappointmentsview` no está gestionada por EF (entidad sin clave,
            // ToView) — se creó a mano fuera de este repo hace tiempo (rastro visible en
            // __EFMigrationsHistory: 20251101233132_AddMedicalAppointmentsView, migración que ya
            // no existe en el código porque quedó absorbida por el squash a InitialCatalog). Su
            // definición original (recuperada con SHOW CREATE VIEW en la base real antes de este
            // cambio) hacía JOIN contra `doctors`/`DoctorId`, que Task 1 de este plan renombró a
            // `professionals`/`ProfessionalId` — quedaba rota tras esa migración. Se la recrea acá
            // apuntando a las tablas ya renombradas, sin cambiar los alias de salida
            // (DoctorFirstName/DoctorLastName se mantienen a propósito, igual que en
            // MedicalAppointmentView.cs).
            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW medicalappointmentsview AS
                SELECT
                    medicalappointments.Id AS Id,
                    medicalappointments.DateTime AS AppointmentDateTime,
                    patients.FirstName AS PatientFirstName,
                    patients.LastName AS PatientLastName,
                    patients.PhoneNumber AS PatientPhoneNumber,
                    medicalappointments.Reason AS Reason,
                    professionals.FirstName AS DoctorFirstName,
                    professionals.LastName AS DoctorLastName,
                    medicalappointments.RowVersion AS RowVersion,
                    medicalappointments.IsDeleted AS IsDeleted,
                    medicalappointments.PatientId AS PatientId,
                    medicalappointments.ProfessionalId AS ProfessionalId
                FROM medicalappointments
                JOIN professionals ON professionals.Id = medicalappointments.ProfessionalId
                JOIN patients ON patients.Id = medicalappointments.PatientId
                WHERE medicalappointments.IsDeleted = 0
                  AND professionals.IsDeleted = 0
                  AND patients.IsDeleted = 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE VIEW medicalappointmentsview AS
                SELECT
                    medicalappointments.Id AS Id,
                    medicalappointments.DateTime AS AppointmentDateTime,
                    patients.FirstName AS PatientFirstName,
                    patients.LastName AS PatientLastName,
                    patients.PhoneNumber AS PatientPhoneNumber,
                    medicalappointments.Reason AS Reason,
                    doctors.FirstName AS DoctorFirstName,
                    doctors.LastName AS DoctorLastName,
                    medicalappointments.RowVersion AS RowVersion,
                    medicalappointments.IsDeleted AS IsDeleted,
                    medicalappointments.PatientId AS PatientId,
                    medicalappointments.DoctorId AS DoctorId
                FROM medicalappointments
                JOIN doctors ON doctors.Id = medicalappointments.DoctorId
                JOIN patients ON patients.Id = medicalappointments.PatientId
                WHERE medicalappointments.IsDeleted = 0
                  AND doctors.IsDeleted = 0
                  AND patients.IsDeleted = 0;
            ");
        }
    }
}
