using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedRec.DataContext.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TABLE IF EXISTS `userroles`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `rolepermissions`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `users`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `roles`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `permissions`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `professionals`;");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "TemplateFieldDefinitions",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "TemplateFieldDefinitions",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Provinces",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Provinces",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Provinces",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Provinces",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Patients",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Patients",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Patients",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Patients",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PatientMedicalVisits",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PatientMedicalVisits",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PatientMedicalVisits",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "PatientMedicalVisits",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PatientMedicalHistories",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PatientMedicalHistories",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PatientMedicalHistories",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "PatientMedicalHistories",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PatientMedicalConditions",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PatientMedicalConditions",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PatientMedicalConditions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "PatientMedicalConditions",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PatientLaboratoryResults",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "PatientLaboratoryResults",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PatientLaboratoryResults",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "PatientLaboratoryResults",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "MedicalVisitDynamicFields",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "MedicalVisitDynamicFields",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "MedicalSpecialties",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "MedicalSpecialties",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MedicalConditionTypes",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "MedicalConditionTypes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MedicalConditionTypes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "MedicalConditionTypes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MedicalConditions",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "MedicalConditions",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MedicalConditions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "MedicalConditions",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "MedicalAppointments",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "MedicalAppointments",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "MedicalAppointments",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "MedicalAppointments",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "LaboratoryResultTypes",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "LaboratoryResultTypes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "LaboratoryResultTypes",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "LaboratoryResultTypes",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "HealthInsuranceCompanies",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "HealthInsuranceCompanies",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "HealthInsuranceCompanies",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "HealthInsuranceCompanies",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Doctors",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Doctors",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Doctors",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Doctors",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Cities",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Cities",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Cities",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UpdatedBy",
                table: "Cities",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FullName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    DoctorId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    RowVersion = table.Column<DateTime>(type: "timestamp(6)", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PermissionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.Sql(@"
                INSERT INTO Permissions (Id, Code, Description, IsDeleted, CreatedAt)
                SELECT UUID(), t.Code, t.Description, 0, UTC_TIMESTAMP(6) FROM (
                    SELECT 'patients.view' AS Code, 'Ver pacientes' AS Description
                    UNION ALL SELECT 'patients.create', 'Crear pacientes'
                    UNION ALL SELECT 'patients.edit', 'Editar pacientes'
                    UNION ALL SELECT 'patients.delete', 'Eliminar pacientes'
                    UNION ALL SELECT 'medicalvisits.view', 'Ver visitas médicas'
                    UNION ALL SELECT 'medicalvisits.create', 'Crear visitas médicas'
                    UNION ALL SELECT 'medicalvisits.edit', 'Editar visitas médicas'
                    UNION ALL SELECT 'medicalvisits.delete', 'Eliminar visitas médicas'
                    UNION ALL SELECT 'appointments.view', 'Ver turnos'
                    UNION ALL SELECT 'appointments.create', 'Crear turnos'
                    UNION ALL SELECT 'appointments.edit', 'Editar turnos'
                    UNION ALL SELECT 'appointments.delete', 'Eliminar turnos'
                    UNION ALL SELECT 'healthinsurance.view', 'Ver obras sociales'
                    UNION ALL SELECT 'healthinsurance.create', 'Crear obras sociales'
                    UNION ALL SELECT 'healthinsurance.edit', 'Editar obras sociales'
                    UNION ALL SELECT 'healthinsurance.delete', 'Eliminar obras sociales'
                    UNION ALL SELECT 'dynamictemplates.view', 'Ver plantillas de campos dinámicos'
                    UNION ALL SELECT 'dynamictemplates.create', 'Crear plantillas de campos dinámicos'
                    UNION ALL SELECT 'dynamictemplates.edit', 'Editar plantillas de campos dinámicos'
                    UNION ALL SELECT 'dynamictemplates.delete', 'Eliminar plantillas de campos dinámicos'
                    UNION ALL SELECT 'users.view', 'Ver usuarios'
                    UNION ALL SELECT 'users.create', 'Crear usuarios'
                    UNION ALL SELECT 'users.edit', 'Editar usuarios'
                    UNION ALL SELECT 'users.delete', 'Eliminar usuarios'
                    UNION ALL SELECT 'roles.view', 'Ver roles'
                    UNION ALL SELECT 'roles.create', 'Crear roles'
                    UNION ALL SELECT 'roles.edit', 'Editar roles'
                    UNION ALL SELECT 'roles.delete', 'Eliminar roles'
                ) AS t;

                INSERT INTO Roles (Id, Name, Description, IsDeleted, CreatedAt)
                VALUES (UUID(), 'Administrador', 'Rol con todos los permisos del sistema', 0, UTC_TIMESTAMP(6));

                INSERT INTO RolePermissions (RoleId, PermissionId)
                SELECT r.Id, p.Id FROM Roles r CROSS JOIN Permissions p WHERE r.Name = 'Administrador';

                INSERT INTO Users (Id, Email, PasswordHash, FullName, IsActive, DoctorId, IsDeleted, CreatedAt)
                VALUES (UUID(), 'admin@medrec.local', 'AQAAAAIAAYagAAAAEM4Lnw8E0rY7V0sfti6KzeoeErb1sMwxAqhif3qvjF+aFxkCE8vfMbBaAxqyFnhS8A==', 'Administrador del sistema', 1, NULL, 0, UTC_TIMESTAMP(6));

                INSERT INTO UserRoles (UserId, RoleId)
                SELECT u.Id, r.Id FROM Users u CROSS JOIN Roles r
                WHERE u.Email = 'admin@medrec.local' AND r.Name = 'Administrador';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TemplateFieldDefinitions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "TemplateFieldDefinitions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Provinces");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Provinces");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Provinces");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Provinces");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PatientMedicalVisits");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PatientMedicalVisits");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PatientMedicalVisits");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PatientMedicalVisits");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PatientMedicalHistories");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PatientMedicalHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PatientMedicalHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PatientMedicalHistories");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PatientMedicalConditions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PatientMedicalConditions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PatientMedicalConditions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PatientMedicalConditions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PatientLaboratoryResults");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "PatientLaboratoryResults");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PatientLaboratoryResults");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "PatientLaboratoryResults");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MedicalVisitDynamicFields");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MedicalVisitDynamicFields");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MedicalSpecialties");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MedicalSpecialties");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MedicalConditionTypes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MedicalConditionTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MedicalConditionTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MedicalConditionTypes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MedicalConditions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MedicalConditions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MedicalConditions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MedicalConditions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "MedicalAppointments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "MedicalAppointments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "MedicalAppointments");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "MedicalAppointments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "LaboratoryResultTypes");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LaboratoryResultTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "LaboratoryResultTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "LaboratoryResultTypes");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "HealthInsuranceCompanies");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "HealthInsuranceCompanies");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "HealthInsuranceCompanies");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "HealthInsuranceCompanies");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Cities");
        }
    }
}
