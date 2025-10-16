using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedRec.DataContext.MySql.Migrations
{
    /// <inheritdoc />
    public partial class ChangesInMedicalVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiastolicPressure",
                table: "PatientMedicalVisits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PulsePerMinute",
                table: "PatientMedicalVisits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SystolicPressure",
                table: "PatientMedicalVisits",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Temperature",
                table: "PatientMedicalVisits",
                type: "double",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiastolicPressure",
                table: "PatientMedicalVisits");

            migrationBuilder.DropColumn(
                name: "PulsePerMinute",
                table: "PatientMedicalVisits");

            migrationBuilder.DropColumn(
                name: "SystolicPressure",
                table: "PatientMedicalVisits");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "PatientMedicalVisits");
        }
    }
}
