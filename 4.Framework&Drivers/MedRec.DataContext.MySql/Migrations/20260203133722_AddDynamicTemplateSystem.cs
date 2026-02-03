using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedRec.DataContext.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddDynamicTemplateSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DoctorId",
                table: "PatientMedicalVisits",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "SpecialtyId",
                table: "PatientMedicalVisits",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "MedicalSpecialties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Icon = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalSpecialties", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TemplateFieldDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    SpecialtyId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FieldName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FieldLabel = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FieldType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsRequired = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    SelectOptions = table.Column<string>(type: "json", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultValue = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Unit = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinimumValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    MaximumValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    HelpText = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateFieldDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateFieldDefinitions_MedicalSpecialties_SpecialtyId",
                        column: x => x.SpecialtyId,
                        principalTable: "MedicalSpecialties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MedicalVisitDynamicFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PatientMedicalVisitId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FieldDefinitionId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FieldValue = table.Column<string>(type: "text", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumericValue = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    DateValue = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    BooleanValue = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalVisitDynamicFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalVisitDynamicFields_PatientMedicalVisits_PatientMedica~",
                        column: x => x.PatientMedicalVisitId,
                        principalTable: "PatientMedicalVisits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MedicalVisitDynamicFields_TemplateFieldDefinitions_FieldDefi~",
                        column: x => x.FieldDefinitionId,
                        principalTable: "TemplateFieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "idx_visit_specialty",
                table: "PatientMedicalVisits",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "idx_specialty_active",
                table: "MedicalSpecialties",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "idx_specialty_name",
                table: "MedicalSpecialties",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_dynamicfield_date",
                table: "MedicalVisitDynamicFields",
                column: "DateValue");

            migrationBuilder.CreateIndex(
                name: "idx_dynamicfield_definition",
                table: "MedicalVisitDynamicFields",
                column: "FieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "idx_dynamicfield_numeric",
                table: "MedicalVisitDynamicFields",
                column: "NumericValue");

            migrationBuilder.CreateIndex(
                name: "idx_dynamicfield_visit",
                table: "MedicalVisitDynamicFields",
                column: "PatientMedicalVisitId");

            migrationBuilder.CreateIndex(
                name: "uk_visit_field",
                table: "MedicalVisitDynamicFields",
                columns: new[] { "PatientMedicalVisitId", "FieldDefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_templatefield_specialty_order",
                table: "TemplateFieldDefinitions",
                columns: new[] { "SpecialtyId", "DisplayOrder" });

            migrationBuilder.CreateIndex(
                name: "uk_field_specialty",
                table: "TemplateFieldDefinitions",
                columns: new[] { "SpecialtyId", "FieldName" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientMedicalVisits_MedicalSpecialties_SpecialtyId",
                table: "PatientMedicalVisits",
                column: "SpecialtyId",
                principalTable: "MedicalSpecialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientMedicalVisits_MedicalSpecialties_SpecialtyId",
                table: "PatientMedicalVisits");

            migrationBuilder.DropTable(
                name: "MedicalVisitDynamicFields");

            migrationBuilder.DropTable(
                name: "TemplateFieldDefinitions");

            migrationBuilder.DropTable(
                name: "MedicalSpecialties");

            migrationBuilder.DropIndex(
                name: "idx_visit_specialty",
                table: "PatientMedicalVisits");

            migrationBuilder.DropColumn(
                name: "DoctorId",
                table: "PatientMedicalVisits");

            migrationBuilder.DropColumn(
                name: "SpecialtyId",
                table: "PatientMedicalVisits");
        }
    }
}
