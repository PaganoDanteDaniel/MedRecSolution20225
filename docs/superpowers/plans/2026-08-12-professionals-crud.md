# CRUD de Profesionales (generalización de Doctor) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reemplazar la tabla/entidad `Doctor` por `Professional` (con `ProfessionalType`: Doctor/Nurse/Receptionist/Administrator), construir un CRUD completo de Profesionales, y permitir crear el `User` asociado en el mismo alta.

**Architecture:** Rename atómico de `Doctor`→`Professional` en todo el solution (capas 1/3/4 existentes), seguido de una feature slice nueva `MedRec.Professionals.*` (7 proyectos, mismo patrón de capas numeradas del resto del repo) para el CRUD, con un `CreateProfessionalOrchestrator` en la capa ViewModels que compone la creación del profesional con la creación opcional de su `User` (reutilizando el `ICreateUserInputPort` de Identity), compensando con un borrado si falla el segundo paso.

**Tech Stack:** .NET 9, EF Core 9 / Pomelo MySQL, Blazor Hybrid (WPF host), xUnit + Moq.

**Spec:** `docs/superpowers/specs/2026-08-12-professionals-crud-design.md`

## Global Constraints

- Todo texto de UI/validación/error en español, siguiendo el estilo ya usado (`"Paciente no encontrado."`).
- Interactores reciben sus puertos/repos por constructor primario; ante fallo de validación llaman `outputPort.ValidationErrorsAsync(...)` y retornan; ante éxito llaman `outputPort.Handle(...)`.
- Escrituras de varios pasos van dentro de `unitOfWork.ExecuteInTransactionWithRetry(...)`.
- Los interactores de Professionals SÍ llaman `authorizationService.EnsurePermissionAsync(...)` al inicio (decisión explícita, ver spec) — esto es una excepción respecto al resto de las features no-Identity, que todavía no lo hacen.
- Nuevos casos de uso se registran vía `AddProfessionalsUseCasesServicesWithProxy()` para quedar envueltos por `UseCaseExceptionProxy` — sin try/catch manual en los interactores para excepciones de infraestructura.
- Orquestadores/Actions/Presenters devuelven `OperationResult<T>` (`MedRec.BusinessObjects.Results`), nunca excepciones, hacia la capa ViewModels.
- Registros de DI van en el `DependencyContainer.cs` propio de cada proyecto, encadenados luego en `MedRec.IoC\DependencyContainer.AddAppServices()`.
- `TargetFramework` de todo proyecto nuevo: `net9.0` (Razor: mismo, con `Microsoft.NET.Sdk.Razor`).

---

## Task 1: Rename global de Doctor a Professional (capas 1/3/4 existentes)

Este task es un refactor atómico: ningún paso intermedio compila hasta que todas las referencias a `Doctor`/`DoctorId` de la solución quedan renombradas. Por eso va como un solo task grande, cerrado por un build verde + tests existentes en verde, en vez de fragmentarse en sub-tasks que dejarían el repo roto entre commits.

**Files:**
- Modify → Rename: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\Doctor.cs` → `Professional.cs`
- Create: `1.EnterpriseBusinessObjects\MedRec.Entity\Enums\ProfessionalType.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\User.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\MedicalAppointment.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\PatientMedicalVisit.cs`
- Modify → Rename: `4.Framework&Drivers\MedRec.DataContext.MySql\Configurations\DoctorConfiguration.cs` → `ProfessionalConfiguration.cs`
- Modify: `4.Framework&Drivers\MedRec.DataContext.MySql\Configurations\MedicalAppointmentConfiguration.cs`
- Modify: `4.Framework&Drivers\MedRec.DataContext.MySql\Configurations\MedicalVisitConfiguration.cs`
- Modify: `4.Framework&Drivers\MedRec.DataContext.MySql\DataContext\MedRecContext.cs`
- Create: `4.Framework&Drivers\MedRec.DataContext.MySql\Migrations\<timestamp>_AddProfessionalsWithTypes.cs` (generado + reescrito a mano)
- Modify → Rename (Identity, capa 2): `DoctorSummaryDto.cs` → `ProfessionalSummaryDto.cs`, `IDoctorLookupRepository.cs` → `IProfessionalLookupRepository.cs`
- Modify → Rename (Identity, capa 3): `IDoctorLookupDataContext.cs` → `IProfessionalLookupDataContext.cs`, `DoctorLookupRepository.cs` → `ProfessionalLookupRepository.cs`
- Modify → Rename (Identity, capa 4): `DoctorLookupDataContextMySql.cs` → `ProfessionalLookupDataContextMySql.cs`
- Modify: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\CreateUserDto.cs`, `UpdateUserDto.cs`
- Modify: `2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\CreateUserInteractor.cs`, `UpdateUserInteractor.cs`
- Modify: `3.InterfaceAdapters\MedRec.Identity.ViewModels\Models\CreateUserModel.cs`, `UpdateUserModel.cs`
- Modify: `3.InterfaceAdapters\MedRec.Identity.Views\Pages\CreateUserPage.razor`, `UpdateUserPage.razor`
- Modify: `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\DependencyContainer.cs`, `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\DependencyContainer.cs`
- Modify (mecánico, ver script Step 8): los ~20 archivos de `MedicalAppointments`/`MedicalVisit` listados en Step 8.

**Interfaces:**
- Produces: `Professional` (entidad), `ProfessionalType` (enum: `Doctor=0, Nurse=1, Receptionist=2, Administrator=3`), `User.ProfessionalId`, `MedicalAppointment.ProfessionalId`, `PatientMedicalVisit.ProfessionalId`, `IProfessionalLookupRepository.ListActiveAsync()` (sigue filtrando `Type == Doctor`, mismo comportamiento que antes).

- [ ] **Step 1: Renombrar la entidad y agregar el enum**

Crear `1.EnterpriseBusinessObjects\MedRec.Entity\Enums\ProfessionalType.cs`:

```csharp
namespace MedRec.Entity.Enums;

public enum ProfessionalType
{
    Doctor = 0,
    Nurse = 1,
    Receptionist = 2,
    Administrator = 3
}
```

Eliminar `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\Doctor.cs` y crear `Professional.cs` en su lugar:

```csharp
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;

public class Professional : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public ProfessionalType Type { get; set; }
    public string? LicenseNumber { get; set; }
    public Guid? SpecialtyId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string FullName => $"{LastName}, {FirstName}";
}
```

Nota: `IsDeleted` pasa de `= true` (default de `Doctor.cs`, nunca ejercitado porque no existía alta de Doctors) a `= false` — es la corrección de un bug latente: con `= true`, cualquier profesional nuevo nacería invisible en los listados filtrados por `!IsDeleted`.

- [ ] **Step 2: Renombrar `DoctorId` en las 3 entidades que lo referencian**

En `User.cs`, reemplazar:
```csharp
public Guid? DoctorId { get; set; }
```
por:
```csharp
public Guid? ProfessionalId { get; set; }
```

En `MedicalAppointment.cs`, reemplazar:
```csharp
public Guid DoctorId { get; set; } = Guid.Empty;
```
por:
```csharp
public Guid ProfessionalId { get; set; } = Guid.Empty;
```

En `PatientMedicalVisit.cs`, reemplazar:
```csharp
public Guid? DoctorId { get; set; }
```
por:
```csharp
public Guid? ProfessionalId { get; set; }
```

- [ ] **Step 3: Build de capa 1 para confirmar que compila sola**

Run: `dotnet build "1.EnterpriseBusinessObjects\MedRec.Entity\MedRec.Entity.csproj"`
Expected: FAIL (capa 4 todavía referencia `Doctor`/`DoctorId` — es esperado en este punto intermedio, seguir).

- [ ] **Step 4: Renombrar la configuración EF y actualizar las que referencian Doctor**

Eliminar `DoctorConfiguration.cs`, crear `4.Framework&Drivers\MedRec.DataContext.MySql\Configurations\ProfessionalConfiguration.cs`:

```csharp
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
```

En `MedicalAppointmentConfiguration.cs`, reemplazar las 3 líneas que usan `DoctorId`/`Doctor`:
```csharp
        builder.Property(ma => ma.DoctorId).IsRequired();
```
→
```csharp
        builder.Property(ma => ma.ProfessionalId).IsRequired();
```
```csharp
        builder.HasIndex(ma => ma.DoctorId);
```
→
```csharp
        builder.HasIndex(ma => ma.ProfessionalId);
```
```csharp
        builder.HasOne<Doctor>().WithMany().HasForeignKey(ma => ma.DoctorId);
```
→
```csharp
        builder.HasOne<Professional>().WithMany().HasForeignKey(ma => ma.ProfessionalId);
```

En `MedicalVisitConfiguration.cs`, reemplazar:
```csharp
        builder.Property(e => e.DoctorId).IsRequired(false);

        builder.HasOne<Doctor>()
            .WithMany()
            .HasForeignKey(e => e.DoctorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.DoctorId)
            .HasDatabaseName("idx_visit_doctor");
```
por:
```csharp
        builder.Property(e => e.ProfessionalId).IsRequired(false);

        builder.HasOne<Professional>()
            .WithMany()
            .HasForeignKey(e => e.ProfessionalId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(e => e.ProfessionalId)
            .HasDatabaseName("idx_visit_professional");
```

En `MedRecContext.cs`, reemplazar:
```csharp
    public DbSet<Doctor> Doctors { get; set; }
```
por:
```csharp
    public DbSet<Professional> Professionals { get; set; }
```

- [ ] **Step 5: Generar el esqueleto de la migración**

Run: `dotnet ef migrations add AddProfessionalsWithTypes --project "4.Framework&Drivers\MedRec.DataContext.MySql" --startup-project "4.Framework&Drivers\MedRec.WPF.UI"`
Expected: se crea `4.Framework&Drivers\MedRec.DataContext.MySql\Migrations\<timestamp>_AddProfessionalsWithTypes.cs` (y su `.Designer.cs`), además de actualizar `MedRecContextModelSnapshot.cs` automáticamente. **No usar el `Up()`/`Down()` que genera** — probablemente scaffolde un `DropTable("Doctors")` + `CreateTable("Professionals")` (pérdida de datos), que reemplazamos en el siguiente paso.

- [ ] **Step 6: Reemplazar el cuerpo de la migración a mano**

Abrir el archivo `<timestamp>_AddProfessionalsWithTypes.cs` recién generado y reemplazar los métodos `Up`/`Down` completos por:

```csharp
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
```

- [ ] **Step 7: Renombrar el lookup de Identity (Doctor → Professional, filtrado por Type==Doctor)**

Eliminar `DoctorSummaryDto.cs`, crear `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\ProfessionalSummaryDto.cs`:
```csharp
namespace MedRec.Identity.BusinessObjects.DTOs;
public class ProfessionalSummaryDto
{
    public ProfessionalSummaryDto(Guid id, string fullName)
    {
        Id = id;
        FullName = fullName;
    }

    public Guid Id { get; }
    public string FullName { get; }
}
```

Eliminar `IDoctorLookupRepository.cs`, crear `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Repositories\IProfessionalLookupRepository.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Repositories;
public interface IProfessionalLookupRepository
{
    Task<IReadOnlyList<ProfessionalSummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
```

Eliminar `IDoctorLookupDataContext.cs`, crear `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\Interfaces\IProfessionalLookupDataContext.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.Repositories.Interfaces;
public interface IProfessionalLookupDataContext
{
    Task<IReadOnlyList<ProfessionalSummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
```

Eliminar `DoctorLookupRepository.cs`, crear `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\Implementations\ProfessionalLookupRepository.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.Repositories.Interfaces;

namespace MedRec.Identity.Repositories.Implementations;
internal class ProfessionalLookupRepository(IProfessionalLookupDataContext dataContext) : IProfessionalLookupRepository
{
    public Task<IReadOnlyList<ProfessionalSummaryDto>> ListActiveAsync(CancellationToken ct = default) =>
        dataContext.ListActiveAsync(ct);
}
```

Eliminar `DoctorLookupDataContextMySql.cs`, crear `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\Services\ProfessionalLookupDataContextMySql.cs` (preserva el comportamiento actual — el combo solo debe listar médicos):
```csharp
using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.Enums;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class ProfessionalLookupDataContextMySql(MedRecContext context) : IProfessionalLookupDataContext
{
    public async Task<IReadOnlyList<ProfessionalSummaryDto>> ListActiveAsync(CancellationToken ct = default)
    {
        return await context.Professionals
            .Where(p => !p.IsDeleted && p.Type == ProfessionalType.Doctor)
            .Select(p => new ProfessionalSummaryDto(p.Id, p.LastName + ", " + p.FirstName))
            .ToListAsync(ct);
    }
}
```

En `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\DependencyContainer.cs`, reemplazar:
```csharp
        services.AddScoped<IDoctorLookupRepository, DoctorLookupRepository>();
```
por:
```csharp
        services.AddScoped<IProfessionalLookupRepository, ProfessionalLookupRepository>();
```

En `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\DependencyContainer.cs`, reemplazar:
```csharp
        services.AddScoped<IDoctorLookupDataContext, DoctorLookupDataContextMySql>();
```
por:
```csharp
        services.AddScoped<IProfessionalLookupDataContext, ProfessionalLookupDataContextMySql>();
```

- [ ] **Step 8: Rename mecánico de `DoctorId` en el resto de la solución**

Ejecutar este script PowerShell (reemplazo literal de texto, un archivo a la vez) sobre esta lista exacta de archivos:

```powershell
$files = @(
  "2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\CreateUserDto.cs",
  "2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\UpdateUserDto.cs",
  "2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\AuthResultDto.cs",
  "2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\CreateUserInteractor.cs",
  "2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\UpdateUserInteractor.cs",
  "2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\AuthenticateUserInteractor.cs",
  "3.InterfaceAdapters\MedRec.Identity.ViewModels\Models\CreateUserModel.cs",
  "3.InterfaceAdapters\MedRec.Identity.ViewModels\Models\UpdateUserModel.cs",
  "4.Framework&Drivers\MedRec.Identity.DataContext.MySql\Services\SessionService.cs",
  "Test\MedRec.Identity.UseCases.Tests\UpdateUserInteractorTests.cs",
  "Test\MedRec.Identity.UseCases.Tests\AuthenticateUserInteractorTests.cs",
  "2.ApplicationBusinessObjects\MedRec.MedicalVisit.BusinessObjects\DTOs\CreateMedicalVisitDto.cs",
  "3.InterfaceAdapters\MedRec.MedicalAppointments.Views\Components\WeeklyScheduleComonet.razor.cs",
  "3.InterfaceAdapters\MedRec.MedicalAppointments.Views\Components\AppointmentScheduleComponent.razor.cs",
  "3.InterfaceAdapters\MedRec.MedicalAppointments.ViewModels\VM\WeeklyScheduleViewModelOrchestrator.cs",
  "3.InterfaceAdapters\MedRec.MedicalAppointments.ViewModels\Orchestration\AppointmentMapper.cs",
  "3.InterfaceAdapters\MedRec.MedicalAppointments.ViewModels\Orchestration\Actions\ReassignAppointmentAction.cs",
  "3.InterfaceAdapters\MedRec.MedicalAppointments.ViewModels\Models\Appointment.cs",
  "2.ApplicationBusinessObjects\MedRec.MedicalAppointments.UseCases\Implementations\ReassignMedicalAppointmentInteractor.cs",
  "2.ApplicationBusinessObjects\MedRec.MedicalAppointments.UseCases\Implementations\CreateMedicalAppointmentInteractor.cs",
  "2.ApplicationBusinessObjects\MedRec.MedicalAppointment.BusinessObjects\EntityView\MedicalAppointmentView.cs",
  "2.ApplicationBusinessObjects\MedRec.MedicalAppointment.BusinessObjects\DTOs\ReassignMedicalAppointmentDto.cs",
  "2.ApplicationBusinessObjects\MedRec.MedicalAppointment.BusinessObjects\DTOs\MedicalAppointmentDto.cs",
  "2.ApplicationBusinessObjects\MedRec.MedicalAppointment.BusinessObjects\DTOs\CreateMedicalAppointmentDto.cs"
)

foreach ($f in $files) {
    if (Test-Path $f) {
        (Get-Content $f -Raw) -replace 'DoctorId', 'ProfessionalId' | Set-Content $f -Encoding utf8 -NoNewline
    } else {
        Write-Warning "No existe: $f"
    }
}
```

Nota: `Presenters\Implementations\CreateMedicalAppointmentPresenter.cs`, `GetMedicalAppointmentsPresenter.cs`, `ReassignMedicalAppointmentPresenter.cs`, `MoveMedicalAppointmentPresenter.cs` aparecieron en la búsqueda original pero no referencian `DoctorId` directamente (falso positivo por la palabra "Doctor" en otro contexto) — no tocar salvo que el build del Step 9 marque error ahí.

- [ ] **Step 9: Build completo y loop de corrección**

Run: `dotnet build MedRecSolution2025.sln`

Si hay errores de compilación, cada uno será una referencia a `Doctor`/`DoctorId`/`IDoctorLookupRepository`/`DoctorSummaryDto` no cubierta por los steps anteriores (por ejemplo, un `using MedRec.Identity.BusinessObjects.Interfaces.Repositories;` con `IDoctorLookupRepository` en un archivo no listado). Para cada error: aplicar el mismo mapeo (`Doctor`→`Professional`, `DoctorId`→`ProfessionalId`, `IDoctorLookupRepository`→`IProfessionalLookupRepository`, `DoctorSummaryDto`→`ProfessionalSummaryDto`) en ese archivo puntual, y repetir el build. Expected al final: `Build succeeded. 0 Error(s)` (los warnings preexistentes no son bloqueantes).

- [ ] **Step 10: Correr los tests existentes**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: PASS (todos verdes; si algún test referenciaba `DoctorId` en un `new User { ... }`, ya quedó renombrado por el Step 8).

Run: `dotnet test "Test\MedRec.MedicalVisit.UseCases.Tests\MedRec.MedicalVisit.UseCases.Tests.csproj"`
Expected: PASS.

- [ ] **Step 11: Commit**

```bash
git add -A
git commit -m "refactor(professionals): renombrar Doctor a Professional en toda la solución

Rename atómico de la entidad, migración EF (RenameTable/RenameColumn/AlterColumn,
sin pérdida de datos), configuraciones, lookup de Identity y todos los consumidores
en MedicalAppointments/MedicalVisit. Base para el CRUD de Profesionales."
```

---

## Task 2: Scaffolding de los proyectos nuevos `MedRec.Professionals.*`

**Files:**
- Create (via `dotnet new` + `dotnet sln add`): `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\MedRec.Professionals.BusinessObjects.csproj`
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\MedRec.Professionals.UseCases.csproj`
- Create: `3.InterfaceAdapters\Repositories\MedRec.Professionals.Repositories\MedRec.Professionals.Repositories.csproj`
- Create: `3.InterfaceAdapters\MedRec.Professionals.Presenters\MedRec.Professionals.Presenters.csproj`
- Create: `3.InterfaceAdapters\MedRec.Professionals.ViewModels\MedRec.Professionals.ViewModels.csproj`
- Create: `3.InterfaceAdapters\MedRec.Professionals.Views\MedRec.Professionals.Views.csproj`
- Create: `4.Framework&Drivers\MedRec.Professionals.DataContext.MySql\MedRec.Professionals.DataContext.MySql.csproj`
- Create: `Test\MedRec.Professionals.UseCases.Tests\MedRec.Professionals.UseCases.Tests.csproj`

- [ ] **Step 1: Crear los 8 proyectos con `dotnet new` y agregarlos al .sln**

```bash
dotnet new classlib -n MedRec.Professionals.BusinessObjects -o "2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects"
dotnet new classlib -n MedRec.Professionals.UseCases -o "2.ApplicationBusinessObjects\MedRec.Professionals.UseCases"
dotnet new classlib -n MedRec.Professionals.Repositories -o "3.InterfaceAdapters\Repositories\MedRec.Professionals.Repositories"
dotnet new classlib -n MedRec.Professionals.Presenters -o "3.InterfaceAdapters\MedRec.Professionals.Presenters"
dotnet new classlib -n MedRec.Professionals.ViewModels -o "3.InterfaceAdapters\MedRec.Professionals.ViewModels"
dotnet new razorclasslib -n MedRec.Professionals.Views -o "3.InterfaceAdapters\MedRec.Professionals.Views"
dotnet new classlib -n MedRec.Professionals.DataContext.MySql -o "4.Framework&Drivers\MedRec.Professionals.DataContext.MySql"
dotnet new xunit -n MedRec.Professionals.UseCases.Tests -o "Test\MedRec.Professionals.UseCases.Tests"

dotnet sln MedRecSolution2025.sln add "2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\MedRec.Professionals.BusinessObjects.csproj"
dotnet sln MedRecSolution2025.sln add "2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\MedRec.Professionals.UseCases.csproj"
dotnet sln MedRecSolution2025.sln add "3.InterfaceAdapters\Repositories\MedRec.Professionals.Repositories\MedRec.Professionals.Repositories.csproj"
dotnet sln MedRecSolution2025.sln add "3.InterfaceAdapters\MedRec.Professionals.Presenters\MedRec.Professionals.Presenters.csproj"
dotnet sln MedRecSolution2025.sln add "3.InterfaceAdapters\MedRec.Professionals.ViewModels\MedRec.Professionals.ViewModels.csproj"
dotnet sln MedRecSolution2025.sln add "3.InterfaceAdapters\MedRec.Professionals.Views\MedRec.Professionals.Views.csproj"
dotnet sln MedRecSolution2025.sln add "4.Framework&Drivers\MedRec.Professionals.DataContext.MySql\MedRec.Professionals.DataContext.MySql.csproj"
dotnet sln MedRecSolution2025.sln add "Test\MedRec.Professionals.UseCases.Tests\MedRec.Professionals.UseCases.Tests.csproj"
```

Borrar los archivos placeholder que genera cada `dotnet new` (`Class1.cs`, y en el razorclasslib `Component1.razor`/`ExampleJsInterop.cs`/`wwwroot\exampleJsInterop.js` si se crearon) — el contenido real se agrega en los tasks siguientes.

- [ ] **Step 2: Sobrescribir cada `.csproj` con las referencias correctas**

`2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\MedRec.Professionals.BusinessObjects.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\1.EnterpriseBusinessObjects\MedRec.Entity\MedRec.Entity.csproj" />
    <ProjectReference Include="..\..\1.EnterpriseBusinessObjects\MedRec.Shared\MedRec.Shared.csproj" />
    <ProjectReference Include="..\MedRec.BusinessObjects\MedRec.BusinessObjects.csproj" />
    <ProjectReference Include="..\MedRec.Identity.BusinessObjects\MedRec.Identity.BusinessObjects.csproj" />
  </ItemGroup>

</Project>
```

`2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\MedRec.Professionals.UseCases.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.9" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MedRec.Professionals.BusinessObjects\MedRec.Professionals.BusinessObjects.csproj" />
  </ItemGroup>

</Project>
```

`3.InterfaceAdapters\Repositories\MedRec.Professionals.Repositories\MedRec.Professionals.Repositories.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\MedRec.Professionals.BusinessObjects.csproj" />
  </ItemGroup>

</Project>
```

`3.InterfaceAdapters\MedRec.Professionals.Presenters\MedRec.Professionals.Presenters.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="9.0.9" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\MedRec.Professionals.BusinessObjects.csproj" />
  </ItemGroup>

</Project>
```

`3.InterfaceAdapters\MedRec.Professionals.ViewModels\MedRec.Professionals.ViewModels.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <ItemGroup>
    <ProjectReference Include="..\..\2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\MedRec.Professionals.BusinessObjects.csproj" />
    <ProjectReference Include="..\..\2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\MedRec.Identity.BusinessObjects.csproj" />
  </ItemGroup>

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

</Project>
```

`3.InterfaceAdapters\MedRec.Professionals.Views\MedRec.Professionals.Views.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <SupportedPlatform Include="browser" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.6" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MedRec.Professionals.ViewModels\MedRec.Professionals.ViewModels.csproj" />
    <ProjectReference Include="..\MedRec.Identity.Views\MedRec.Identity.Views.csproj" />
  </ItemGroup>

</Project>
```
(La referencia a `MedRec.Identity.Views` es para poder usar `<RoleCheckboxList>` desde `CreateProfessionalPage.razor` — ver Task 12.)

`4.Framework&Drivers\MedRec.Professionals.DataContext.MySql\MedRec.Professionals.DataContext.MySql.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\3.InterfaceAdapters\Repositories\MedRec.Professionals.Repositories\MedRec.Professionals.Repositories.csproj" />
    <ProjectReference Include="..\MedRec.DataContext.MySql\MedRec.DataContext.MySql.csproj" />
  </ItemGroup>

</Project>
```

`Test\MedRec.Professionals.UseCases.Tests\MedRec.Professionals.UseCases.Tests.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.2" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="Moq" Version="4.20.72" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\MedRec.Professionals.BusinessObjects.csproj" />
    <ProjectReference Include="..\..\2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\MedRec.Professionals.UseCases.csproj" />
    <ProjectReference Include="..\..\3.InterfaceAdapters\MedRec.Professionals.ViewModels\MedRec.Professionals.ViewModels.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 3: Build para confirmar que los proyectos vacíos compilan y referencian bien**

Run: `dotnet build MedRecSolution2025.sln`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 4: Commit**

```bash
git add -A
git commit -m "chore(professionals): scaffolding de los 8 proyectos de la feature slice Professionals"
```

---

## Task 3: Permisos nuevos `Professionals_*`

**Files:**
- Modify: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Constants\SystemPermissions.cs`

**Interfaces:**
- Produces: `SystemPermissions.Professionals_View/Create/Edit/Delete` (strings), incluidos en `SystemPermissions.All`.

- [ ] **Step 1: Agregar las 4 constantes y sus entradas en `All`**

En `SystemPermissions.cs`, después del bloque de `Roles_*` (línea 38), agregar:
```csharp

    public const string Professionals_View = "professionals.view";
    public const string Professionals_Create = "professionals.create";
    public const string Professionals_Edit = "professionals.edit";
    public const string Professionals_Delete = "professionals.delete";
```

Y en el array `All`, después de `(Roles_Delete, "Eliminar roles"),`, agregar:
```csharp
        (Professionals_View, "Ver profesionales"),
        (Professionals_Create, "Crear profesionales"),
        (Professionals_Edit, "Editar profesionales"),
        (Professionals_Delete, "Eliminar profesionales"),
```

- [ ] **Step 2: Build**

Run: `dotnet build "2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\MedRec.Identity.BusinessObjects.csproj"`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 3: Commit**

```bash
git add "2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Constants\SystemPermissions.cs"
git commit -m "feat(professionals): agregar permisos Professionals_View/Create/Edit/Delete"
```

---

## Task 4: Capa 2 (BusinessObjects) — DTOs, Validators, Ports, `IProfessionalRepositoryUoW`

**Files:**
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\DTOs\CreateProfessionalDto.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\DTOs\UpdateProfessionalDto.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\DTOs\ProfessionalDto.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\DTOs\SpecialtySummaryDto.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\Validators\CreateProfessionalValidator.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\Validators\UpdateProfessionalValidator.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\Interfaces\Repositories\IProfessionalRepositoryUoW.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\Interfaces\Repositories\ISpecialtyLookupRepository.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\Interfaces\Ports\ICreateProfessionalInputPort.cs` (+ `OutputPort`)
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\Interfaces\Ports\IUpdateProfessionalInputPort.cs` (+ `OutputPort`)
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\Interfaces\Ports\IDeleteProfessionalInputPort.cs` (+ `OutputPort`)
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\Interfaces\Ports\IListProfessionalsInputPort.cs` (+ `OutputPort`)
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\Interfaces\Ports\IGetProfessionalByIdInputPort.cs` (+ `OutputPort`)

**Interfaces:**
- Produces: `CreateProfessionalDto(string firstName, string lastName, string email, string? phone, DateTime hireDate, ProfessionalType type, string? licenseNumber, Guid? specialtyId)`; `UpdateProfessionalDto(Guid id, string firstName, string lastName, string phone, ProfessionalType type, string? licenseNumber, Guid? specialtyId, byte[] rowVersion)`; `ProfessionalDto(Guid id, string firstName, string lastName, string email, string phone, DateTime hireDate, ProfessionalType type, string? licenseNumber, Guid? specialtyId, byte[] rowVersion)` con `.FullName`; `SpecialtySummaryDto(Guid id, string name)`; `IProfessionalRepositoryUoW` con `GetByIdAsync/GetByEmailAsync/ListAsync/CreateAsync/UpdateAsync/SoftDeleteAsync`; `ISpecialtyLookupRepository.ListActiveAsync()`.

- [ ] **Step 1: DTOs**

`DTOs\CreateProfessionalDto.cs`:
```csharp
using MedRec.Entity.Enums;

namespace MedRec.Professionals.BusinessObjects.DTOs;
public class CreateProfessionalDto
{
    public CreateProfessionalDto(
        string firstName,
        string lastName,
        string email,
        string? phone,
        DateTime hireDate,
        ProfessionalType type,
        string? licenseNumber,
        Guid? specialtyId)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        HireDate = hireDate;
        Type = type;
        LicenseNumber = licenseNumber;
        SpecialtyId = specialtyId;
    }

    public string FirstName { get; }
    public string LastName { get; }
    public string Email { get; }
    public string? Phone { get; }
    public DateTime HireDate { get; }
    public ProfessionalType Type { get; }
    public string? LicenseNumber { get; }
    public Guid? SpecialtyId { get; }
}
```

`DTOs\UpdateProfessionalDto.cs` (sin `Email`: no editable, mismo criterio ya aplicado a `User.Email`):
```csharp
using MedRec.Entity.Enums;

namespace MedRec.Professionals.BusinessObjects.DTOs;
public class UpdateProfessionalDto
{
    public UpdateProfessionalDto(
        Guid id,
        string firstName,
        string lastName,
        string phone,
        ProfessionalType type,
        string? licenseNumber,
        Guid? specialtyId,
        byte[] rowVersion)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        Type = type;
        LicenseNumber = licenseNumber;
        SpecialtyId = specialtyId;
        RowVersion = rowVersion;
    }

    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Phone { get; }
    public ProfessionalType Type { get; }
    public string? LicenseNumber { get; }
    public Guid? SpecialtyId { get; }
    public byte[] RowVersion { get; }
}
```

`DTOs\ProfessionalDto.cs`:
```csharp
using MedRec.Entity.Enums;

namespace MedRec.Professionals.BusinessObjects.DTOs;
public class ProfessionalDto
{
    public ProfessionalDto(
        Guid id,
        string firstName,
        string lastName,
        string email,
        string phone,
        DateTime hireDate,
        ProfessionalType type,
        string? licenseNumber,
        Guid? specialtyId,
        byte[] rowVersion)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Phone = phone;
        HireDate = hireDate;
        Type = type;
        LicenseNumber = licenseNumber;
        SpecialtyId = specialtyId;
        RowVersion = rowVersion;
    }

    public Guid Id { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string FullName => $"{LastName}, {FirstName}";
    public string Email { get; }
    public string Phone { get; }
    public DateTime HireDate { get; }
    public ProfessionalType Type { get; }
    public string? LicenseNumber { get; }
    public Guid? SpecialtyId { get; }
    public byte[] RowVersion { get; }
}
```

`DTOs\SpecialtySummaryDto.cs`:
```csharp
namespace MedRec.Professionals.BusinessObjects.DTOs;
public class SpecialtySummaryDto
{
    public SpecialtySummaryDto(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; }
    public string Name { get; }
}
```

- [ ] **Step 2: Validators**

`Validators\CreateProfessionalValidator.cs`:
```csharp
using MedRec.Entity.Enums;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.Professionals.BusinessObjects.Validators;
public static class CreateProfessionalValidator
{
    public static IReadOnlyList<ValidationError> Validate(CreateProfessionalDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        errors.AddRange(Guard.Against(dto.FirstName, nameof(dto.FirstName))
            .NotNullOrEmpty("El nombre es obligatorio.").Errors);

        errors.AddRange(Guard.Against(dto.LastName, nameof(dto.LastName))
            .NotNullOrEmpty("El apellido es obligatorio.").Errors);

        errors.AddRange(Guard.Against(dto.Email, nameof(dto.Email))
            .NotNullOrEmpty("El email es obligatorio.")
            .InvalidEmail("El email no tiene un formato válido.").Errors);

        errors.AddRange(Guard.Against(dto.Type, nameof(dto.Type)).IsDefined().Errors);

        if (dto.Type == ProfessionalType.Doctor || dto.Type == ProfessionalType.Nurse)
        {
            errors.AddRange(Guard.Against(dto.LicenseNumber ?? string.Empty, nameof(dto.LicenseNumber))
                .NotNullOrEmpty("La matrícula es obligatoria para este tipo de profesional.").Errors);
        }

        if (dto.Type == ProfessionalType.Doctor)
        {
            errors.AddRange(Guard.Against(dto.SpecialtyId ?? Guid.Empty, nameof(dto.SpecialtyId))
                .NotNullOrEmpty("La especialidad es obligatoria para médicos.").Errors);
        }

        return errors;
    }
}
```

`Validators\UpdateProfessionalValidator.cs`:
```csharp
using MedRec.Entity.Enums;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.Professionals.BusinessObjects.Validators;
public static class UpdateProfessionalValidator
{
    public static IReadOnlyList<ValidationError> Validate(UpdateProfessionalDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        errors.AddRange(Guard.Against(dto.FirstName, nameof(dto.FirstName))
            .NotNullOrEmpty("El nombre es obligatorio.").Errors);

        errors.AddRange(Guard.Against(dto.LastName, nameof(dto.LastName))
            .NotNullOrEmpty("El apellido es obligatorio.").Errors);

        errors.AddRange(Guard.Against(dto.Type, nameof(dto.Type)).IsDefined().Errors);

        if (dto.Type == ProfessionalType.Doctor || dto.Type == ProfessionalType.Nurse)
        {
            errors.AddRange(Guard.Against(dto.LicenseNumber ?? string.Empty, nameof(dto.LicenseNumber))
                .NotNullOrEmpty("La matrícula es obligatoria para este tipo de profesional.").Errors);
        }

        if (dto.Type == ProfessionalType.Doctor)
        {
            errors.AddRange(Guard.Against(dto.SpecialtyId ?? Guid.Empty, nameof(dto.SpecialtyId))
                .NotNullOrEmpty("La especialidad es obligatoria para médicos.").Errors);
        }

        return errors;
    }
}
```

- [ ] **Step 3: `IProfessionalRepositoryUoW` e `ISpecialtyLookupRepository`**

`Interfaces\Repositories\IProfessionalRepositoryUoW.cs`:
```csharp
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
public interface IProfessionalRepositoryUoW
{
    Task<Professional?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Professional?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<ProfessionalDto>> ListAsync(ProfessionalType? typeFilter, CancellationToken ct = default);
    Task CreateAsync(Professional professional, CancellationToken ct = default);
    Task UpdateAsync(Professional professional, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
```

`Interfaces\Repositories\ISpecialtyLookupRepository.cs`:
```csharp
using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
public interface ISpecialtyLookupRepository
{
    Task<IReadOnlyList<SpecialtySummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
```

- [ ] **Step 4: Ports (5 casos de uso, input+output = 10 archivos)**

`Interfaces\Ports\ICreateProfessionalInputPort.cs`:
```csharp
using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface ICreateProfessionalInputPort
{
    Task HandleAsync(CreateProfessionalDto dto, CancellationToken ct = default);
}
```

`Interfaces\Ports\ICreateProfessionalOutputPort.cs`:
```csharp
using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface ICreateProfessionalOutputPort : IBaseOutputPort
{
    OperationResult<Guid> Result { get; }
    Task Handle(Guid professionalId, CancellationToken ct = default);
}
```

`Interfaces\Ports\IUpdateProfessionalInputPort.cs`:
```csharp
using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IUpdateProfessionalInputPort
{
    Task HandleAsync(UpdateProfessionalDto dto, CancellationToken ct = default);
}
```

`Interfaces\Ports\IUpdateProfessionalOutputPort.cs`:
```csharp
using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IUpdateProfessionalOutputPort : IBaseOutputPort
{
    OperationResult<bool> Result { get; }
    Task Handle(CancellationToken ct = default);
}
```

`Interfaces\Ports\IDeleteProfessionalInputPort.cs`:
```csharp
namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IDeleteProfessionalInputPort
{
    Task HandleAsync(Guid id, CancellationToken ct = default);
}
```

`Interfaces\Ports\IDeleteProfessionalOutputPort.cs`:
```csharp
using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IDeleteProfessionalOutputPort : IBaseOutputPort
{
    OperationResult<bool> Result { get; }
    Task Handle(CancellationToken ct = default);
}
```

`Interfaces\Ports\IListProfessionalsInputPort.cs`:
```csharp
using MedRec.Entity.Enums;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IListProfessionalsInputPort
{
    Task HandleAsync(ProfessionalType? typeFilter, CancellationToken ct = default);
}
```

`Interfaces\Ports\IListProfessionalsOutputPort.cs`:
```csharp
using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IListProfessionalsOutputPort : IBaseOutputPort
{
    OperationResult<IReadOnlyList<ProfessionalDto>> Result { get; }
    Task Handle(IReadOnlyList<ProfessionalDto> professionals, CancellationToken ct = default);
}
```

`Interfaces\Ports\IGetProfessionalByIdInputPort.cs`:
```csharp
namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IGetProfessionalByIdInputPort
{
    Task HandleAsync(Guid id, CancellationToken ct = default);
}
```

`Interfaces\Ports\IGetProfessionalByIdOutputPort.cs`:
```csharp
using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.BusinessObjects.Interfaces.Ports;
public interface IGetProfessionalByIdOutputPort : IBaseOutputPort
{
    OperationResult<ProfessionalDto?> Result { get; }
    Task Handle(ProfessionalDto? professional, CancellationToken ct = default);
}
```

- [ ] **Step 5: Build**

Run: `dotnet build "2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects\MedRec.Professionals.BusinessObjects.csproj"`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 6: Commit**

```bash
git add "2.ApplicationBusinessObjects\MedRec.Professionals.BusinessObjects"
git commit -m "feat(professionals): DTOs, validators, ports y IProfessionalRepositoryUoW (capa 2)"
```

---

## Task 5: `CreateProfessionalInteractor` (TDD)

**Files:**
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\Implementations\CreateProfessionalInteractor.cs`
- Create: `Test\MedRec.Professionals.UseCases.Tests\CreateProfessionalInteractorTests.cs`

**Interfaces:**
- Consumes: `ICreateProfessionalOutputPort`, `IProfessionalRepositoryUoW`, `IAuthorizationService`/`ICurrentUserContext` (de `MedRec.Identity.BusinessObjects`/`MedRec.Entity`), `IRepositoryUnitOfWork`, `IModelValidatorHub<CreateProfessionalDto>` (Task 4).
- Produces: `CreateProfessionalInteractor : ICreateProfessionalInputPort`.

- [ ] **Step 1: Escribir los tests (deben fallar por falta del interactor)**

`Test\MedRec.Professionals.UseCases.Tests\CreateProfessionalInteractorTests.cs`:
```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.UseCases.Implementations;
using MedRec.Shared.Exceptions;
using MedRec.Validator.Interfaces;
using MedRec.Validator.ValueObjects;
using Moq;

namespace MedRec.Professionals.UseCases.Tests;

public class CreateProfessionalInteractorTests
{
    private static (
        Mock<ICreateProfessionalOutputPort> presenter,
        Mock<IProfessionalRepositoryUoW> repo,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork,
        Mock<IModelValidatorHub<CreateProfessionalDto>> validator) CreateMocks()
    {
        return (
            new Mock<ICreateProfessionalOutputPort>(),
            new Mock<IProfessionalRepositoryUoW>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>(),
            new Mock<IModelValidatorHub<CreateProfessionalDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<CreateProfessionalDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<CreateProfessionalDto>(), It.IsAny<Func<CreateProfessionalDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCallerLacksPermission()
    {
        var dto = new CreateProfessionalDto("Ana", "García", "ana@medrec.local", null, DateTime.Today, ProfessionalType.Receptionist, null, null);
        var (presenter, repo, authorization, currentUser, unitOfWork, validator) = CreateMocks();

        authorization.Setup(a => a.EnsurePermissionAsync(It.IsAny<Guid?>(), "professionals.create", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessException(new ErrorInfo("No tiene permiso.", ErrorCode.Forbidden)));

        var interactor = new CreateProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await Assert.ThrowsAsync<BusinessException>(() => interactor.HandleAsync(dto, CancellationToken.None));

        repo.Verify(r => r.CreateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationErrors_WhenDoctorHasNoSpecialty()
    {
        var dto = new CreateProfessionalDto("Ana", "García", "ana@medrec.local", null, DateTime.Today, ProfessionalType.Doctor, "MP123", null);
        var (presenter, repo, authorization, currentUser, unitOfWork, validator) = CreateMocks();

        validator.Setup(v => v.Validate(dto, It.IsAny<Func<CreateProfessionalDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(false);
        validator.SetupGet(v => v.Errors).Returns(new[] { new ValidationError("SpecialtyId", "La especialidad es obligatoria para médicos.") });

        var interactor = new CreateProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Once);
        repo.Verify(r => r.CreateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenEmailAlreadyExists()
    {
        var dto = new CreateProfessionalDto("Ana", "García", "existente@medrec.local", null, DateTime.Today, ProfessionalType.Receptionist, null, null);
        var (presenter, repo, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        repo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Professional { Id = Guid.NewGuid(), Email = dto.Email });

        var interactor = new CreateProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        repo.Verify(r => r.CreateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateProfessional_WhenValid()
    {
        var dto = new CreateProfessionalDto("Ana", "García", "ana@medrec.local", "1140001111", DateTime.Today, ProfessionalType.Receptionist, null, null);
        var (presenter, repo, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);
        SetUpTransactionToRunWork(unitOfWork);

        repo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync((Professional?)null);

        var interactor = new CreateProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        repo.Verify(r => r.CreateAsync(
            It.Is<Professional>(p => p.Email == dto.Email && p.Type == ProfessionalType.Receptionist && p.IsDeleted == false),
            It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 2: Confirmar que el test falla por falta de implementación**

Run: `dotnet test "Test\MedRec.Professionals.UseCases.Tests\MedRec.Professionals.UseCases.Tests.csproj"`
Expected: FAIL en compilación (`CreateProfessionalInteractor` no existe todavía).

- [ ] **Step 3: Implementar el interactor**

`2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\Implementations\CreateProfessionalInteractor.cs`:
```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Professionals.UseCases.Implementations;

public class CreateProfessionalInteractor(
    ICreateProfessionalOutputPort presenter,
    IProfessionalRepositoryUoW professionalRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork,
    IModelValidatorHub<CreateProfessionalDto> validatorHub) : ICreateProfessionalInputPort
{
    public async Task HandleAsync(CreateProfessionalDto dto, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Professionals_Create, ct);

        var isValid = await validatorHub.Validate(dto, CreateProfessionalValidator.Validate);
        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var existing = await professionalRepository.GetByEmailAsync(dto.Email, ct);
        if (existing is not null)
        {
            await presenter.ErrorAsync(new ErrorInfo("Ya existe un profesional con ese email.", ErrorCode.DuplicateKey, null, 409));
            return;
        }

        var professional = new Professional
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone ?? string.Empty,
            HireDate = dto.HireDate,
            Type = dto.Type,
            LicenseNumber = dto.LicenseNumber,
            SpecialtyId = dto.SpecialtyId,
            IsDeleted = false
        };

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await professionalRepository.CreateAsync(professional, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await presenter.Handle(professional.Id, ct);
    }
}
```

- [ ] **Step 4: Confirmar que los tests pasan**

Run: `dotnet test "Test\MedRec.Professionals.UseCases.Tests\MedRec.Professionals.UseCases.Tests.csproj"`
Expected: PASS (4/4).

- [ ] **Step 5: Commit**

```bash
git add "2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\Implementations\CreateProfessionalInteractor.cs" "Test\MedRec.Professionals.UseCases.Tests\CreateProfessionalInteractorTests.cs"
git commit -m "feat(professionals): CreateProfessionalInteractor con tests"
```

---

## Task 6: `UpdateProfessionalInteractor`, `DeleteProfessionalInteractor`, `ListProfessionalsInteractor`, `GetProfessionalByIdInteractor` (TDD) + `DependencyContainer`

**Files:**
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\Implementations\UpdateProfessionalInteractor.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\Implementations\DeleteProfessionalInteractor.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\Implementations\ListProfessionalsInteractor.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\Implementations\GetProfessionalByIdInteractor.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\DependencyContainer.cs`
- Create: `Test\MedRec.Professionals.UseCases.Tests\UpdateProfessionalInteractorTests.cs`
- Create: `Test\MedRec.Professionals.UseCases.Tests\DeleteProfessionalInteractorTests.cs`
- Create: `Test\MedRec.Professionals.UseCases.Tests\ListProfessionalsInteractorTests.cs`
- Create: `Test\MedRec.Professionals.UseCases.Tests\GetProfessionalByIdInteractorTests.cs`

**Interfaces:**
- Produces: `AddProfessionalsUseCasesServicesWithProxy(this IServiceCollection, bool rethrow = false)`.

- [ ] **Step 1: Test de `UpdateProfessionalInteractor` (debe fallar por falta de implementación)**

`Test\MedRec.Professionals.UseCases.Tests\UpdateProfessionalInteractorTests.cs`:
```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.UseCases.Implementations;
using MedRec.Validator.Interfaces;
using MedRec.Validator.ValueObjects;
using Moq;

namespace MedRec.Professionals.UseCases.Tests;

public class UpdateProfessionalInteractorTests
{
    private static (
        Mock<IUpdateProfessionalOutputPort> presenter,
        Mock<IProfessionalRepositoryUoW> repo,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork,
        Mock<IModelValidatorHub<UpdateProfessionalDto>> validator) CreateMocks()
    {
        return (
            new Mock<IUpdateProfessionalOutputPort>(),
            new Mock<IProfessionalRepositoryUoW>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>(),
            new Mock<IModelValidatorHub<UpdateProfessionalDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<UpdateProfessionalDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<UpdateProfessionalDto>(), It.IsAny<Func<UpdateProfessionalDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenProfessionalNotFound()
    {
        var dto = new UpdateProfessionalDto(Guid.NewGuid(), "Ana", "García", "1140001111", ProfessionalType.Receptionist, null, null, Array.Empty<byte>());
        var (presenter, repo, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        repo.Setup(r => r.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Professional?)null);

        var interactor = new UpdateProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        repo.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateProfessional_WhenValid()
    {
        var existing = new Professional { Id = Guid.NewGuid(), FirstName = "Vieja", LastName = "García", Email = "ana@medrec.local", Type = ProfessionalType.Receptionist };
        var dto = new UpdateProfessionalDto(existing.Id, "Ana", "García", "1140001111", ProfessionalType.Receptionist, null, null, new byte[] { 1 });
        var (presenter, repo, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);
        SetUpTransactionToRunWork(unitOfWork);

        repo.Setup(r => r.GetByIdAsync(dto.Id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var interactor = new UpdateProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        repo.Verify(r => r.UpdateAsync(It.Is<Professional>(p => p.FirstName == "Ana"), It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 2: Test de `DeleteProfessionalInteractor`**

`Test\MedRec.Professionals.UseCases.Tests\DeleteProfessionalInteractorTests.cs`:
```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.UseCases.Implementations;
using Moq;

namespace MedRec.Professionals.UseCases.Tests;

public class DeleteProfessionalInteractorTests
{
    private static (
        Mock<IDeleteProfessionalOutputPort> presenter,
        Mock<IProfessionalRepositoryUoW> repo,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork) CreateMocks()
    {
        return (
            new Mock<IDeleteProfessionalOutputPort>(),
            new Mock<IProfessionalRepositoryUoW>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>());
    }

    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenProfessionalNotFound()
    {
        var id = Guid.NewGuid();
        var (presenter, repo, authorization, currentUser, unitOfWork) = CreateMocks();
        repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Professional?)null);

        var interactor = new DeleteProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object);

        await interactor.HandleAsync(id, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        repo.Verify(r => r.SoftDeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldSoftDelete_WhenProfessionalExists()
    {
        var id = Guid.NewGuid();
        var (presenter, repo, authorization, currentUser, unitOfWork) = CreateMocks();
        SetUpTransactionToRunWork(unitOfWork);
        repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(new Professional { Id = id });

        var interactor = new DeleteProfessionalInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object, unitOfWork.Object);

        await interactor.HandleAsync(id, CancellationToken.None);

        repo.Verify(r => r.SoftDeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 3: Test de `ListProfessionalsInteractor` y `GetProfessionalByIdInteractor`**

`Test\MedRec.Professionals.UseCases.Tests\ListProfessionalsInteractorTests.cs`:
```csharp
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.UseCases.Implementations;
using Moq;

namespace MedRec.Professionals.UseCases.Tests;

public class ListProfessionalsInteractorTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnListFromRepository()
    {
        var presenter = new Mock<IListProfessionalsOutputPort>();
        var repo = new Mock<IProfessionalRepositoryUoW>();
        var authorization = new Mock<IAuthorizationService>();
        var currentUser = new Mock<ICurrentUserContext>();

        var list = new List<ProfessionalDto>
        {
            new(Guid.NewGuid(), "Ana", "García", "ana@medrec.local", "", DateTime.Today, ProfessionalType.Receptionist, null, null, Array.Empty<byte>())
        };
        repo.Setup(r => r.ListAsync(null, It.IsAny<CancellationToken>())).ReturnsAsync(list);

        var interactor = new ListProfessionalsInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object);

        await interactor.HandleAsync(null, CancellationToken.None);

        presenter.Verify(p => p.Handle(list, It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

`Test\MedRec.Professionals.UseCases.Tests\GetProfessionalByIdInteractorTests.cs`:
```csharp
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.UseCases.Implementations;
using Moq;

namespace MedRec.Professionals.UseCases.Tests;

public class GetProfessionalByIdInteractorTests
{
    [Fact]
    public async Task HandleAsync_ShouldReturnNullDto_WhenNotFound()
    {
        var id = Guid.NewGuid();
        var presenter = new Mock<IGetProfessionalByIdOutputPort>();
        var repo = new Mock<IProfessionalRepositoryUoW>();
        var authorization = new Mock<IAuthorizationService>();
        var currentUser = new Mock<ICurrentUserContext>();
        repo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Professional?)null);

        var interactor = new GetProfessionalByIdInteractor(presenter.Object, repo.Object, authorization.Object, currentUser.Object);

        await interactor.HandleAsync(id, CancellationToken.None);

        presenter.Verify(p => p.Handle((ProfessionalDto?)null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 4: Confirmar que fallan por falta de implementación**

Run: `dotnet test "Test\MedRec.Professionals.UseCases.Tests\MedRec.Professionals.UseCases.Tests.csproj"`
Expected: FAIL en compilación.

- [ ] **Step 5: Implementar los 4 interactores**

`Implementations\UpdateProfessionalInteractor.cs`:
```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Professionals.UseCases.Implementations;

public class UpdateProfessionalInteractor(
    IUpdateProfessionalOutputPort presenter,
    IProfessionalRepositoryUoW professionalRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork,
    IModelValidatorHub<UpdateProfessionalDto> validatorHub) : IUpdateProfessionalInputPort
{
    public async Task HandleAsync(UpdateProfessionalDto dto, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Professionals_Edit, ct);

        var isValid = await validatorHub.Validate(dto, UpdateProfessionalValidator.Validate);
        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var professional = await professionalRepository.GetByIdAsync(dto.Id, ct);
        if (professional is null)
        {
            await presenter.ErrorAsync(new ErrorInfo("Profesional no encontrado.", ErrorCode.NotFound, null, 404));
            return;
        }

        professional.FirstName = dto.FirstName;
        professional.LastName = dto.LastName;
        professional.Phone = dto.Phone;
        professional.Type = dto.Type;
        professional.LicenseNumber = dto.LicenseNumber;
        professional.SpecialtyId = dto.SpecialtyId;
        professional.RowVersion = dto.RowVersion;

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await professionalRepository.UpdateAsync(professional, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await presenter.Handle(ct);
    }
}
```

`Implementations\DeleteProfessionalInteractor.cs`:
```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;

namespace MedRec.Professionals.UseCases.Implementations;

public class DeleteProfessionalInteractor(
    IDeleteProfessionalOutputPort presenter,
    IProfessionalRepositoryUoW professionalRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork) : IDeleteProfessionalInputPort
{
    public async Task HandleAsync(Guid id, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Professionals_Delete, ct);

        var professional = await professionalRepository.GetByIdAsync(id, ct);
        if (professional is null)
        {
            await presenter.ErrorAsync(new ErrorInfo("Profesional no encontrado.", ErrorCode.NotFound, null, 404));
            return;
        }

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await professionalRepository.SoftDeleteAsync(id, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await presenter.Handle(ct);
    }
}
```

`Implementations\ListProfessionalsInteractor.cs`:
```csharp
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;

namespace MedRec.Professionals.UseCases.Implementations;

public class ListProfessionalsInteractor(
    IListProfessionalsOutputPort presenter,
    IProfessionalRepositoryUoW professionalRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext) : IListProfessionalsInputPort
{
    public async Task HandleAsync(ProfessionalType? typeFilter, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Professionals_View, ct);

        var professionals = await professionalRepository.ListAsync(typeFilter, ct);
        await presenter.Handle(professionals, ct);
    }
}
```

`Implementations\GetProfessionalByIdInteractor.cs`:
```csharp
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;

namespace MedRec.Professionals.UseCases.Implementations;

public class GetProfessionalByIdInteractor(
    IGetProfessionalByIdOutputPort presenter,
    IProfessionalRepositoryUoW professionalRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext) : IGetProfessionalByIdInputPort
{
    public async Task HandleAsync(Guid id, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Professionals_View, ct);

        var professional = await professionalRepository.GetByIdAsync(id, ct);
        var dto = professional is null
            ? null
            : new ProfessionalDto(
                professional.Id, professional.FirstName, professional.LastName, professional.Email,
                professional.Phone, professional.HireDate, professional.Type, professional.LicenseNumber,
                professional.SpecialtyId, professional.RowVersion);

        await presenter.Handle(dto, ct);
    }
}
```

- [ ] **Step 6: `DependencyContainer.cs` con el proxy de excepciones**

`2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\DependencyContainer.cs`:
```csharp
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.UseCases.Implementations;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddProfessionalsUseCasesServicesWithProxy(
        this IServiceCollection services,
        bool rethrow = false)
    {
        return services.AddUseCaseExceptionDecorators(
            [
                typeof(ICreateProfessionalInputPort).Assembly,
                typeof(CreateProfessionalInteractor).Assembly
            ], rethrow);
    }
}
```

Actualizar el `.csproj` de `MedRec.Professionals.UseCases` para referenciar `MedRec.BusinessObjects` (de donde viene `AddUseCaseExceptionDecorators`) y `MedRec.Identity.BusinessObjects` (de donde vienen `IAuthorizationService`/`SystemPermissions`) agregando dentro del `<ItemGroup>` de `ProjectReference`:
```xml
    <ProjectReference Include="..\MedRec.BusinessObjects\MedRec.BusinessObjects.csproj" />
    <ProjectReference Include="..\MedRec.Identity.BusinessObjects\MedRec.Identity.BusinessObjects.csproj" />
```

- [ ] **Step 7: Confirmar que todos los tests pasan**

Run: `dotnet test "Test\MedRec.Professionals.UseCases.Tests\MedRec.Professionals.UseCases.Tests.csproj"`
Expected: PASS (9/9 entre este task y el anterior).

- [ ] **Step 8: Commit**

```bash
git add "2.ApplicationBusinessObjects\MedRec.Professionals.UseCases" "Test\MedRec.Professionals.UseCases.Tests"
git commit -m "feat(professionals): interactores de Update/Delete/List/GetById con tests y DependencyContainer"
```

---

## Task 7: Capa 3 (Repositories) — `ProfessionalRepository`, `SpecialtyLookupRepository`

**Files:**
- Create: `3.InterfaceAdapters\Repositories\MedRec.Professionals.Repositories\Interfaces\IProfessionalDataContext.cs`
- Create: `3.InterfaceAdapters\Repositories\MedRec.Professionals.Repositories\Interfaces\ISpecialtyLookupDataContext.cs`
- Create: `3.InterfaceAdapters\Repositories\MedRec.Professionals.Repositories\Implementations\ProfessionalRepository.cs`
- Create: `3.InterfaceAdapters\Repositories\MedRec.Professionals.Repositories\Implementations\SpecialtyLookupRepository.cs`
- Create: `3.InterfaceAdapters\Repositories\MedRec.Professionals.Repositories\DependencyContainer.cs`

**Interfaces:**
- Consumes: `IProfessionalRepositoryUoW`, `ISpecialtyLookupRepository` (Task 4).
- Produces: `IProfessionalDataContext`, `ISpecialtyLookupDataContext` (implementadas en capa 4, Task 8), `AddProfessionalsRepositoriesServices(this IServiceCollection)`.

- [ ] **Step 1: Interfaces de DataContext (capa 3, agnósticas de EF)**

`Interfaces\IProfessionalDataContext.cs`:
```csharp
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.Repositories.Interfaces;
public interface IProfessionalDataContext
{
    Task<Professional?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Professional?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<ProfessionalDto>> ListAsync(ProfessionalType? typeFilter, CancellationToken ct = default);
    Task CreateAsync(Professional professional, CancellationToken ct = default);
    Task UpdateAsync(Professional professional, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
```

`Interfaces\ISpecialtyLookupDataContext.cs`:
```csharp
using MedRec.Professionals.BusinessObjects.DTOs;

namespace MedRec.Professionals.Repositories.Interfaces;
public interface ISpecialtyLookupDataContext
{
    Task<IReadOnlyList<SpecialtySummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
```

- [ ] **Step 2: Implementaciones pass-through**

`Implementations\ProfessionalRepository.cs`:
```csharp
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.Repositories.Interfaces;

namespace MedRec.Professionals.Repositories.Implementations;
internal class ProfessionalRepository(IProfessionalDataContext dataContext) : IProfessionalRepositoryUoW
{
    public Task<Professional?> GetByIdAsync(Guid id, CancellationToken ct = default) => dataContext.GetByIdAsync(id, ct);
    public Task<Professional?> GetByEmailAsync(string email, CancellationToken ct = default) => dataContext.GetByEmailAsync(email, ct);
    public Task<IReadOnlyList<ProfessionalDto>> ListAsync(ProfessionalType? typeFilter, CancellationToken ct = default) => dataContext.ListAsync(typeFilter, ct);
    public Task CreateAsync(Professional professional, CancellationToken ct = default) => dataContext.CreateAsync(professional, ct);
    public Task UpdateAsync(Professional professional, CancellationToken ct = default) => dataContext.UpdateAsync(professional, ct);
    public Task SoftDeleteAsync(Guid id, CancellationToken ct = default) => dataContext.SoftDeleteAsync(id, ct);
}
```

`Implementations\SpecialtyLookupRepository.cs`:
```csharp
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.Repositories.Interfaces;

namespace MedRec.Professionals.Repositories.Implementations;
internal class SpecialtyLookupRepository(ISpecialtyLookupDataContext dataContext) : ISpecialtyLookupRepository
{
    public Task<IReadOnlyList<SpecialtySummaryDto>> ListActiveAsync(CancellationToken ct = default) =>
        dataContext.ListActiveAsync(ct);
}
```

- [ ] **Step 3: `DependencyContainer.cs`**

```csharp
using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.Repositories.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddProfessionalsRepositoriesServices(this IServiceCollection services)
    {
        services.AddScoped<IProfessionalRepositoryUoW, ProfessionalRepository>();
        services.AddScoped<ISpecialtyLookupRepository, SpecialtyLookupRepository>();
        return services;
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build "3.InterfaceAdapters\Repositories\MedRec.Professionals.Repositories\MedRec.Professionals.Repositories.csproj"`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 5: Commit**

```bash
git add "3.InterfaceAdapters\Repositories\MedRec.Professionals.Repositories"
git commit -m "feat(professionals): ProfessionalRepository y SpecialtyLookupRepository (capa 3)"
```

---

## Task 8: Capa 4 (DataContext.MySql) — `ProfessionalDataContextMySql`, `SpecialtyLookupDataContextMySql`

**Files:**
- Create: `4.Framework&Drivers\MedRec.Professionals.DataContext.MySql\Services\ProfessionalDataContextMySql.cs`
- Create: `4.Framework&Drivers\MedRec.Professionals.DataContext.MySql\Services\SpecialtyLookupDataContextMySql.cs`
- Create: `4.Framework&Drivers\MedRec.Professionals.DataContext.MySql\DependencyContainer.cs`

**Interfaces:**
- Consumes: `IProfessionalDataContext`, `ISpecialtyLookupDataContext` (Task 7), `MedRecContext` (`MedRec.DataContext.MySql`).
- Produces: `AddProfessionalsDataContextMySqlServices(this IServiceCollection)`.

- [ ] **Step 1: Implementaciones EF**

`Services\ProfessionalDataContextMySql.cs`:
```csharp
using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Professionals.DataContext.MySql.Services;

internal class ProfessionalDataContextMySql(MedRecContext context) : IProfessionalDataContext
{
    public async Task<Professional?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await context.Professionals.FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, ct);

    public async Task<Professional?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        await context.Professionals.FirstOrDefaultAsync(p => p.Email == email && !p.IsDeleted, ct);

    public async Task<IReadOnlyList<ProfessionalDto>> ListAsync(ProfessionalType? typeFilter, CancellationToken ct = default)
    {
        var query = context.Professionals.Where(p => !p.IsDeleted);
        if (typeFilter.HasValue)
            query = query.Where(p => p.Type == typeFilter.Value);

        return await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Select(p => new ProfessionalDto(p.Id, p.FirstName, p.LastName, p.Email, p.Phone, p.HireDate, p.Type, p.LicenseNumber, p.SpecialtyId, p.RowVersion))
            .ToListAsync(ct);
    }

    public Task CreateAsync(Professional professional, CancellationToken ct = default)
    {
        context.Professionals.Add(professional);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Professional professional, CancellationToken ct = default)
    {
        context.Professionals.Update(professional);
        return Task.CompletedTask;
    }

    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var professional = await context.Professionals.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (professional is not null)
            professional.IsDeleted = true;
    }
}
```

`Services\SpecialtyLookupDataContextMySql.cs`:
```csharp
using MedRec.DataContext.MySql.DataContext;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Professionals.DataContext.MySql.Services;

internal class SpecialtyLookupDataContextMySql(MedRecContext context) : ISpecialtyLookupDataContext
{
    public async Task<IReadOnlyList<SpecialtySummaryDto>> ListActiveAsync(CancellationToken ct = default) =>
        await context.MedicalSpecialties
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.Name)
            .Select(s => new SpecialtySummaryDto(s.Id, s.Name))
            .ToListAsync(ct);
}
```

- [ ] **Step 2: `DependencyContainer.cs`**

```csharp
using MedRec.Professionals.DataContext.MySql.Services;
using MedRec.Professionals.Repositories.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddProfessionalsDataContextMySqlServices(this IServiceCollection services)
    {
        services.AddScoped<IProfessionalDataContext, ProfessionalDataContextMySql>();
        services.AddScoped<ISpecialtyLookupDataContext, SpecialtyLookupDataContextMySql>();
        return services;
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build "4.Framework&Drivers\MedRec.Professionals.DataContext.MySql\MedRec.Professionals.DataContext.MySql.csproj"`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 4: Commit**

```bash
git add "4.Framework&Drivers\MedRec.Professionals.DataContext.MySql"
git commit -m "feat(professionals): implementaciones EF de ProfessionalDataContext y SpecialtyLookupDataContext (capa 4)"
```

---

## Task 9: Capa 3 (Presenters)

**Files:**
- Create: `3.InterfaceAdapters\MedRec.Professionals.Presenters\Implementations\CreateProfessionalPresenter.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.Presenters\Implementations\UpdateProfessionalPresenter.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.Presenters\Implementations\DeleteProfessionalPresenter.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.Presenters\Implementations\ListProfessionalsPresenter.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.Presenters\Implementations\GetProfessionalByIdPresenter.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.Presenters\DependencyContainer.cs`

**Interfaces:**
- Produces: `AddProfessionalsPresentersServices(this IServiceCollection)`.

- [ ] **Step 1: Los 5 presenters**

`Implementations\CreateProfessionalPresenter.cs`:
```csharp
using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;

namespace MedRec.Professionals.Presenters.Implementations;
internal class CreateProfessionalPresenter : BaseOutputPort<Guid>, ICreateProfessionalOutputPort
{
    public Task Handle(Guid professionalId, CancellationToken ct = default)
    {
        Result = OperationResult<Guid>.Ok(professionalId);
        return Task.CompletedTask;
    }
}
```

`Implementations\UpdateProfessionalPresenter.cs`:
```csharp
using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;

namespace MedRec.Professionals.Presenters.Implementations;
internal class UpdateProfessionalPresenter : BaseOutputPort<bool>, IUpdateProfessionalOutputPort
{
    public Task Handle(CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }
}
```

`Implementations\DeleteProfessionalPresenter.cs`:
```csharp
using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;

namespace MedRec.Professionals.Presenters.Implementations;
internal class DeleteProfessionalPresenter : BaseOutputPort<bool>, IDeleteProfessionalOutputPort
{
    public Task Handle(CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }
}
```

`Implementations\ListProfessionalsPresenter.cs`:
```csharp
using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;

namespace MedRec.Professionals.Presenters.Implementations;
internal class ListProfessionalsPresenter : BaseOutputPort<IReadOnlyList<ProfessionalDto>>, IListProfessionalsOutputPort
{
    public Task Handle(IReadOnlyList<ProfessionalDto> professionals, CancellationToken ct = default)
    {
        Result = OperationResult<IReadOnlyList<ProfessionalDto>>.Ok(professionals);
        return Task.CompletedTask;
    }
}
```

`Implementations\GetProfessionalByIdPresenter.cs`:
```csharp
using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;

namespace MedRec.Professionals.Presenters.Implementations;
internal class GetProfessionalByIdPresenter : BaseOutputPort<ProfessionalDto?>, IGetProfessionalByIdOutputPort
{
    public Task Handle(ProfessionalDto? professional, CancellationToken ct = default)
    {
        Result = OperationResult<ProfessionalDto?>.Ok(professional);
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 2: `DependencyContainer.cs`**

```csharp
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.Presenters.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddProfessionalsPresentersServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateProfessionalOutputPort, CreateProfessionalPresenter>();
        services.AddScoped<IUpdateProfessionalOutputPort, UpdateProfessionalPresenter>();
        services.AddScoped<IDeleteProfessionalOutputPort, DeleteProfessionalPresenter>();
        services.AddScoped<IListProfessionalsOutputPort, ListProfessionalsPresenter>();
        services.AddScoped<IGetProfessionalByIdOutputPort, GetProfessionalByIdPresenter>();
        return services;
    }
}
```

- [ ] **Step 3: Build**

Run: `dotnet build "3.InterfaceAdapters\MedRec.Professionals.Presenters\MedRec.Professionals.Presenters.csproj"`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 4: Commit**

```bash
git add "3.InterfaceAdapters\MedRec.Professionals.Presenters"
git commit -m "feat(professionals): presenters de los 5 casos de uso (capa 3)"
```

---

## Task 10: Capa 3 (ViewModels) — Models, Mapper, Actions y `CreateProfessionalOrchestrator` (TDD para la compensación)

**Files:**
- Create: `3.InterfaceAdapters\MedRec.Professionals.ViewModels\Models\CreateProfessionalModel.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.ViewModels\Models\CreateUserForProfessionalModel.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.ViewModels\Models\ProfessionalModel.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.ViewModels\Models\UpdateProfessionalModel.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.ViewModels\Orchestration\ProfessionalMapper.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.ViewModels\Orchestration\Actions\Interfaces\ICreateProfessionalAction.cs`, `IDeleteProfessionalAction.cs`, `ICreateUserForProfessionalAction.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.ViewModels\Orchestration\Actions\CreateProfessionalAction.cs`, `DeleteProfessionalAction.cs`, `CreateUserForProfessionalAction.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.ViewModels\Orchestration\Interfaces\ICreateProfessionalOrchestrator.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.ViewModels\Orchestration\CreateProfessionalOrchestrator.cs`
- Create: `Test\MedRec.Professionals.UseCases.Tests\CreateProfessionalOrchestratorTests.cs`

**Interfaces:**
- Consumes: `ICreateProfessionalInputPort/OutputPort`, `IUpdateProfessionalInputPort/OutputPort`, `IDeleteProfessionalInputPort/OutputPort` (Task 4); `ICreateUserInputPort/OutputPort` (`MedRec.Identity.BusinessObjects.Interfaces.Ports`, ya existente); `CreateUserDto` (ya existente, constructor `(string email, string fullName, string temporaryPassword, IReadOnlyList<Guid> roleIds, Guid? professionalId)`).
- Produces: `ICreateProfessionalOrchestrator.CreateProfessional(CreateProfessionalModel, CancellationToken) : Task<OperationResult<Guid>>`.

- [ ] **Step 1: Models**

`Models\CreateUserForProfessionalModel.cs`:
```csharp
namespace MedRec.Professionals.ViewModels.Models;
public class CreateUserForProfessionalModel
{
    public string TemporaryPassword { get; set; } = string.Empty;
    public List<Guid> RoleIds { get; set; } = new();
}
```

`Models\CreateProfessionalModel.cs`:
```csharp
using MedRec.Entity.Enums;

namespace MedRec.Professionals.ViewModels.Models;
public class CreateProfessionalModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Today;
    public ProfessionalType Type { get; set; } = ProfessionalType.Doctor;
    public string? LicenseNumber { get; set; }
    public Guid? SpecialtyId { get; set; }
    public CreateUserForProfessionalModel? CreateUser { get; set; }
}
```

`Models\ProfessionalModel.cs`:
```csharp
using MedRec.Entity.Enums;

namespace MedRec.Professionals.ViewModels.Models;
public class ProfessionalModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{LastName}, {FirstName}";
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public ProfessionalType Type { get; set; }
    public string? LicenseNumber { get; set; }
    public Guid? SpecialtyId { get; set; }
}
```

`Models\UpdateProfessionalModel.cs`:
```csharp
using MedRec.Entity.Enums;

namespace MedRec.Professionals.ViewModels.Models;
public class UpdateProfessionalModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public ProfessionalType Type { get; set; } = ProfessionalType.Doctor;
    public string? LicenseNumber { get; set; }
    public Guid? SpecialtyId { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
```

- [ ] **Step 2: `ProfessionalMapper`**

`Orchestration\ProfessionalMapper.cs`:
```csharp
using MedRec.Professionals.BusinessObjects.DTOs;
using MedRec.Professionals.ViewModels.Models;

namespace MedRec.Professionals.ViewModels.Orchestration;
public static class ProfessionalMapper
{
    public static CreateProfessionalDto ToCreateDto(CreateProfessionalModel model) =>
        new(model.FirstName, model.LastName, model.Email, model.Phone, model.HireDate, model.Type, model.LicenseNumber, model.SpecialtyId);

    public static UpdateProfessionalDto ToUpdateDto(UpdateProfessionalModel model) =>
        new(model.Id, model.FirstName, model.LastName, model.Phone, model.Type, model.LicenseNumber, model.SpecialtyId, model.RowVersion);

    public static ProfessionalModel ToModel(ProfessionalDto dto) => new()
    {
        Id = dto.Id,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        Email = dto.Email,
        Phone = dto.Phone,
        HireDate = dto.HireDate,
        Type = dto.Type,
        LicenseNumber = dto.LicenseNumber,
        SpecialtyId = dto.SpecialtyId
    };
}
```

- [ ] **Step 3: Actions**

`Orchestration\Actions\Interfaces\ICreateProfessionalAction.cs`:
```csharp
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.ViewModels.Models;

namespace MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;
public interface ICreateProfessionalAction
{
    Task<OperationResult<Guid>> ExecuteAsync(CreateProfessionalModel model, CancellationToken ct = default);
}
```

`Orchestration\Actions\Interfaces\IDeleteProfessionalAction.cs`:
```csharp
using MedRec.BusinessObjects.Results;

namespace MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;
public interface IDeleteProfessionalAction
{
    Task<OperationResult<bool>> ExecuteAsync(Guid professionalId, CancellationToken ct = default);
}
```

`Orchestration\Actions\Interfaces\ICreateUserForProfessionalAction.cs`:
```csharp
using MedRec.BusinessObjects.Results;

namespace MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;
public interface ICreateUserForProfessionalAction
{
    Task<OperationResult<bool>> ExecuteAsync(
        Guid professionalId,
        string email,
        string fullName,
        string temporaryPassword,
        IReadOnlyList<Guid> roleIds,
        CancellationToken ct = default);
}
```

`Orchestration\Actions\CreateProfessionalAction.cs`:
```csharp
using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.ViewModels.Models;
using MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.Professionals.ViewModels.Orchestration.Actions;

public class CreateProfessionalAction(
    ICreateProfessionalInputPort inPort,
    ICreateProfessionalOutputPort outPort) : ICreateProfessionalAction
{
    public async Task<OperationResult<Guid>> ExecuteAsync(CreateProfessionalModel model, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var dto = ProfessionalMapper.ToCreateDto(model);
            await inPort.HandleAsync(dto, ct);
            return outPort.Result;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<Guid>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<Guid>(new ErrorInfo($"Error crítico al crear el profesional: {ex.Message}"), null);
        }
    }
}
```

`Orchestration\Actions\DeleteProfessionalAction.cs`:
```csharp
using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.Professionals.ViewModels.Orchestration.Actions;

public class DeleteProfessionalAction(
    IDeleteProfessionalInputPort inPort,
    IDeleteProfessionalOutputPort outPort) : IDeleteProfessionalAction
{
    public async Task<OperationResult<bool>> ExecuteAsync(Guid professionalId, CancellationToken ct = default)
    {
        try
        {
            await inPort.HandleAsync(professionalId, ct);
            return outPort.Result;
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<bool>(new ErrorInfo($"Error crítico al revertir la creación del profesional: {ex.Message}"), null);
        }
    }
}
```

`Orchestration\Actions\CreateUserForProfessionalAction.cs`:
```csharp
using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;

namespace MedRec.Professionals.ViewModels.Orchestration.Actions;

public class CreateUserForProfessionalAction(
    ICreateUserInputPort inPort,
    ICreateUserOutputPort outPort) : ICreateUserForProfessionalAction
{
    public async Task<OperationResult<bool>> ExecuteAsync(
        Guid professionalId,
        string email,
        string fullName,
        string temporaryPassword,
        IReadOnlyList<Guid> roleIds,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        try
        {
            var dto = new CreateUserDto(email, fullName, temporaryPassword, roleIds, professionalId);
            await inPort.HandleAsync(dto, ct);
            return outPort.Result;
        }
        catch (OperationCanceledException)
        {
            return OperationResult.Cancelled<bool>();
        }
        catch (Exception ex)
        {
            return OperationResult.Fail<bool>(new ErrorInfo($"Error crítico al crear el usuario: {ex.Message}"), null);
        }
    }
}
```

- [ ] **Step 4: Escribir los tests del Orchestrator (deben fallar por falta de implementación)**

`Test\MedRec.Professionals.UseCases.Tests\CreateProfessionalOrchestratorTests.cs`:
```csharp
using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Professionals.ViewModels.Models;
using MedRec.Professionals.ViewModels.Orchestration;
using MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;
using Moq;

namespace MedRec.Professionals.UseCases.Tests;

public class CreateProfessionalOrchestratorTests
{
    [Fact]
    public async Task CreateProfessional_ShouldReturnSuccess_WhenNoUserRequested()
    {
        var professionalId = Guid.NewGuid();
        var createProfessional = new Mock<ICreateProfessionalAction>();
        var createUser = new Mock<ICreateUserForProfessionalAction>();
        var deleteProfessional = new Mock<IDeleteProfessionalAction>();

        createProfessional.Setup(a => a.ExecuteAsync(It.IsAny<CreateProfessionalModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok(professionalId));

        var orchestrator = new CreateProfessionalOrchestrator(createProfessional.Object, createUser.Object, deleteProfessional.Object);
        var model = new CreateProfessionalModel { CreateUser = null };

        var result = await orchestrator.CreateProfessional(model, CancellationToken.None);

        Assert.True(result.Success);
        Assert.Equal(professionalId, result.Value);
        createUser.Verify(a => a.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateProfessional_ShouldCreateUser_WhenRequested()
    {
        var professionalId = Guid.NewGuid();
        var createProfessional = new Mock<ICreateProfessionalAction>();
        var createUser = new Mock<ICreateUserForProfessionalAction>();
        var deleteProfessional = new Mock<IDeleteProfessionalAction>();

        createProfessional.Setup(a => a.ExecuteAsync(It.IsAny<CreateProfessionalModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok(professionalId));
        createUser.Setup(a => a.ExecuteAsync(professionalId, "ana@medrec.local", "Ana García", "Temporal123!", It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok(true));

        var orchestrator = new CreateProfessionalOrchestrator(createProfessional.Object, createUser.Object, deleteProfessional.Object);
        var model = new CreateProfessionalModel
        {
            FirstName = "Ana",
            LastName = "García",
            Email = "ana@medrec.local",
            CreateUser = new CreateUserForProfessionalModel { TemporaryPassword = "Temporal123!", RoleIds = new List<Guid>() }
        };

        var result = await orchestrator.CreateProfessional(model, CancellationToken.None);

        Assert.True(result.Success);
        deleteProfessional.Verify(a => a.ExecuteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateProfessional_ShouldCompensate_WhenUserCreationFails()
    {
        var professionalId = Guid.NewGuid();
        var createProfessional = new Mock<ICreateProfessionalAction>();
        var createUser = new Mock<ICreateUserForProfessionalAction>();
        var deleteProfessional = new Mock<IDeleteProfessionalAction>();

        createProfessional.Setup(a => a.ExecuteAsync(It.IsAny<CreateProfessionalModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok(professionalId));
        createUser.Setup(a => a.ExecuteAsync(professionalId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Fail<bool>(new ErrorInfo("Ya existe un usuario con ese email."), null));
        deleteProfessional.Setup(a => a.ExecuteAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResult.Ok(true));

        var orchestrator = new CreateProfessionalOrchestrator(createProfessional.Object, createUser.Object, deleteProfessional.Object);
        var model = new CreateProfessionalModel
        {
            CreateUser = new CreateUserForProfessionalModel { TemporaryPassword = "Temporal123!", RoleIds = new List<Guid>() }
        };

        var result = await orchestrator.CreateProfessional(model, CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal("Ya existe un usuario con ese email.", result.Error?.Message);
        deleteProfessional.Verify(a => a.ExecuteAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 5: Confirmar que fallan por falta de implementación**

Run: `dotnet test "Test\MedRec.Professionals.UseCases.Tests\MedRec.Professionals.UseCases.Tests.csproj"`
Expected: FAIL en compilación (`ICreateProfessionalOrchestrator`/`CreateProfessionalOrchestrator` no existen todavía).

- [ ] **Step 6: Implementar el Orchestrator**

`Orchestration\Interfaces\ICreateProfessionalOrchestrator.cs`:
```csharp
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.ViewModels.Models;

namespace MedRec.Professionals.ViewModels.Orchestration.Interfaces;
public interface ICreateProfessionalOrchestrator
{
    Task<OperationResult<Guid>> CreateProfessional(CreateProfessionalModel model, CancellationToken ct = default);
}
```

`Orchestration\CreateProfessionalOrchestrator.cs`:
```csharp
using MedRec.BusinessObjects.Results;
using MedRec.Professionals.ViewModels.Models;
using MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;
using MedRec.Professionals.ViewModels.Orchestration.Interfaces;

namespace MedRec.Professionals.ViewModels.Orchestration;

public class CreateProfessionalOrchestrator(
    ICreateProfessionalAction createProfessional,
    ICreateUserForProfessionalAction createUser,
    IDeleteProfessionalAction deleteProfessional) : ICreateProfessionalOrchestrator
{
    public async Task<OperationResult<Guid>> CreateProfessional(CreateProfessionalModel model, CancellationToken ct = default)
    {
        var professionalResult = await createProfessional.ExecuteAsync(model, ct);
        if (!professionalResult.Success)
            return professionalResult;

        if (model.CreateUser is null)
            return professionalResult;

        var userResult = await createUser.ExecuteAsync(
            professionalResult.Value,
            model.Email,
            $"{model.FirstName} {model.LastName}",
            model.CreateUser.TemporaryPassword,
            model.CreateUser.RoleIds,
            ct);

        if (userResult.Success)
            return professionalResult;

        // Compensación best-effort: si el borrado también falla (p.ej. el usuario actual
        // tiene Professionals_Create pero no Professionals_Delete), igual se propaga el
        // error real de la creación del usuario en vez de uno de permisos que lo taparía.
        await deleteProfessional.ExecuteAsync(professionalResult.Value, ct);

        return OperationResult.Fail<Guid>(
            userResult.Error ?? new MedRec.Entity.DTOs.ErrorInfo("No se pudo crear el usuario del profesional."),
            userResult.MessageAction,
            userResult.ValidationErrors);
    }
}
```

- [ ] **Step 7: Confirmar que los tests pasan**

Run: `dotnet test "Test\MedRec.Professionals.UseCases.Tests\MedRec.Professionals.UseCases.Tests.csproj"`
Expected: PASS (12/12 acumulados).

- [ ] **Step 8: Commit**

```bash
git add "3.InterfaceAdapters\MedRec.Professionals.ViewModels" "Test\MedRec.Professionals.UseCases.Tests\CreateProfessionalOrchestratorTests.cs"
git commit -m "feat(professionals): Models, Mapper, Actions y CreateProfessionalOrchestrator con compensación"
```

---

## Task 11: Capa 3 (ViewModels) — `CreateProfessionalVM`, `ProfessionalsListVM`, `UpdateProfessionalVM`

**Files:**
- Create: `3.InterfaceAdapters\MedRec.Professionals.ViewModels\VM\CreateProfessionalVM.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.ViewModels\VM\ProfessionalsListVM.cs`
- Create: `3.InterfaceAdapters\MedRec.Professionals.ViewModels\VM\UpdateProfessionalVM.cs`

**Interfaces:**
- Consumes: `ICreateProfessionalOrchestrator` (Task 10); `IListProfessionalsInputPort/OutputPort`, `IUpdateProfessionalInputPort/OutputPort`, `IGetProfessionalByIdInputPort/OutputPort`, `IDeleteProfessionalInputPort/OutputPort` (Task 4).
- Produces: `CreateProfessionalVM.Model/IsProcessing/InformationMessage/Success/CreateAsync()`; `ProfessionalsListVM.Professionals/LoadAsync()/DeleteAsync()`; `UpdateProfessionalVM.Model/LoadAsync()/UpdateAsync()`.

- [ ] **Step 1: `CreateProfessionalVM`**

`VM\CreateProfessionalVM.cs`:
```csharp
using MedRec.Professionals.ViewModels.Models;
using MedRec.Professionals.ViewModels.Orchestration.Interfaces;

namespace MedRec.Professionals.ViewModels.VM;
public class CreateProfessionalVM(ICreateProfessionalOrchestrator orchestrator)
{
    public CreateProfessionalModel Model { get; set; } = new();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;
    public bool Success { get; private set; }

    public async Task CreateAsync(CancellationToken ct = default)
    {
        IsProcessing = true;
        Success = false;
        try
        {
            InformationMessage = string.Empty;
            var result = await orchestrator.CreateProfessional(Model, ct);

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join(" ", result.ValidationErrors.Select(e => e.ErrorMessage));
            }
            else if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "No se pudo crear el profesional.";
            }
            else
            {
                InformationMessage = "Profesional creado correctamente.";
                Success = true;
                Model = new CreateProfessionalModel();
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
```

- [ ] **Step 2: `ProfessionalsListVM`**

`VM\ProfessionalsListVM.cs`:
```csharp
using MedRec.Entity.Enums;
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.ViewModels.Models;
using MedRec.Professionals.ViewModels.Orchestration;

namespace MedRec.Professionals.ViewModels.VM;
public class ProfessionalsListVM(
    IListProfessionalsInputPort listInteractor,
    IListProfessionalsOutputPort listPresenter,
    IDeleteProfessionalInputPort deleteInteractor,
    IDeleteProfessionalOutputPort deletePresenter)
{
    public IReadOnlyList<ProfessionalModel> Professionals { get; private set; } = Array.Empty<ProfessionalModel>();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;

    public async Task LoadAsync(ProfessionalType? typeFilter = null, CancellationToken ct = default)
    {
        IsProcessing = true;
        try
        {
            await listInteractor.HandleAsync(typeFilter, ct);
            var result = listPresenter.Result;
            Professionals = result.Success
                ? (result.Value ?? Array.Empty<MedRec.Professionals.BusinessObjects.DTOs.ProfessionalDto>()).Select(ProfessionalMapper.ToModel).ToArray()
                : Array.Empty<ProfessionalModel>();
            if (!result.Success)
                InformationMessage = result.Error?.Message ?? "No se pudo cargar el listado de profesionales.";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        IsProcessing = true;
        try
        {
            await deleteInteractor.HandleAsync(id, ct);
            var result = deletePresenter.Result;
            if (!result.Success)
                InformationMessage = result.Error?.Message ?? "No se pudo eliminar el profesional.";
            else
                await LoadAsync(ct: ct);
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
```

- [ ] **Step 3: `UpdateProfessionalVM`**

`VM\UpdateProfessionalVM.cs`:
```csharp
using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.ViewModels.Models;
using MedRec.Professionals.ViewModels.Orchestration;

namespace MedRec.Professionals.ViewModels.VM;
public class UpdateProfessionalVM(
    IUpdateProfessionalInputPort updateInteractor,
    IUpdateProfessionalOutputPort updatePresenter,
    IGetProfessionalByIdInputPort getInteractor,
    IGetProfessionalByIdOutputPort getPresenter)
{
    public UpdateProfessionalModel Model { get; set; } = new();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;
    public bool Success { get; private set; }

    public async Task LoadAsync(Guid id, CancellationToken ct = default)
    {
        IsProcessing = true;
        try
        {
            await getInteractor.HandleAsync(id, ct);
            var result = getPresenter.Result;
            if (result.Success && result.Value is not null)
            {
                var m = ProfessionalMapper.ToModel(result.Value);
                Model = new UpdateProfessionalModel
                {
                    Id = m.Id,
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    Phone = m.Phone,
                    Type = m.Type,
                    LicenseNumber = m.LicenseNumber,
                    SpecialtyId = m.SpecialtyId,
                    RowVersion = result.Value.RowVersion
                };
            }
            else
            {
                InformationMessage = result.Error?.Message ?? "Profesional no encontrado.";
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }

    public async Task UpdateAsync(CancellationToken ct = default)
    {
        IsProcessing = true;
        Success = false;
        try
        {
            InformationMessage = string.Empty;
            await updateInteractor.HandleAsync(ProfessionalMapper.ToUpdateDto(Model), ct);
            var result = updatePresenter.Result;

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join(" ", result.ValidationErrors.Select(e => e.ErrorMessage));
            }
            else if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "No se pudo editar el profesional.";
            }
            else
            {
                InformationMessage = "Profesional actualizado correctamente.";
                Success = true;
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
```

- [ ] **Step 4: Build**

Run: `dotnet build "3.InterfaceAdapters\MedRec.Professionals.ViewModels\MedRec.Professionals.ViewModels.csproj"`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 5: Commit**

```bash
git add "3.InterfaceAdapters\MedRec.Professionals.ViewModels\VM"
git commit -m "feat(professionals): CreateProfessionalVM, ProfessionalsListVM y UpdateProfessionalVM"
```

---

## Task 12: Capa 3 (Views) — `ProfessionalTypeFields`, `CreateProfessionalPage`, `UpdateProfessionalPage`, `ProfessionalsListPage`

**Files:**
- Create: `3.InterfaceAdapters\MedRec.Professionals.Views\Components\ProfessionalTypeFields.razor`
- Create: `3.InterfaceAdapters\MedRec.Professionals.Views\Pages\CreateProfessionalPage.razor`
- Create: `3.InterfaceAdapters\MedRec.Professionals.Views\Pages\UpdateProfessionalPage.razor`
- Create: `3.InterfaceAdapters\MedRec.Professionals.Views\Pages\ProfessionalsListPage.razor`
- Create: `3.InterfaceAdapters\MedRec.Professionals.Views\_Imports.razor`

**Interfaces:**
- Consumes: `CreateProfessionalVM`, `ProfessionalsListVM`, `UpdateProfessionalVM` (Task 11); `ISpecialtyLookupRepository` (Task 4); `IRoleLookupRepository`/`RoleCheckboxList` (Identity, ya existentes); `ISessionService`/`SystemPermissions` (Identity, ya existentes).

- [ ] **Step 1: `_Imports.razor`**

```razor
@using System.Net.Http
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using MedRec.Identity.Views.Components
```

- [ ] **Step 2: `ProfessionalTypeFields.razor` (componente compartido Create/Update)**

```razor
@using MedRec.Entity.Enums
@using MedRec.Professionals.BusinessObjects.DTOs

<div class="form-row">
    <label for="type">Tipo de profesional</label>
    <select id="type" value="@Type" @onchange="OnTypeChanged">
        <option value="@ProfessionalType.Doctor">Médico</option>
        <option value="@ProfessionalType.Nurse">Enfermero</option>
        <option value="@ProfessionalType.Receptionist">Recepcionista</option>
        <option value="@ProfessionalType.Administrator">Administrador</option>
    </select>
</div>

@if (Type == ProfessionalType.Doctor || Type == ProfessionalType.Nurse)
{
    <div class="form-row">
        <label for="licenseNumber">Matrícula</label>
        <input id="licenseNumber" type="text" value="@LicenseNumber" @oninput="OnLicenseNumberChanged" />
    </div>
}

@if (Type == ProfessionalType.Doctor)
{
    <div class="form-row">
        <label for="specialtyId">Especialidad</label>
        <select id="specialtyId" value="@SpecialtyId" @onchange="OnSpecialtyChanged">
            <option value="">-- Seleccionar --</option>
            @foreach (var specialty in AvailableSpecialties)
            {
                <option value="@specialty.Id">@specialty.Name</option>
            }
        </select>
    </div>
}

@code {
    [Parameter] public ProfessionalType Type { get; set; }
    [Parameter] public EventCallback<ProfessionalType> TypeChanged { get; set; }
    [Parameter] public string? LicenseNumber { get; set; }
    [Parameter] public EventCallback<string?> LicenseNumberChanged { get; set; }
    [Parameter] public Guid? SpecialtyId { get; set; }
    [Parameter] public EventCallback<Guid?> SpecialtyIdChanged { get; set; }
    [Parameter] public IReadOnlyList<SpecialtySummaryDto> AvailableSpecialties { get; set; } = Array.Empty<SpecialtySummaryDto>();

    private async Task OnTypeChanged(ChangeEventArgs e)
    {
        var type = Enum.Parse<ProfessionalType>(e.Value!.ToString()!);
        await TypeChanged.InvokeAsync(type);
    }

    private async Task OnLicenseNumberChanged(ChangeEventArgs e)
    {
        await LicenseNumberChanged.InvokeAsync(e.Value?.ToString());
    }

    private async Task OnSpecialtyChanged(ChangeEventArgs e)
    {
        var text = e.Value?.ToString();
        await SpecialtyIdChanged.InvokeAsync(string.IsNullOrWhiteSpace(text) ? null : Guid.Parse(text));
    }
}
```

- [ ] **Step 3: `CreateProfessionalPage.razor`**

```razor
@page "/professionals/create"
@inject CreateProfessionalVM VM
@inject MedRec.Professionals.BusinessObjects.Interfaces.Repositories.ISpecialtyLookupRepository SpecialtyLookup
@inject MedRec.Identity.BusinessObjects.Interfaces.Repositories.IRoleLookupRepository RoleLookup
@inject MedRec.Identity.BusinessObjects.Interfaces.Services.ISessionService SessionService
@using MedRec.Identity.BusinessObjects.Constants
@using MedRec.Professionals.ViewModels.Models
@using MedRec.Professionals.Views.Components

<div class="page-container">
    <h2>Nuevo profesional</h2>

    @if (SessionService.CurrentUser?.Permissions.Contains(SystemPermissions.Professionals_Create) != true)
    {
        <p class="info-message">No tiene permiso para crear profesionales.</p>
        return;
    }

    <EditForm Model="VM.Model" OnValidSubmit="HandleCreate" autocomplete="off">
        <div class="form-row">
            <label for="firstName">Nombre</label>
            <InputText id="firstName" @bind-Value="VM.Model.FirstName" />
        </div>

        <div class="form-row">
            <label for="lastName">Apellido</label>
            <InputText id="lastName" @bind-Value="VM.Model.LastName" />
        </div>

        <div class="form-row">
            <label for="email">Email</label>
            <InputText id="email" @bind-Value="VM.Model.Email" />
        </div>

        <div class="form-row">
            <label for="phone">Teléfono</label>
            <InputText id="phone" @bind-Value="VM.Model.Phone" />
        </div>

        <div class="form-row">
            <label for="hireDate">Fecha de ingreso</label>
            <InputDate id="hireDate" @bind-Value="VM.Model.HireDate" />
        </div>

        <ProfessionalTypeFields @bind-Type="VM.Model.Type"
                                 @bind-LicenseNumber="VM.Model.LicenseNumber"
                                 @bind-SpecialtyId="VM.Model.SpecialtyId"
                                 AvailableSpecialties="_specialties" />

        <div class="form-row">
            <label>
                <input type="checkbox" @bind="_createUser" />
                Crear también un usuario para este profesional
            </label>
        </div>

        @if (_createUser)
        {
            <div class="form-row">
                <label>Email del usuario</label>
                <input type="text" value="@VM.Model.Email" disabled />
            </div>

            <div class="form-row">
                <label>Nombre del usuario</label>
                <input type="text" value="@($"{VM.Model.FirstName} {VM.Model.LastName}")" disabled />
            </div>

            <div class="form-row">
                <label for="temporaryPassword">Contraseña temporal</label>
                <input id="temporaryPassword" type="text" @bind="_temporaryPassword" />
            </div>

            <div class="form-row">
                <label>Roles</label>
                <RoleCheckboxList AvailableRoles="_roles" @bind-SelectedRoleIds="_selectedRoleIds" />
            </div>
        }

        @if (!string.IsNullOrEmpty(VM.InformationMessage))
        {
            <p class="info-message">@VM.InformationMessage</p>
        }

        <button type="submit" disabled="@VM.IsProcessing">
            @(VM.IsProcessing ? "Guardando..." : "Crear profesional")
        </button>
    </EditForm>
</div>

@code {
    private IReadOnlyList<MedRec.Professionals.BusinessObjects.DTOs.SpecialtySummaryDto> _specialties = Array.Empty<MedRec.Professionals.BusinessObjects.DTOs.SpecialtySummaryDto>();
    private IReadOnlyList<MedRec.Identity.BusinessObjects.DTOs.RoleSummaryDto> _roles = Array.Empty<MedRec.Identity.BusinessObjects.DTOs.RoleSummaryDto>();
    private bool _createUser;
    private string _temporaryPassword = string.Empty;
    private List<Guid> _selectedRoleIds = new();

    protected override async Task OnInitializedAsync()
    {
        _specialties = await SpecialtyLookup.ListActiveAsync();
        _roles = await RoleLookup.ListActiveAsync();
    }

    private async Task HandleCreate()
    {
        VM.Model.CreateUser = _createUser
            ? new CreateUserForProfessionalModel { TemporaryPassword = _temporaryPassword, RoleIds = _selectedRoleIds }
            : null;

        await VM.CreateAsync();

        if (VM.Success)
        {
            _createUser = false;
            _temporaryPassword = string.Empty;
            _selectedRoleIds = new();
        }

        StateHasChanged();
    }
}
```

- [ ] **Step 4: `UpdateProfessionalPage.razor`**

```razor
@page "/professionals/{ProfessionalId:guid}/edit"
@inject UpdateProfessionalVM VM
@inject MedRec.Professionals.BusinessObjects.Interfaces.Repositories.ISpecialtyLookupRepository SpecialtyLookup
@inject MedRec.Identity.BusinessObjects.Interfaces.Services.ISessionService SessionService
@using MedRec.Identity.BusinessObjects.Constants
@using MedRec.Professionals.Views.Components

<div class="page-container">
    <h2>Editar profesional</h2>

    @if (SessionService.CurrentUser?.Permissions.Contains(SystemPermissions.Professionals_Edit) != true)
    {
        <p class="info-message">No tiene permiso para editar profesionales.</p>
        return;
    }

    <EditForm Model="VM.Model" OnValidSubmit="HandleUpdate" autocomplete="off">
        <div class="form-row">
            <label for="firstName">Nombre</label>
            <InputText id="firstName" @bind-Value="VM.Model.FirstName" />
        </div>

        <div class="form-row">
            <label for="lastName">Apellido</label>
            <InputText id="lastName" @bind-Value="VM.Model.LastName" />
        </div>

        <div class="form-row">
            <label for="phone">Teléfono</label>
            <InputText id="phone" @bind-Value="VM.Model.Phone" />
        </div>

        <ProfessionalTypeFields @bind-Type="VM.Model.Type"
                                 @bind-LicenseNumber="VM.Model.LicenseNumber"
                                 @bind-SpecialtyId="VM.Model.SpecialtyId"
                                 AvailableSpecialties="_specialties" />

        @if (!string.IsNullOrEmpty(VM.InformationMessage))
        {
            <p class="info-message">@VM.InformationMessage</p>
        }

        <button type="submit" disabled="@VM.IsProcessing">
            @(VM.IsProcessing ? "Guardando..." : "Guardar cambios")
        </button>
    </EditForm>
</div>

@code {
    [Parameter] public Guid ProfessionalId { get; set; }

    private IReadOnlyList<MedRec.Professionals.BusinessObjects.DTOs.SpecialtySummaryDto> _specialties = Array.Empty<MedRec.Professionals.BusinessObjects.DTOs.SpecialtySummaryDto>();

    protected override async Task OnInitializedAsync()
    {
        _specialties = await SpecialtyLookup.ListActiveAsync();
        await VM.LoadAsync(ProfessionalId);
    }

    private async Task HandleUpdate()
    {
        await VM.UpdateAsync();

        if (VM.Success)
        {
            // Recargar (incluyendo RowVersion) para que un segundo guardado consecutivo
            // no dispare un falso conflicto de concurrencia — mismo criterio que UpdateUserPage.
            await VM.LoadAsync(ProfessionalId);
        }

        StateHasChanged();
    }
}
```

- [ ] **Step 5: `ProfessionalsListPage.razor`**

```razor
@page "/professionals"
@using MedRec.Identity.BusinessObjects.Constants
@inject ProfessionalsListVM VM
@inject MedRec.Identity.BusinessObjects.Interfaces.Services.ISessionService SessionService

<div class="page-container">
    <h2>Profesionales</h2>
    @if (SessionService.CurrentUser?.Permissions.Contains(SystemPermissions.Professionals_Create) == true)
    {
        <a href="/professionals/create">+ Nuevo profesional</a>
    }

    @if (!string.IsNullOrEmpty(VM.InformationMessage))
    {
        <p class="info-message">@VM.InformationMessage</p>
    }

    <table>
        <thead>
            <tr>
                <th>Nombre</th>
                <th>Tipo</th>
                <th>Email</th>
                <th>Teléfono</th>
                <th>Acciones</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var professional in VM.Professionals)
            {
                <tr>
                    <td>@professional.FullName</td>
                    <td>@professional.Type</td>
                    <td>@professional.Email</td>
                    <td>@professional.Phone</td>
                    <td>
                        @if (SessionService.CurrentUser?.Permissions.Contains(SystemPermissions.Professionals_Edit) == true)
                        {
                            <a href="@($"/professionals/{professional.Id}/edit")">Editar</a>
                        }
                        @if (SessionService.CurrentUser?.Permissions.Contains(SystemPermissions.Professionals_Delete) == true)
                        {
                            <button @onclick="() => DeleteAsync(professional.Id)" disabled="@VM.IsProcessing">Eliminar</button>
                        }
                    </td>
                </tr>
            }
        </tbody>
    </table>
</div>

@code {
    protected override async Task OnInitializedAsync()
    {
        await VM.LoadAsync();
    }

    private async Task DeleteAsync(Guid id)
    {
        await VM.DeleteAsync(id);
        StateHasChanged();
    }
}
```

- [ ] **Step 6: Build**

Run: `dotnet build "3.InterfaceAdapters\MedRec.Professionals.Views\MedRec.Professionals.Views.csproj"`
Expected: FAIL (esperado — todavía no está registrado en DI ni referenciado desde el host; ver Task 13). Confirmar que el único error es de resolución de tipos DI en tiempo de ejecución, no de compilación de Razor/C#; si hay errores de compilación de markup, corregirlos ahora.

- [ ] **Step 7: Commit**

```bash
git add "3.InterfaceAdapters\MedRec.Professionals.Views"
git commit -m "feat(professionals): páginas de alta, edición y listado de Profesionales"
```

---

## Task 13: Wiring final — IoC, host WPF, NavMenu

**Files:**
- Modify: `4.Framework&Drivers\MedRec.IoC\MedRec.IoC.csproj`
- Modify: `4.Framework&Drivers\MedRec.IoC\DependencyContainer.cs`
- Modify: `4.Framework&Drivers\MedRec.WPF.UI\MedRec.WPF.UI.csproj`
- Modify: `4.Framework&Drivers\MedRec.WPF.UI\Main.razor`
- Modify: `4.Framework&Drivers\MedRec.WPF.UI\Shared\NavMenu.razor`

**Interfaces:**
- Consumes: `AddProfessionalsDataContextMySqlServices`, `AddProfessionalsRepositoriesServices`, `AddProfessionalsUseCasesServicesWithProxy`, `AddProfessionalsPresentersServices` (Tasks 6-9); `CreateProfessionalVM`, `ProfessionalsListVM`, `UpdateProfessionalVM` (Task 11).

- [ ] **Step 1: Agregar referencias de proyecto a `MedRec.IoC.csproj`**

Dentro del `<ItemGroup>` de `ProjectReference`, agregar:
```xml
    <ProjectReference Include="..\..\2.ApplicationBusinessObjects\MedRec.Professionals.UseCases\MedRec.Professionals.UseCases.csproj" />
    <ProjectReference Include="..\..\3.InterfaceAdapters\Repositories\MedRec.Professionals.Repositories\MedRec.Professionals.Repositories.csproj" />
    <ProjectReference Include="..\..\3.InterfaceAdapters\MedRec.Professionals.Presenters\MedRec.Professionals.Presenters.csproj" />
    <ProjectReference Include="..\..\3.InterfaceAdapters\MedRec.Professionals.ViewModels\MedRec.Professionals.ViewModels.csproj" />
    <ProjectReference Include="..\MedRec.Professionals.DataContext.MySql\MedRec.Professionals.DataContext.MySql.csproj" />
```

- [ ] **Step 2: Registrar los servicios en `DependencyContainer.AddAppServices()`**

En `4.Framework&Drivers\MedRec.IoC\DependencyContainer.cs`, agregar el `using`:
```csharp
using MedRec.Professionals.ViewModels.VM;
```

Y, después del bloque de Identity (`services.AddTransient<ChangePasswordVM>();`, antes de `services.AddValidatorServices();`), agregar:
```csharp

        services.AddProfessionalsDataContextMySqlServices()
                .AddProfessionalsRepositoriesServices()
                .AddProfessionalsUseCasesServicesWithProxy()
                .AddProfessionalsPresentersServices();

        services.AddTransient<CreateProfessionalVM>();
        services.AddTransient<ProfessionalsListVM>();
        services.AddTransient<UpdateProfessionalVM>();
```

- [ ] **Step 3: Referenciar `MedRec.Professionals.ViewModels`/`.Views` desde el host WPF**

En `4.Framework&Drivers\MedRec.WPF.UI\MedRec.WPF.UI.csproj`, dentro del `<ItemGroup>` de `ProjectReference`, agregar:
```xml
    <ProjectReference Include="..\..\3.InterfaceAdapters\MedRec.Professionals.ViewModels\MedRec.Professionals.ViewModels.csproj" />
    <ProjectReference Include="..\..\3.InterfaceAdapters\MedRec.Professionals.Views\MedRec.Professionals.Views.csproj" />
```

- [ ] **Step 4: Registrar el assembly de rutas en `Main.razor`**

En `4.Framework&Drivers\MedRec.WPF.UI\Main.razor`, agregar el `@using`:
```razor
@using MedRec.Professionals.Views.Pages
```

Y agregar `typeof(ProfessionalsListPage).Assembly` a la lista `AdditionalAssemblies`:
```razor
<Router AppAssembly="@typeof(MainLayout).Assembly"
		AdditionalAssemblies="new[] {typeof(CreatePatientPage).Assembly,
									 typeof(CreateMedicalVisitPage).Assembly,
                                     typeof(AppointmentScheduleComponent).Assembly,
									 typeof(Home).Assembly,
									 typeof(UsersListPage).Assembly,
									 typeof(ProfessionalsListPage).Assembly}">
```

- [ ] **Step 5: Nuevo ítem de menú en `NavMenu.razor`**

En `4.Framework&Drivers\MedRec.WPF.UI\Shared\NavMenu.razor`, después del `<li>` de "Usuarios" (antes del `<li>` de "Contacto"), agregar:
```razor
        @if (SessionService.CurrentUser?.Permissions.Contains(SystemPermissions.Professionals_View) == true)
        {
            <li>
                <a href="/professionals">
                    <i class="bi bi-person-badge"></i>
                    @if (!_isCollapsed)
                    {
                        <span>Profesionales</span>
                    }
                </a>
            </li>
        }
```

- [ ] **Step 6: Build completo de la solución**

Run: `dotnet build MedRecSolution2025.sln`
Expected: `Build succeeded. 0 Error(s)`.

- [ ] **Step 7: Correr toda la suite de tests**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: PASS.

Run: `dotnet test "Test\MedRec.MedicalVisit.UseCases.Tests\MedRec.MedicalVisit.UseCases.Tests.csproj"`
Expected: PASS.

Run: `dotnet test "Test\MedRec.Professionals.UseCases.Tests\MedRec.Professionals.UseCases.Tests.csproj"`
Expected: PASS (12/12).

- [ ] **Step 8: Aplicar la migración a la base de datos de desarrollo**

Run: `dotnet ef database update --project "4.Framework&Drivers\MedRec.DataContext.MySql" --startup-project "4.Framework&Drivers\MedRec.WPF.UI"`
Expected: aplica `AddProfessionalsWithTypes` sin errores; `Doctors` pasa a llamarse `Professionals` con la columna `Type` nueva, y los datos de médicos existentes se preservan.

- [ ] **Step 9: Prueba manual en la app**

Usar la skill `run` para lanzar la app WPF y verificar en vivo:
- El ítem "Profesionales" aparece en el menú solo con `professionals.view`.
- Alta de un Recepcionista sin matrícula/especialidad — debe guardar sin pedirlas.
- Alta de un Médico sin especialidad — debe rechazar con el mensaje de validación.
- Alta de un profesional con "Crear también un usuario" tildado — el usuario debe poder loguearse después con la contraseña temporal, y figurar en `/users` vinculado al profesional recién creado.
- Edición de un profesional cambia correctamente los campos condicionales al cambiar el tipo.
- El combo "Profesional vinculado" de `CreateUserPage`/`UpdateUserPage` sigue mostrando solo médicos.

- [ ] **Step 10: Commit**

```bash
git add -A
git commit -m "feat(professionals): wiring final en IoC, host WPF y NavMenu"
```

---

## Self-Review

**1. Cobertura de la spec:** rename completo Doctor→Professional (Task 1) ✓; tipos de profesional y campos condicionales (Tasks 1, 4, 12) ✓; permisos nuevos (Task 3) ✓; CRUD completo — alta/edición/listado/baja lógica (Tasks 5, 6, 11, 12) ✓; alta combinada con Usuario + compensación transaccional (Task 10) ✓; lookup de especialidades nuevo (Tasks 4, 7, 8) ✓; asignación de turnos/visitas restringida a Doctor preservada vía el filtro del lookup renombrado (Task 1, Step 7) ✓; permisos a nivel interactor (decisión corregida en la spec) (Tasks 5, 6) ✓; tests de interactores y del Orchestrator (Tasks 5, 6, 10) ✓; wiring en IoC/host/menú (Task 13) ✓. Institución/Clínica y aislamiento por profesional quedan explícitamente fuera de alcance, sin tareas asociadas (correcto, están diferidos a specs futuras).

**2. Placeholders:** no quedan `TODO`/`TBD`; todo paso de código trae contenido completo. El único punto no 100% determinista es el Step 5 del Task 1 (el scaffold automático de `dotnet ef migrations add`), pero el Step 6 siguiente reemplaza su contenido íntegramente por código exacto — no depende de lo que el tool haya generado.

**3. Consistencia de tipos:** `ProfessionalDto`/`CreateProfessionalDto`/`UpdateProfessionalDto` mantienen los mismos nombres de parámetros y orden en DTOs, Validators, Interactores, Mapper y tests. `IProfessionalRepositoryUoW` tiene la misma firma en la interfaz (Task 4), el repositorio (Task 7) y su implementación EF (Task 8). Los nombres de Actions/Ports coinciden entre su declaración (Task 4/10) y su consumo en el Orchestrator/VMs (Tasks 10-11).

---

## Execution Handoff

Plan completo y guardado en `docs/superpowers/plans/2026-08-12-professionals-crud.md`. Dos opciones de ejecución:

**1. Subagent-Driven (recomendado)** — despacho un subagente nuevo por task, con revisión entre tasks, iteración rápida.

**2. Ejecución en esta sesión** — ejecuto los tasks en lote en esta misma conversación, con checkpoints para revisión.

¿Cuál preferís?
