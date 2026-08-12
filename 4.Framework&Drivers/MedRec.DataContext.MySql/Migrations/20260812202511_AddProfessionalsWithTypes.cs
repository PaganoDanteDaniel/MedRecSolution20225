using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedRec.DataContext.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddProfessionalsWithTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Doctors",
                newName: "Professionals");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "Users",
                newName: "ProfessionalId");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "MedicalAppointments",
                newName: "ProfessionalId");
            migrationBuilder.RenameIndex(
                name: "IX_MedicalAppointments_DoctorId",
                table: "MedicalAppointments",
                newName: "IX_MedicalAppointments_ProfessionalId");

            migrationBuilder.RenameColumn(
                name: "DoctorId",
                table: "PatientMedicalVisits",
                newName: "ProfessionalId");
            migrationBuilder.RenameIndex(
                name: "idx_visit_doctor",
                table: "PatientMedicalVisits",
                newName: "idx_visit_professional");

            migrationBuilder.RenameIndex(
                name: "idx_doctor_specialty",
                table: "Professionals",
                newName: "idx_professional_specialty");
            migrationBuilder.RenameIndex(
                name: "IX_Doctors_Email",
                table: "Professionals",
                newName: "IX_Professionals_Email");
            migrationBuilder.RenameIndex(
                name: "IX_Doctors_IsDeleted",
                table: "Professionals",
                newName: "IX_Professionals_IsDeleted");
            migrationBuilder.RenameIndex(
                name: "IX_Doctors_LicenseNumber",
                table: "Professionals",
                newName: "IX_Professionals_LicenseNumber");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Professionals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Professionals",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<Guid>(
                name: "SpecialtyId",
                table: "Professionals",
                type: "char(36)",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "char(36)");

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalAppointments_Doctors_DoctorId",
                table: "MedicalAppointments");
            migrationBuilder.AddForeignKey(
                name: "FK_MedicalAppointments_Professionals_ProfessionalId",
                table: "MedicalAppointments",
                column: "ProfessionalId",
                principalTable: "Professionals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropForeignKey(
                name: "FK_PatientMedicalVisits_Doctors_DoctorId",
                table: "PatientMedicalVisits");
            migrationBuilder.AddForeignKey(
                name: "FK_PatientMedicalVisits_Professionals_ProfessionalId",
                table: "PatientMedicalVisits",
                column: "ProfessionalId",
                principalTable: "Professionals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_MedicalSpecialties_SpecialtyId",
                table: "Professionals");
            migrationBuilder.AddForeignKey(
                name: "FK_Professionals_MedicalSpecialties_SpecialtyId",
                table: "Professionals",
                column: "SpecialtyId",
                principalTable: "MedicalSpecialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Professionals_MedicalSpecialties_SpecialtyId",
                table: "Professionals");
            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_MedicalSpecialties_SpecialtyId",
                table: "Professionals",
                column: "SpecialtyId",
                principalTable: "MedicalSpecialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.DropForeignKey(
                name: "FK_PatientMedicalVisits_Professionals_ProfessionalId",
                table: "PatientMedicalVisits");
            migrationBuilder.AddForeignKey(
                name: "FK_PatientMedicalVisits_Doctors_DoctorId",
                table: "PatientMedicalVisits",
                column: "ProfessionalId",
                principalTable: "Professionals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.DropForeignKey(
                name: "FK_MedicalAppointments_Professionals_ProfessionalId",
                table: "MedicalAppointments");
            migrationBuilder.AddForeignKey(
                name: "FK_MedicalAppointments_Doctors_DoctorId",
                table: "MedicalAppointments",
                column: "ProfessionalId",
                principalTable: "Professionals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Professionals");

            migrationBuilder.AlterColumn<Guid>(
                name: "SpecialtyId",
                table: "Professionals",
                type: "char(36)",
                nullable: false,
                defaultValue: Guid.Empty,
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Professionals",
                type: "varchar(50)",
                unicode: false,
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldUnicode: false,
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.RenameIndex(
                name: "IX_Professionals_LicenseNumber",
                table: "Professionals",
                newName: "IX_Doctors_LicenseNumber");
            migrationBuilder.RenameIndex(
                name: "IX_Professionals_IsDeleted",
                table: "Professionals",
                newName: "IX_Doctors_IsDeleted");
            migrationBuilder.RenameIndex(
                name: "IX_Professionals_Email",
                table: "Professionals",
                newName: "IX_Doctors_Email");
            migrationBuilder.RenameIndex(
                name: "idx_professional_specialty",
                table: "Professionals",
                newName: "idx_doctor_specialty");

            migrationBuilder.RenameIndex(
                name: "idx_visit_professional",
                table: "PatientMedicalVisits",
                newName: "idx_visit_doctor");
            migrationBuilder.RenameColumn(
                name: "ProfessionalId",
                table: "PatientMedicalVisits",
                newName: "DoctorId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalAppointments_ProfessionalId",
                table: "MedicalAppointments",
                newName: "IX_MedicalAppointments_DoctorId");
            migrationBuilder.RenameColumn(
                name: "ProfessionalId",
                table: "MedicalAppointments",
                newName: "DoctorId");

            migrationBuilder.RenameColumn(
                name: "ProfessionalId",
                table: "Users",
                newName: "DoctorId");

            migrationBuilder.RenameTable(
                name: "Professionals",
                newName: "Doctors");
        }
    }
}
