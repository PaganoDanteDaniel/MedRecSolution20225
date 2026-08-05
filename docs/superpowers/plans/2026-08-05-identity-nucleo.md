# Identity — Núcleo (dominio, login, sesión, auditoría, migración) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Después de este plan, MedRecSolution2025 exige login para entrar, registra automáticamente quién creó/modificó cada fila y cuándo (en las 16 entidades existentes + las nuevas de Identity), y expone `IAuthorizationService` listo para que otras features lo consuman (el cableado de los chequeos de permiso dentro de cada interactor existente es un plan aparte).

**Architecture:** Nueva feature slice `Identity` siguiendo el patrón de 5 proyectos + DataContext ya usado por Patients/HealthInsurance/MedicalVisit (ver `CLAUDE.md`). El login pasa por `AuthenticateUserInteractor` → JWT con claims de roles/permisos → `ISessionService` en memoria (sin persistencia entre aperturas) → `AppShell.razor` gatea el `Router` existente. La auditoría se centraliza sobreescribiendo `MedRecContext.SaveChangesAsync`, sin tocar los interactores existentes de otras features.

**Tech Stack:** .NET 9, EF Core (Pomelo.EntityFrameworkCore.MySql), `Microsoft.AspNetCore.Identity.PasswordHasher<TUser>` (paquete `Microsoft.Extensions.Identity.Core`), `System.IdentityModel.Tokens.Jwt`, xUnit + Moq.

## Global Constraints

- Namespaces, nombres de proyecto y ubicación de carpetas deben replicar exactamente el patrón de la feature `Patients` (ver spec `docs/superpowers/specs/2026-08-05-identity-design.md`, sección "Arquitectura").
- Los proyectos `Repositories` van siempre bajo `3.InterfaceAdapters\Repositories\MedRec.<Feature>.Repositories\` (no directo bajo `3.InterfaceAdapters\`) — confirmado contra `MedRecSolution2025.sln`.
- Toda interfaz que termine en `InputPort` se registra automáticamente con el proxy de excepciones vía `AddUseCaseExceptionDecorators` (ver `2.ApplicationBusinessObjects\MedRec.BusinessObjects\DependencyContainer.cs`) — no agregar try/catch manual en interactores.
- Mensajes de validación/UI en español, siguiendo el estilo existente (`"El email es obligatorio."`).
- No modificar destructivamente datos de producción sin el `DROP TABLE IF EXISTS` de las 6 tablas huérfanas documentado en el spec — ese paso va primero en la migración.
- TargetFramework `net9.0` (Razor: `net9.0`, WPF host ya en `net9.0-windows7.0`, sin cambios ahí salvo lo indicado).

---

### Task 1: Fundamentos de capa 1 — auditoría, contexto de usuario actual, código de error Forbidden

**Files:**
- Create: `1.EnterpriseBusinessObjects\MedRec.Entity\Interfaces\IAuditableEntity.cs`
- Create: `1.EnterpriseBusinessObjects\MedRec.Entity\Interfaces\ICurrentUserContext.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\Enums\ErrorCode.cs`

**Interfaces:**
- Produces: `IAuditableEntity` (CreatedBy/CreatedAt/UpdatedBy/UpdatedAt), `ICurrentUserContext.UserId : Guid?`, `ErrorCode.Forbidden` — usados por todas las tareas siguientes.

- [ ] **Step 1: Crear `IAuditableEntity`**

```csharp
namespace MedRec.Entity.Interfaces;

public interface IAuditableEntity
{
    Guid? CreatedBy { get; set; }
    DateTime CreatedAt { get; set; }
    Guid? UpdatedBy { get; set; }
    DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 2: Crear `ICurrentUserContext`**

```csharp
namespace MedRec.Entity.Interfaces;

public interface ICurrentUserContext
{
    Guid? UserId { get; }
}
```

- [ ] **Step 3: Agregar `Forbidden` a `ErrorCode`**

Reemplazar el contenido de `1.EnterpriseBusinessObjects\MedRec.Entity\Enums\ErrorCode.cs`:

```csharp
namespace MedRec.Entity.Enums;
public enum ErrorCode
{
    None = 0,
    Cancelled,
    DuplicateKey,
    ConcurrencyError,
    ValidationError,
    DatabaseError,
    UpdateError,
    NotFound,
    Conflict,
    Forbidden,
    Unknown
}
```

- [ ] **Step 4: Compilar `MedRec.Entity` para verificar que no rompe nada**

Run: `dotnet build "1.EnterpriseBusinessObjects\MedRec.Entity\MedRec.Entity.csproj"`
Expected: Build succeeded.

- [ ] **Step 5: Commit**

```bash
git add "1.EnterpriseBusinessObjects/MedRec.Entity/Interfaces/IAuditableEntity.cs" "1.EnterpriseBusinessObjects/MedRec.Entity/Interfaces/ICurrentUserContext.cs" "1.EnterpriseBusinessObjects/MedRec.Entity/Enums/ErrorCode.cs"
git commit -m "feat(identity): agregar IAuditableEntity, ICurrentUserContext y ErrorCode.Forbidden"
```

---

### Task 2: Extender `BaseOutputPort<T>` para mapear `ErrorCode.Forbidden`

**Files:**
- Modify: `2.ApplicationBusinessObjects\MedRec.BusinessObjects\Abstracts\BaseOutputPort.cs`

- [ ] **Step 1: Agregar el case al switch existente**

En el método `ErrorAsync`, el switch que mapea `message.Code` a `UserMessageAction` queda:

```csharp
        var action = message.Code switch
        {
            ErrorCode.DuplicateKey => UserMessageAction.ShowWarning,
            ErrorCode.ConcurrencyError => UserMessageAction.ShowConcurrencyMessage,
            ErrorCode.DatabaseError => UserMessageAction.ShowError,
            ErrorCode.NotFound => UserMessageAction.ShowError,
            ErrorCode.Forbidden => UserMessageAction.ShowError,
            ErrorCode.Cancelled => UserMessageAction.ShowInfoMessage,
            _ => UserMessageAction.ShowError
        };
```

- [ ] **Step 2: Compilar**

Run: `dotnet build "2.ApplicationBusinessObjects\MedRec.BusinessObjects\MedRec.BusinessObjects.csproj"`
Expected: Build succeeded.

- [ ] **Step 3: Commit**

```bash
git add "2.ApplicationBusinessObjects/MedRec.BusinessObjects/Abstracts/BaseOutputPort.cs"
git commit -m "feat(identity): mapear ErrorCode.Forbidden a UserMessageAction.ShowError"
```

---

### Task 3: Auditoría en las 16 entidades existentes

**Files:**
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\Patient.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\Doctor.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\MedicalAppointment.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\PatientMedicalVisit.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\City.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\Province.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\HealthInsuranceCompany.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\LaboratoryResultType.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\MedicalCondition.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\MedicalConditionType.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\PatientLaboratoryResult.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\PatientMedicalCondition.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\PatientMedicalHistory.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\MedicalSpecialty.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\TemplateFieldDefinition.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\MedicalVisitDynamicField.cs`

**Nota importante para el implementador:** el dump de `medrecdb` provisto por el cliente (2026-08-05) NO incluye las tablas `laboratoryresulttypes`, `medicalconditions`, `medicalconditiontypes`, `patientlaboratoryresults` ni `patientmedicalconditions`, aunque sus entidades sí existen en el código. Antes de aplicar la migración (Task 11) en producción, correr `SHOW TABLES LIKE '<nombre>';` para cada una y confirmar si existen. Este task igual actualiza las 13 entidades en código (es inofensivo), pero la migración del Task 11 solo emite `ALTER TABLE` para las 11 tablas confirmadas en el dump.

13 entidades sin `CreatedAt`/`UpdatedAt` previo: agregar las 4 propiedades + `: IAuditableEntity`. 3 entidades que ya tienen `CreatedAt`/`UpdatedAt` (`MedicalSpecialty`, `TemplateFieldDefinition`, `MedicalVisitDynamicField`): agregar solo `CreatedBy`/`UpdatedBy` + `: IAuditableEntity`.

- [ ] **Step 1: Patient.cs**

```csharp
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class Patient : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DocumentNumber { get; set; }
    public string Address { get; set; }
    public Guid? CityId { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public DateTime DateOfBirth { get; set; }
    public BiologicalSex BiologicalSexId { get; set; } = BiologicalSex.Unknown;
    public Guid? HealthInsuranceId { get; set; }
    public string HealthInsuranceMemberNumber { get; set; }
    public string HealthInsuranceCard { get; set; }
    public string HealthInsurancePlan { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string FullName => $"{LastName}, {FirstName}";
}
```

- [ ] **Step 2: Doctor.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;

public class Doctor : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public Guid SpecialtyId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime HireDate { get; set; } = DateTime.Now;
    public bool IsDeleted { get; set; } = true;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string FullName => $"{LastName}, {FirstName}";
}
```

- [ ] **Step 3: MedicalAppointment.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class MedicalAppointment : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime DateTime { get; set; }
    public Guid PatientId { get; set; }
    public Guid DoctorId { get; set; } = Guid.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 4: PatientMedicalVisit.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;

public class PatientMedicalVisit : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MedicalHistoryId { get; set; }
    public DateTime VisitDate { get; set; }
    public string Reason { get; set; }
    public string Diagnosis { get; set; } = String.Empty;
    public string Treatment { get; set; } = String.Empty;
    public int? SystolicPressure { get; set; }
    public int? DiastolicPressure { get; set; }
    public int? PulsePerMinute { get; set; }
    public double? Temperature { get; set; }
    public string Notes { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? DoctorId { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 5: City.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class City : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProvinceId { get; set; }
    public string CityName { get; set; }
    public string CityCode { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 6: Province.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class Province : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 7: HealthInsuranceCompany.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class HealthInsuranceCompany : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Acronym { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 8: LaboratoryResultType.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class LaboratoryResultType : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ResultName { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 9: MedicalCondition.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class MedicalCondition : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConditionTypeId { get; set; }
    public string Name { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 10: MedicalConditionType.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class MedicalConditionType : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 11: PatientLaboratoryResult.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class PatientLaboratoryResult : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LaboratoryResultId { get; set; }
    public Guid MedicalVisitId { get; set; }
    public DateTime ResultDate { get; set; }
    public string ResultValue { get; set; }
    public string ResultNotes { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 12: PatientMedicalCondition.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class PatientMedicalCondition : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientMedicalHistoryId { get; set; }
    public Guid MedicalConditionId { get; set; }
    public string Description { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 13: PatientMedicalHistory.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class PatientMedicalHistory : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public string Notes { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 14: MedicalSpecialty.cs** (ya tenía CreatedAt/UpdatedAt)

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;

public class MedicalSpecialty : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public bool IsDeleted { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
    public byte[] RowVersion { get; set; }
}
```

- [ ] **Step 15: TemplateFieldDefinition.cs** (ya tenía CreatedAt/UpdatedAt)

Agregar `using MedRec.Entity.Interfaces;`, cambiar `public class TemplateFieldDefinition` por `public class TemplateFieldDefinition : IAuditableEntity`, y agregar `public Guid? CreatedBy { get; set; }` / `public Guid? UpdatedBy { get; set; }` (el resto de la clase queda igual).

- [ ] **Step 16: MedicalVisitDynamicField.cs** (ya tenía CreatedAt/UpdatedAt)

Agregar `using MedRec.Entity.Interfaces;`, cambiar `public class MedicalVisitDynamicField` por `public class MedicalVisitDynamicField : IAuditableEntity`, y agregar `public Guid? CreatedBy { get; set; }` / `public Guid? UpdatedBy { get; set; }` (el resto de la clase queda igual).

- [ ] **Step 17: Compilar**

Run: `dotnet build "1.EnterpriseBusinessObjects\MedRec.Entity\MedRec.Entity.csproj"`
Expected: Build succeeded (0 errores).

- [ ] **Step 18: Commit**

```bash
git add "1.EnterpriseBusinessObjects/MedRec.Entity/POCOEntities/"
git commit -m "feat(identity): implementar IAuditableEntity en las 16 entidades existentes"
```

---

### Task 4: Entidades nuevas de Identity

**Files:**
- Create: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\User.cs`
- Create: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\Role.cs`
- Create: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\Permission.cs`
- Create: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\UserRole.cs`
- Create: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\RolePermission.cs`

- [ ] **Step 1: User.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class User : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public string FullName { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? DoctorId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 2: Role.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class Role : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 3: Permission.cs**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.Entity.POCOEntities;
public class Permission : IAuditableEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; }
    public string Description { get; set; }
    public bool IsDeleted { get; set; } = false;
    public byte[] RowVersion { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

- [ ] **Step 4: UserRole.cs (tabla puente, sin auditoría)**

```csharp
namespace MedRec.Entity.POCOEntities;
public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}
```

- [ ] **Step 5: RolePermission.cs (tabla puente, sin auditoría)**

```csharp
namespace MedRec.Entity.POCOEntities;
public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
}
```

- [ ] **Step 6: Compilar y commit**

Run: `dotnet build "1.EnterpriseBusinessObjects\MedRec.Entity\MedRec.Entity.csproj"`
Expected: Build succeeded.

```bash
git add "1.EnterpriseBusinessObjects/MedRec.Entity/POCOEntities/User.cs" "1.EnterpriseBusinessObjects/MedRec.Entity/POCOEntities/Role.cs" "1.EnterpriseBusinessObjects/MedRec.Entity/POCOEntities/Permission.cs" "1.EnterpriseBusinessObjects/MedRec.Entity/POCOEntities/UserRole.cs" "1.EnterpriseBusinessObjects/MedRec.Entity/POCOEntities/RolePermission.cs"
git commit -m "feat(identity): agregar entidades User, Role, Permission, UserRole, RolePermission"
```

---

### Task 5: Scaffolding del proyecto `MedRec.Identity.BusinessObjects`

**Files:**
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\MedRec.Identity.BusinessObjects.csproj`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Constants\SystemPermissions.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\AuthenticateUserDto.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\AuthResultDto.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\IAuthenticateUserInputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\IAuthenticateUserOutputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Services\IAuthorizationService.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Services\IPasswordHasher.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Services\IAuthTokenGenerator.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Services\ISessionService.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Repositories\IUserQueriesRepository.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Validators\AuthenticateUserValidator.cs`

**Interfaces:**
- Produces: `AuthenticateUserDto(string Email, string Password)`, `AuthResultDto(Guid UserId, string Email, string FullName, Guid? DoctorId, IReadOnlyList<string> Roles, IReadOnlyList<string> Permissions, string Token, DateTime ExpiresAtUtc)`, `IAuthenticateUserInputPort.HandleAsync(AuthenticateUserDto, CancellationToken)`, `IAuthenticateUserOutputPort.Handle(AuthResultDto, CancellationToken)` / `.InvalidCredentials()`, `IAuthorizationService.EnsurePermissionAsync(Guid?, string, CancellationToken)`, `IPasswordHasher.Hash(string)` / `.Verify(string,string)`, `IAuthTokenGenerator.GenerateToken(...)`, `ISessionService`, `IUserQueriesRepository.GetByEmailAsync/GetRoleNamesAsync/GetPermissionCodesAsync`. Usados por Tasks 6-13.

- [ ] **Step 1: Scaffolding del proyecto vía CLI**

Run:
```bash
dotnet new classlib -n MedRec.Identity.BusinessObjects -o "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects" --no-restore
rm "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/Class1.cs"
dotnet sln MedRecSolution2025.sln add "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/MedRec.Identity.BusinessObjects.csproj"
dotnet add "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/MedRec.Identity.BusinessObjects.csproj" reference "1.EnterpriseBusinessObjects/MedRec.Entity/MedRec.Entity.csproj"
dotnet add "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/MedRec.Identity.BusinessObjects.csproj" reference "1.EnterpriseBusinessObjects/MedRec.Shared/MedRec.Shared.csproj"
dotnet add "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/MedRec.Identity.BusinessObjects.csproj" reference "2.ApplicationBusinessObjects/MedRec.BusinessObjects/MedRec.BusinessObjects.csproj"
```

Expected: el `.csproj` generado queda equivalente a (verificar y ajustar `TargetFramework`/`ImplicitUsings` si el template no los puso):

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
  </ItemGroup>
</Project>
```

- [ ] **Step 2: SystemPermissions.cs**

```csharp
namespace MedRec.Identity.BusinessObjects.Constants;

public static class SystemPermissions
{
    public const string Patients_View = "patients.view";
    public const string Patients_Create = "patients.create";
    public const string Patients_Edit = "patients.edit";
    public const string Patients_Delete = "patients.delete";

    public const string MedicalVisits_View = "medicalvisits.view";
    public const string MedicalVisits_Create = "medicalvisits.create";
    public const string MedicalVisits_Edit = "medicalvisits.edit";
    public const string MedicalVisits_Delete = "medicalvisits.delete";

    public const string Appointments_View = "appointments.view";
    public const string Appointments_Create = "appointments.create";
    public const string Appointments_Edit = "appointments.edit";
    public const string Appointments_Delete = "appointments.delete";

    public const string HealthInsurance_View = "healthinsurance.view";
    public const string HealthInsurance_Create = "healthinsurance.create";
    public const string HealthInsurance_Edit = "healthinsurance.edit";
    public const string HealthInsurance_Delete = "healthinsurance.delete";

    public const string DynamicTemplates_View = "dynamictemplates.view";
    public const string DynamicTemplates_Create = "dynamictemplates.create";
    public const string DynamicTemplates_Edit = "dynamictemplates.edit";
    public const string DynamicTemplates_Delete = "dynamictemplates.delete";

    public const string Users_View = "users.view";
    public const string Users_Create = "users.create";
    public const string Users_Edit = "users.edit";
    public const string Users_Delete = "users.delete";

    public const string Roles_View = "roles.view";
    public const string Roles_Create = "roles.create";
    public const string Roles_Edit = "roles.edit";
    public const string Roles_Delete = "roles.delete";

    public static readonly IReadOnlyList<(string Code, string Description)> All = new[]
    {
        (Patients_View, "Ver pacientes"),
        (Patients_Create, "Crear pacientes"),
        (Patients_Edit, "Editar pacientes"),
        (Patients_Delete, "Eliminar pacientes"),
        (MedicalVisits_View, "Ver visitas médicas"),
        (MedicalVisits_Create, "Crear visitas médicas"),
        (MedicalVisits_Edit, "Editar visitas médicas"),
        (MedicalVisits_Delete, "Eliminar visitas médicas"),
        (Appointments_View, "Ver turnos"),
        (Appointments_Create, "Crear turnos"),
        (Appointments_Edit, "Editar turnos"),
        (Appointments_Delete, "Eliminar turnos"),
        (HealthInsurance_View, "Ver obras sociales"),
        (HealthInsurance_Create, "Crear obras sociales"),
        (HealthInsurance_Edit, "Editar obras sociales"),
        (HealthInsurance_Delete, "Eliminar obras sociales"),
        (DynamicTemplates_View, "Ver plantillas de campos dinámicos"),
        (DynamicTemplates_Create, "Crear plantillas de campos dinámicos"),
        (DynamicTemplates_Edit, "Editar plantillas de campos dinámicos"),
        (DynamicTemplates_Delete, "Eliminar plantillas de campos dinámicos"),
        (Users_View, "Ver usuarios"),
        (Users_Create, "Crear usuarios"),
        (Users_Edit, "Editar usuarios"),
        (Users_Delete, "Eliminar usuarios"),
        (Roles_View, "Ver roles"),
        (Roles_Create, "Crear roles"),
        (Roles_Edit, "Editar roles"),
        (Roles_Delete, "Eliminar roles"),
    };
}
```

- [ ] **Step 3: DTOs**

`DTOs\AuthenticateUserDto.cs`:
```csharp
namespace MedRec.Identity.BusinessObjects.DTOs;
public class AuthenticateUserDto
{
    public AuthenticateUserDto(string email, string password)
    {
        Email = email;
        Password = password;
    }

    public string Email { get; }
    public string Password { get; }
}
```

`DTOs\AuthResultDto.cs`:
```csharp
namespace MedRec.Identity.BusinessObjects.DTOs;
public class AuthResultDto
{
    public AuthResultDto(
        Guid userId,
        string email,
        string fullName,
        Guid? doctorId,
        IReadOnlyList<string> roles,
        IReadOnlyList<string> permissions,
        string token,
        DateTime expiresAtUtc)
    {
        UserId = userId;
        Email = email;
        FullName = fullName;
        DoctorId = doctorId;
        Roles = roles;
        Permissions = permissions;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid UserId { get; }
    public string Email { get; }
    public string FullName { get; }
    public Guid? DoctorId { get; }
    public IReadOnlyList<string> Roles { get; }
    public IReadOnlyList<string> Permissions { get; }
    public string Token { get; }
    public DateTime ExpiresAtUtc { get; }
}
```

- [ ] **Step 4: Ports**

`Interfaces\Ports\IAuthenticateUserInputPort.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IAuthenticateUserInputPort
{
    Task HandleAsync(AuthenticateUserDto dto, CancellationToken ct = default);
}
```

`Interfaces\Ports\IAuthenticateUserOutputPort.cs`:
```csharp
using MedRec.BusinessObjects.Interfaces;
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IAuthenticateUserOutputPort : ICommonOutputPort
{
    Task Handle(AuthResultDto result, CancellationToken ct = default);
    Task InvalidCredentials();
}
```

- [ ] **Step 5: Servicios (IAuthorizationService, IPasswordHasher, IAuthTokenGenerator, ISessionService)**

`Interfaces\Services\IAuthorizationService.cs`:
```csharp
namespace MedRec.Identity.BusinessObjects.Interfaces.Services;
public interface IAuthorizationService
{
    Task EnsurePermissionAsync(Guid? userId, string permissionCode, CancellationToken ct = default);
}
```

`Interfaces\Services\IPasswordHasher.cs`:
```csharp
namespace MedRec.Identity.BusinessObjects.Interfaces.Services;
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string passwordHash);
}
```

`Interfaces\Services\IAuthTokenGenerator.cs`:
```csharp
namespace MedRec.Identity.BusinessObjects.Interfaces.Services;
public interface IAuthTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(
        Guid userId, string email, IReadOnlyList<string> roles, IReadOnlyList<string> permissions);
}
```

`Interfaces\Services\ISessionService.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Services;
public interface ISessionService
{
    bool IsAuthenticated { get; }
    AuthResultDto? CurrentUser { get; }
    event Action? OnSessionChanged;
    Task LoginAsync(AuthResultDto result, CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
}
```

- [ ] **Step 6: Repositorio de consultas**

`Interfaces\Repositories\IUserQueriesRepository.cs`:
```csharp
using MedRec.Entity.POCOEntities;

namespace MedRec.Identity.BusinessObjects.Interfaces.Repositories;
public interface IUserQueriesRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct = default);
}
```

- [ ] **Step 7: Validador**

`Validators\AuthenticateUserValidator.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.Identity.BusinessObjects.Validators;
public static class AuthenticateUserValidator
{
    public static IReadOnlyList<ValidationError> Validate(AuthenticateUserDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        var emailValidation = Guard.Against(dto.Email, nameof(dto.Email))
            .NotNullOrEmpty("El email es obligatorio.")
            .InvalidEmail("El email no tiene un formato válido.");
        errors.AddRange(emailValidation.Errors);

        var passwordValidation = Guard.Against(dto.Password, nameof(dto.Password))
            .NotNullOrEmpty("La contraseña es obligatoria.");
        errors.AddRange(passwordValidation.Errors);

        return errors;
    }
}
```

- [ ] **Step 8: Compilar**

Run: `dotnet build "2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\MedRec.Identity.BusinessObjects.csproj"`
Expected: Build succeeded.

- [ ] **Step 9: Commit**

```bash
git add "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/" MedRecSolution2025.sln
git commit -m "feat(identity): scaffolding de MedRec.Identity.BusinessObjects (DTOs, ports, servicios, validador)"
```

---

### Task 6: Scaffolding del proyecto de tests `MedRec.Identity.UseCases.Tests`

**Files:**
- Create: `Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj`

- [ ] **Step 1: Scaffolding vía CLI**

Run:
```bash
dotnet new xunit -n MedRec.Identity.UseCases.Tests -o "Test/MedRec.Identity.UseCases.Tests" --no-restore
rm "Test/MedRec.Identity.UseCases.Tests/UnitTest1.cs"
dotnet sln MedRecSolution2025.sln add "Test/MedRec.Identity.UseCases.Tests/MedRec.Identity.UseCases.Tests.csproj"
dotnet add "Test/MedRec.Identity.UseCases.Tests/MedRec.Identity.UseCases.Tests.csproj" package Moq --version 4.20.72
dotnet add "Test/MedRec.Identity.UseCases.Tests/MedRec.Identity.UseCases.Tests.csproj" reference "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/MedRec.Identity.BusinessObjects.csproj"
```

Expected: el csproj queda equivalente al de `MedRec.MedicalVisit.UseCases.Tests.csproj` (mismas versiones de `Microsoft.NET.Test.Sdk`/`xunit`/`coverlet.collector`), con `<Nullable>enable</Nullable>` y `<IsPackable>false</IsPackable>`. La `ProjectReference` a `MedRec.Identity.UseCases` se agrega en el Task 8 (ese proyecto todavía no existe).

- [ ] **Step 2: Commit**

```bash
git add "Test/MedRec.Identity.UseCases.Tests/" MedRecSolution2025.sln
git commit -m "test(identity): scaffolding del proyecto MedRec.Identity.UseCases.Tests"
```

---

### Task 7: TDD — `AuthorizationService.EnsurePermissionAsync`

**Files:**
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.UseCases\MedRec.Identity.UseCases.csproj`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\AuthorizationService.cs`
- Test: `Test\MedRec.Identity.UseCases.Tests\AuthorizationServiceTests.cs`

**Interfaces:**
- Consumes: `IUserQueriesRepository.GetPermissionCodesAsync` (Task 5).
- Produces: `AuthorizationService : IAuthorizationService` — consumida directamente (sin proxy) por interactores de todas las features en un plan futuro.

- [ ] **Step 1: Scaffolding del proyecto `MedRec.Identity.UseCases`**

Run:
```bash
dotnet new classlib -n MedRec.Identity.UseCases -o "2.ApplicationBusinessObjects/MedRec.Identity.UseCases" --no-restore
rm "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/Class1.cs"
dotnet sln MedRecSolution2025.sln add "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/MedRec.Identity.UseCases.csproj"
dotnet add "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/MedRec.Identity.UseCases.csproj" package Microsoft.Extensions.DependencyInjection.Abstractions --version 9.0.9
dotnet add "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/MedRec.Identity.UseCases.csproj" reference "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/MedRec.Identity.BusinessObjects.csproj"
dotnet add "Test/MedRec.Identity.UseCases.Tests/MedRec.Identity.UseCases.Tests.csproj" reference "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/MedRec.Identity.UseCases.csproj"
```

- [ ] **Step 2: Escribir el test que falla**

`Test\MedRec.Identity.UseCases.Tests\AuthorizationServiceTests.cs`:
```csharp
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.UseCases.Implementations;
using MedRec.Shared.Exceptions;
using Moq;

namespace MedRec.Identity.UseCases.Tests;

public class AuthorizationServiceTests
{
    [Fact]
    public async Task EnsurePermissionAsync_ShouldThrowForbidden_WhenUserIdIsNull()
    {
        var repoMock = new Mock<IUserQueriesRepository>();
        var service = new AuthorizationService(repoMock.Object);

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.EnsurePermissionAsync(null, "patients.view", CancellationToken.None));
    }

    [Fact]
    public async Task EnsurePermissionAsync_ShouldThrowForbidden_WhenPermissionNotGranted()
    {
        var userId = Guid.NewGuid();
        var repoMock = new Mock<IUserQueriesRepository>();
        repoMock.Setup(r => r.GetPermissionCodesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "patients.view" });

        var service = new AuthorizationService(repoMock.Object);

        await Assert.ThrowsAsync<BusinessException>(() =>
            service.EnsurePermissionAsync(userId, "patients.delete", CancellationToken.None));
    }

    [Fact]
    public async Task EnsurePermissionAsync_ShouldNotThrow_WhenPermissionGranted()
    {
        var userId = Guid.NewGuid();
        var repoMock = new Mock<IUserQueriesRepository>();
        repoMock.Setup(r => r.GetPermissionCodesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "patients.view" });

        var service = new AuthorizationService(repoMock.Object);

        await service.EnsurePermissionAsync(userId, "patients.view", CancellationToken.None);
        // Sin excepción = éxito.
    }
}
```

- [ ] **Step 3: Correr los tests y verificar que fallan (no compila: `AuthorizationService` no existe)**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: FAIL (error de compilación: no existe `MedRec.Identity.UseCases.Implementations.AuthorizationService`).

- [ ] **Step 4: Implementar `AuthorizationService`**

```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Shared.Exceptions;

namespace MedRec.Identity.UseCases.Implementations;

public class AuthorizationService(IUserQueriesRepository userQueriesRepository) : IAuthorizationService
{
    public async Task EnsurePermissionAsync(Guid? userId, string permissionCode, CancellationToken ct = default)
    {
        if (userId is null)
            throw new BusinessException(new ErrorInfo("No hay una sesión activa.", ErrorCode.Forbidden, null, 403));

        var permissions = await userQueriesRepository.GetPermissionCodesAsync(userId.Value, ct);
        if (!permissions.Contains(permissionCode))
            throw new BusinessException(new ErrorInfo(
                "No tiene permiso para realizar esta acción.", ErrorCode.Forbidden, new { permissionCode }, 403));
    }
}
```

- [ ] **Step 5: Correr los tests y verificar que pasan**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: PASS (3 tests).

- [ ] **Step 6: Commit**

```bash
git add "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/" "Test/MedRec.Identity.UseCases.Tests/" MedRecSolution2025.sln
git commit -m "feat(identity): implementar AuthorizationService.EnsurePermissionAsync (TDD)"
```

---

### Task 8: TDD — `AuthenticateUserInteractor`

**Files:**
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\AuthenticateUserInteractor.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.UseCases\DependencyContainer.cs`
- Test: `Test\MedRec.Identity.UseCases.Tests\AuthenticateUserInteractorTests.cs`

**Interfaces:**
- Consumes: `IAuthenticateUserOutputPort`, `IUserQueriesRepository`, `IPasswordHasher`, `IAuthTokenGenerator`, `IModelValidatorHub<AuthenticateUserDto>` (todas de Task 5).
- Produces: `AuthenticateUserInteractor : IAuthenticateUserInputPort` — registrado automáticamente por `AddUseCaseExceptionDecorators`.

- [ ] **Step 1: Escribir los tests que fallan**

`Test\MedRec.Identity.UseCases.Tests\AuthenticateUserInteractorTests.cs`:
```csharp
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.UseCases.Implementations;
using MedRec.Validator.Interfaces;
using MedRec.Validator.ValueObjects;
using Moq;

namespace MedRec.Identity.UseCases.Tests;

public class AuthenticateUserInteractorTests
{
    private static (
        Mock<IAuthenticateUserOutputPort> presenter,
        Mock<IUserQueriesRepository> userRepo,
        Mock<IPasswordHasher> hasher,
        Mock<IAuthTokenGenerator> tokenGenerator,
        Mock<IModelValidatorHub<AuthenticateUserDto>> validator) CreateMocks()
    {
        return (
            new Mock<IAuthenticateUserOutputPort>(),
            new Mock<IUserQueriesRepository>(),
            new Mock<IPasswordHasher>(),
            new Mock<IAuthTokenGenerator>(),
            new Mock<IModelValidatorHub<AuthenticateUserDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<AuthenticateUserDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<AuthenticateUserDto>(), It.IsAny<Func<AuthenticateUserDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationErrors_WhenDtoIsInvalid()
    {
        var dto = new AuthenticateUserDto("", "");
        var (presenter, userRepo, hasher, tokenGenerator, validator) = CreateMocks();

        validator.Setup(v => v.Validate(dto, It.IsAny<Func<AuthenticateUserDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(false);
        validator.SetupGet(v => v.Errors).Returns(new[] { new ValidationError("Email", "El email es obligatorio.") });

        var interactor = new AuthenticateUserInteractor(presenter.Object, userRepo.Object, hasher.Object, tokenGenerator.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Once);
        presenter.Verify(p => p.InvalidCredentials(), Times.Never);
        presenter.Verify(p => p.Handle(It.IsAny<AuthResultDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnInvalidCredentials_WhenUserNotFound()
    {
        var dto = new AuthenticateUserDto("nadie@medrec.local", "Cambiar123!");
        var (presenter, userRepo, hasher, tokenGenerator, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        userRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var interactor = new AuthenticateUserInteractor(presenter.Object, userRepo.Object, hasher.Object, tokenGenerator.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.InvalidCredentials(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnInvalidCredentials_WhenPasswordDoesNotMatch()
    {
        var dto = new AuthenticateUserDto("admin@medrec.local", "MalaClave");
        var (presenter, userRepo, hasher, tokenGenerator, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        var user = new User { Id = Guid.NewGuid(), Email = dto.Email, PasswordHash = "hash", IsActive = true };
        userRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(false);

        var interactor = new AuthenticateUserInteractor(presenter.Object, userRepo.Object, hasher.Object, tokenGenerator.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.InvalidCredentials(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnInvalidCredentials_WhenUserIsInactive()
    {
        var dto = new AuthenticateUserDto("admin@medrec.local", "Cambiar123!");
        var (presenter, userRepo, hasher, tokenGenerator, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        var user = new User { Id = Guid.NewGuid(), Email = dto.Email, PasswordHash = "hash", IsActive = false };
        userRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);

        var interactor = new AuthenticateUserInteractor(presenter.Object, userRepo.Object, hasher.Object, tokenGenerator.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.InvalidCredentials(), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAuthResult_WhenCredentialsAreValid()
    {
        var dto = new AuthenticateUserDto("admin@medrec.local", "Cambiar123!");
        var (presenter, userRepo, hasher, tokenGenerator, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        var user = new User { Id = Guid.NewGuid(), Email = dto.Email, PasswordHash = "hash", FullName = "Admin", IsActive = true, DoctorId = null };
        userRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);
        userRepo.Setup(r => r.GetRoleNamesAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new List<string> { "Administrador" });
        userRepo.Setup(r => r.GetPermissionCodesAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new List<string> { "patients.view" });
        tokenGenerator.Setup(t => t.GenerateToken(user.Id, user.Email, It.IsAny<IReadOnlyList<string>>(), It.IsAny<IReadOnlyList<string>>()))
            .Returns(("token123", DateTime.UtcNow.AddHours(4)));

        var interactor = new AuthenticateUserInteractor(presenter.Object, userRepo.Object, hasher.Object, tokenGenerator.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.Handle(
            It.Is<AuthResultDto>(r => r.UserId == user.Id && r.Token == "token123" && r.Roles.Contains("Administrador")),
            It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.InvalidCredentials(), Times.Never);
    }
}
```

- [ ] **Step 2: Correr los tests y verificar que fallan**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: FAIL (no compila: `AuthenticateUserInteractor` no existe).

- [ ] **Step 3: Implementar `AuthenticateUserInteractor`**

```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Identity.UseCases.Implementations;

internal class AuthenticateUserInteractor(
    IAuthenticateUserOutputPort presenter,
    IUserQueriesRepository userQueriesRepository,
    IPasswordHasher passwordHasher,
    IAuthTokenGenerator tokenGenerator,
    IModelValidatorHub<AuthenticateUserDto> validatorHub) : IAuthenticateUserInputPort
{
    public async Task HandleAsync(AuthenticateUserDto dto, CancellationToken ct = default)
    {
        var isValid = await validatorHub.Validate(dto, AuthenticateUserValidator.Validate);
        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var user = await userQueriesRepository.GetByEmailAsync(dto.Email, ct);
        if (user is null || !user.IsActive || !passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            await presenter.InvalidCredentials();
            return;
        }

        var roles = await userQueriesRepository.GetRoleNamesAsync(user.Id, ct);
        var permissions = await userQueriesRepository.GetPermissionCodesAsync(user.Id, ct);
        var (token, expiresAtUtc) = tokenGenerator.GenerateToken(user.Id, user.Email, roles, permissions);

        await presenter.Handle(
            new AuthResultDto(user.Id, user.Email, user.FullName, user.DoctorId, roles, permissions, token, expiresAtUtc),
            ct);
    }
}
```

- [ ] **Step 4: Correr los tests y verificar que pasan**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: PASS (8 tests en total: 3 de `AuthorizationServiceTests` + 5 de `AuthenticateUserInteractorTests`).

- [ ] **Step 5: `DependencyContainer.cs` de la capa UseCases**

```csharp
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.UseCases.Implementations;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddIdentityUseCasesServicesWithProxy(
        this IServiceCollection services,
        bool rethrow = false)
    {
        services.AddScoped<IAuthorizationService, AuthorizationService>();

        return services.AddUseCaseExceptionDecorators(
            [
                typeof(IAuthenticateUserInputPort).Assembly,
                typeof(AuthenticateUserInteractor).Assembly
            ], rethrow);
    }
}
```

- [ ] **Step 6: Compilar todo y commit**

Run: `dotnet build "2.ApplicationBusinessObjects\MedRec.Identity.UseCases\MedRec.Identity.UseCases.csproj"`
Expected: Build succeeded.

```bash
git add "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/" "Test/MedRec.Identity.UseCases.Tests/"
git commit -m "feat(identity): implementar AuthenticateUserInteractor y su DI (TDD)"
```

---

### Task 9: Proyecto `MedRec.Identity.Repositories` (capa 3)

**Files:**
- Create: `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\MedRec.Identity.Repositories.csproj`
- Create: `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\Interfaces\IUserQueriesDataContext.cs`
- Create: `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\Implementations\UserQueriesRepository.cs`
- Create: `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\DependencyContainer.cs`

**Interfaces:**
- Consumes: `IUserQueriesRepository` (Task 5).
- Produces: `IUserQueriesDataContext` — implementado por `MedRec.Identity.DataContext.MySql` en Task 11.

- [ ] **Step 1: Scaffolding**

Run:
```bash
dotnet new classlib -n MedRec.Identity.Repositories -o "3.InterfaceAdapters/Repositories/MedRec.Identity.Repositories" --no-restore
rm "3.InterfaceAdapters/Repositories/MedRec.Identity.Repositories/Class1.cs"
dotnet sln MedRecSolution2025.sln add "3.InterfaceAdapters/Repositories/MedRec.Identity.Repositories/MedRec.Identity.Repositories.csproj"
dotnet add "3.InterfaceAdapters/Repositories/MedRec.Identity.Repositories/MedRec.Identity.Repositories.csproj" reference "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/MedRec.Identity.BusinessObjects.csproj"
```

- [ ] **Step 2: `IUserQueriesDataContext`**

```csharp
using MedRec.Entity.POCOEntities;

namespace MedRec.Identity.Repositories.Interfaces;
public interface IUserQueriesDataContext
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct = default);
}
```

- [ ] **Step 3: `UserQueriesRepository` (delega en el DataContext)**

```csharp
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.Repositories.Interfaces;

namespace MedRec.Identity.Repositories.Implementations;
internal class UserQueriesRepository(IUserQueriesDataContext dataContext) : IUserQueriesRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        dataContext.GetByEmailAsync(email, ct);

    public Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct = default) =>
        dataContext.GetRoleNamesAsync(userId, ct);

    public Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct = default) =>
        dataContext.GetPermissionCodesAsync(userId, ct);
}
```

- [ ] **Step 4: DI**

```csharp
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.Repositories.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddIdentityRepositoriesServices(this IServiceCollection services)
    {
        services.AddScoped<IUserQueriesRepository, UserQueriesRepository>();
        return services;
    }
}
```

- [ ] **Step 5: Compilar y commit**

Run: `dotnet build "3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\MedRec.Identity.Repositories.csproj"`
Expected: Build succeeded.

```bash
git add "3.InterfaceAdapters/Repositories/MedRec.Identity.Repositories/" MedRecSolution2025.sln
git commit -m "feat(identity): agregar MedRec.Identity.Repositories (capa 3)"
```

---

### Task 10: DbSets, EF Configurations y auditoría automática en `MedRecContext`

**Files:**
- Modify: `4.Framework&Drivers\MedRec.DataContext.MySql\DataContext\MedRecContext.cs`
- Create: `4.Framework&Drivers\MedRec.DataContext.MySql\DataContext\NullCurrentUserContext.cs`
- Create: `4.Framework&Drivers\MedRec.DataContext.MySql\Configurations\UserConfiguration.cs`
- Create: `4.Framework&Drivers\MedRec.DataContext.MySql\Configurations\RoleConfiguration.cs`
- Create: `4.Framework&Drivers\MedRec.DataContext.MySql\Configurations\PermissionConfiguration.cs`
- Create: `4.Framework&Drivers\MedRec.DataContext.MySql\Configurations\UserRoleConfiguration.cs`
- Create: `4.Framework&Drivers\MedRec.DataContext.MySql\Configurations\RolePermissionConfiguration.cs`
- Modify: `4.Framework&Drivers\MedRec.DataContext.MySql\DataContext\DesignTimeDbContextFactory.cs`

**Interfaces:**
- Consumes: `ICurrentUserContext`, `IAuditableEntity` (Task 1).
- Produces: `MedRecContext.Users/Roles/Permissions/UserRoles/RolePermissions` DbSets, stamping automático de auditoría en cada `SaveChangesAsync`.

- [ ] **Step 1: `NullCurrentUserContext` (para el factory de diseño de `dotnet ef`)**

```csharp
using MedRec.Entity.Interfaces;

namespace MedRec.DataContext.MySql.DataContext;

internal class NullCurrentUserContext : ICurrentUserContext
{
    public Guid? UserId => null;
}
```

- [ ] **Step 2: Configurations de las 5 entidades nuevas**

`Configurations\UserConfiguration.cs`:
```csharp
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

        builder.Property(u => u.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(500);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.IsActive).HasDefaultValue(true);
        builder.Property(u => u.IsDeleted).HasDefaultValue(false);

        builder.Property(u => u.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
    }
}
```

`Configurations\RoleConfiguration.cs`:
```csharp
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        builder.Property(r => r.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(r => r.Name).IsUnique();
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.Property(r => r.IsDeleted).HasDefaultValue(false);

        builder.Property(r => r.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
    }
}
```

`Configurations\PermissionConfiguration.cs`:
```csharp
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Code).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Description).HasMaxLength(250);
        builder.Property(p => p.IsDeleted).HasDefaultValue(false);

        builder.Property(p => p.RowVersion).IsConcurrencyToken().ValueGeneratedOnAddOrUpdate();
    }
}
```

`Configurations\UserRoleConfiguration.cs`:
```csharp
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder.HasOne<User>().WithMany().HasForeignKey(ur => ur.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Role>().WithMany().HasForeignKey(ur => ur.RoleId).OnDelete(DeleteBehavior.Cascade);
    }
}
```

`Configurations\RolePermissionConfiguration.cs`:
```csharp
using MedRec.Entity.POCOEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedRec.DataContext.MySql.Configurations;
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

        builder.HasOne<Role>().WithMany().HasForeignKey(rp => rp.RoleId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Permission>().WithMany().HasForeignKey(rp => rp.PermissionId).OnDelete(DeleteBehavior.Cascade);
    }
}
```

- [ ] **Step 3: Modificar `MedRecContext.cs`**

Agregar `using MedRec.Entity.Interfaces;` al inicio. Reemplazar los dos constructores existentes por:

```csharp
    // Constructor principal para producción (inyectado por DI)
    public MedRecContext(IOptions<DBOptionsMySql> dbOptions, ICurrentUserContext currentUserContext)
        : base()
    {
        count += 1;
        _dbOptions = dbOptions ?? throw new ArgumentNullException(nameof(dbOptions));
        _currentUserContext = currentUserContext ?? throw new ArgumentNullException(nameof(currentUserContext));
        if (string.IsNullOrEmpty(_dbOptions.Value.ConnectionString))
            throw new ArgumentException("Connection string is required.", nameof(dbOptions));
    }

    // Constructor para tiempo de diseño (EF Core tools)
    internal MedRecContext(DbContextOptions<MedRecContext> options)
        : this(options, new NullCurrentUserContext())
    {
    }

    internal MedRecContext(DbContextOptions<MedRecContext> options, ICurrentUserContext currentUserContext)
        : base(options)
    {
        count += 1;
        _currentUserContext = currentUserContext;
    }
```

Agregar el campo (junto a `_dbOptions`):
```csharp
    private readonly ICurrentUserContext _currentUserContext;
```

Agregar los 5 DbSets nuevos, junto a los existentes:
```csharp
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
```

Agregar el override de `SaveChangesAsync` y el método de stamping (antes de `Dispose`):
```csharp
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInfo()
    {
        var userId = _currentUserContext.UserId;
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
                entry.Entity.UpdatedBy = userId;
            }
        }
    }
```

- [ ] **Step 4: Actualizar `DesignTimeDbContextFactory.cs`**

El `return new MedRecContext(optionsBuilder.Options);` sigue funcionando sin cambios (usa el ctor de un solo parámetro que ahora delega a `NullCurrentUserContext`). No requiere edición.

- [ ] **Step 5: Compilar**

Run: `dotnet build "4.Framework&Drivers\MedRec.DataContext.MySql\MedRec.DataContext.MySql.csproj"`
Expected: Build succeeded.

- [ ] **Step 6: Commit**

```bash
git add "4.Framework&Drivers/MedRec.DataContext.MySql/"
git commit -m "feat(identity): DbSets de Identity + auditoría automática en MedRecContext.SaveChangesAsync"
```

---

### Task 11: Proyecto `MedRec.Identity.DataContext.MySql` (capa 4) — hashing, JWT, sesión, contexto de usuario, repositorio EF

**Files:**
- Create: `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\MedRec.Identity.DataContext.MySql.csproj`
- Create: `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\Services\UserQueriesDataContextMySql.cs`
- Create: `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\Services\PasswordHasherService.cs`
- Create: `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\Services\JwtAuthTokenGenerator.cs`
- Create: `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\Services\SessionService.cs`
- Create: `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\Services\CurrentUserContext.cs`
- Create: `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\DependencyContainer.cs`
- Modify: `1.EnterpriseBusinessObjects\MedRec.Shared\Security\Jwt.cs`

**Interfaces:**
- Consumes: `IUserQueriesDataContext` (Task 9), `MedRecContext` (Task 10), `ICurrentUserContext`/`IAuditableEntity` (Task 1), `ISessionService`/`IPasswordHasher`/`IAuthTokenGenerator` (Task 5).
- Produces: implementaciones concretas registradas en DI — consumidas por `MedRec.IoC` en Task 15.

- [ ] **Step 1: Scaffolding**

Run:
```bash
dotnet new classlib -n MedRec.Identity.DataContext.MySql -o "4.Framework&Drivers/MedRec.Identity.DataContext.MySql" --no-restore
rm "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/Class1.cs"
dotnet sln MedRecSolution2025.sln add "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/MedRec.Identity.DataContext.MySql.csproj"
dotnet add "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/MedRec.Identity.DataContext.MySql.csproj" package Pomelo.EntityFrameworkCore.MySql --version 9.0.0
dotnet add "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/MedRec.Identity.DataContext.MySql.csproj" package Microsoft.Extensions.Identity.Core --version 9.0.9
dotnet add "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/MedRec.Identity.DataContext.MySql.csproj" package System.IdentityModel.Tokens.Jwt --version 8.2.1
dotnet add "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/MedRec.Identity.DataContext.MySql.csproj" package Microsoft.Extensions.Options --version 9.0.9
dotnet add "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/MedRec.Identity.DataContext.MySql.csproj" reference "3.InterfaceAdapters/Repositories/MedRec.Identity.Repositories/MedRec.Identity.Repositories.csproj"
dotnet add "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/MedRec.Identity.DataContext.MySql.csproj" reference "4.Framework&Drivers/MedRec.DataContext.MySql/MedRec.DataContext.MySql.csproj"
```

- [ ] **Step 2: Agregar `ExpirationMinutes` a `Jwt.cs`**

Reemplazar `1.EnterpriseBusinessObjects\MedRec.Shared\Security\Jwt.cs`:
```csharp
namespace MedRec.Shared.Security;
public class Jwt
{
    public const string SectionKey = nameof(Jwt);
    public string Key { get; set; }
    public int ExpirationMinutes { get; set; } = 240;
}
```

(`appsettings.json` ya tiene `"Jwt": { "Key": "ENC:...", "ExpirationMinutes": 240 }`, no requiere cambios.)

- [ ] **Step 3: `UserQueriesDataContextMySql` — consultas manuales (sin navigation properties, siguiendo la convención del resto del código)**

```csharp
using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class UserQueriesDataContextMySql(MedRecContext context) : IUserQueriesDataContext
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct = default)
    {
        return await (
            from ur in context.UserRoles
            join r in context.Roles on ur.RoleId equals r.Id
            where ur.UserId == userId && !r.IsDeleted
            select r.Name
        ).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct = default)
    {
        return await (
            from ur in context.UserRoles
            join rp in context.RolePermissions on ur.RoleId equals rp.RoleId
            join p in context.Permissions on rp.PermissionId equals p.Id
            where ur.UserId == userId && !p.IsDeleted
            select p.Code
        ).Distinct().ToListAsync(ct);
    }
}
```

- [ ] **Step 4: `PasswordHasherService`**

```csharp
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    // El parámetro "user" del PasswordHasher<TUser> de ASP.NET Core Identity no se usa
    // internamente por la implementación default: solo hace falta para la firma genérica.
    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public bool Verify(string password, string passwordHash) =>
        _hasher.VerifyHashedPassword(null!, passwordHash, password) != PasswordVerificationResult.Failed;
}
```

- [ ] **Step 5: `JwtAuthTokenGenerator`**

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Shared.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class JwtAuthTokenGenerator(IOptions<Jwt> jwtOptions) : IAuthTokenGenerator
{
    public (string Token, DateTime ExpiresAtUtc) GenerateToken(
        Guid userId, string email, IReadOnlyList<string> roles, IReadOnlyList<string> permissions)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtOptions.Value.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
        };
        claims.AddRange(roles.Select(r => new Claim("role", r)));
        claims.AddRange(permissions.Select(p => new Claim("permission", p)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Value.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(claims: claims, expires: expiresAtUtc, signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
```

- [ ] **Step 6: `SessionService` (en memoria, sin persistencia entre aperturas)**

```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Services;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class SessionService : ISessionService
{
    public bool IsAuthenticated => CurrentUser is not null;
    public AuthResultDto? CurrentUser { get; private set; }
    public event Action? OnSessionChanged;

    public Task LoginAsync(AuthResultDto result, CancellationToken ct = default)
    {
        CurrentUser = result;
        OnSessionChanged?.Invoke();
        return Task.CompletedTask;
    }

    public Task LogoutAsync(CancellationToken ct = default)
    {
        CurrentUser = null;
        OnSessionChanged?.Invoke();
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 7: `CurrentUserContext`**

```csharp
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Interfaces.Services;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class CurrentUserContext(ISessionService sessionService) : ICurrentUserContext
{
    public Guid? UserId => sessionService.CurrentUser?.UserId;
}
```

- [ ] **Step 8: `DependencyContainer.cs`**

`SessionService` y `CurrentUserContext` van **Singleton**: deben sobrevivir durante toda la vida del proceso (la sesión no depende del scope de un componente Blazor puntual).

```csharp
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.DataContext.MySql.Services;
using MedRec.Identity.Repositories.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddIdentityDataContextMySqlServices(this IServiceCollection services)
    {
        services.AddScoped<IUserQueriesDataContext, UserQueriesDataContextMySql>();
        services.AddSingleton<IPasswordHasher, PasswordHasherService>();
        services.AddSingleton<IAuthTokenGenerator, JwtAuthTokenGenerator>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<ICurrentUserContext, CurrentUserContext>();

        return services;
    }
}
```

- [ ] **Step 9: Compilar y commit**

Run: `dotnet build "4.Framework&Drivers\MedRec.Identity.DataContext.MySql\MedRec.Identity.DataContext.MySql.csproj"`
Expected: Build succeeded.

```bash
git add "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/" "1.EnterpriseBusinessObjects/MedRec.Shared/Security/Jwt.cs" MedRecSolution2025.sln
git commit -m "feat(identity): implementar hashing, JWT, sesión y ICurrentUserContext (capa 4)"
```

---

### Task 12: Proyecto `MedRec.Identity.Presenters`

**Files:**
- Create: `3.InterfaceAdapters\MedRec.Identity.Presenters\MedRec.Identity.Presenters.csproj`
- Create: `3.InterfaceAdapters\MedRec.Identity.Presenters\Implementations\AuthenticateUserPresenter.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.Presenters\DependencyContainer.cs`

- [ ] **Step 1: Scaffolding**

Run:
```bash
dotnet new classlib -n MedRec.Identity.Presenters -o "3.InterfaceAdapters/MedRec.Identity.Presenters" --no-restore
rm "3.InterfaceAdapters/MedRec.Identity.Presenters/Class1.cs"
dotnet sln MedRecSolution2025.sln add "3.InterfaceAdapters/MedRec.Identity.Presenters/MedRec.Identity.Presenters.csproj"
dotnet add "3.InterfaceAdapters/MedRec.Identity.Presenters/MedRec.Identity.Presenters.csproj" reference "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/MedRec.Identity.BusinessObjects.csproj"
```

- [ ] **Step 2: `AuthenticateUserPresenter`**

```csharp
using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;

namespace MedRec.Identity.Presenters.Implementations;
internal class AuthenticateUserPresenter : BaseOutputPort<AuthResultDto>, IAuthenticateUserOutputPort
{
    public Task Handle(AuthResultDto result, CancellationToken ct = default)
    {
        Result = OperationResult<AuthResultDto>.Ok(result);
        return Task.CompletedTask;
    }

    public Task InvalidCredentials()
    {
        Result = OperationResult<AuthResultDto>.Fail(
            new ErrorInfo("Email o contraseña incorrectos.", ErrorCode.Forbidden, null, 401));
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 3: DI**

```csharp
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.Presenters.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddIdentityPresentersServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticateUserOutputPort, AuthenticateUserPresenter>();
        return services;
    }
}
```

- [ ] **Step 4: Compilar y commit**

Run: `dotnet build "3.InterfaceAdapters\MedRec.Identity.Presenters\MedRec.Identity.Presenters.csproj"`
Expected: Build succeeded.

```bash
git add "3.InterfaceAdapters/MedRec.Identity.Presenters/" MedRecSolution2025.sln
git commit -m "feat(identity): agregar AuthenticateUserPresenter"
```

---

### Task 13: Proyectos `MedRec.Identity.ViewModels` y `MedRec.Identity.Views` — pantalla de login

**Files:**
- Create: `3.InterfaceAdapters\MedRec.Identity.ViewModels\MedRec.Identity.ViewModels.csproj`
- Create: `3.InterfaceAdapters\MedRec.Identity.ViewModels\Models\LoginModel.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.ViewModels\VM\LoginVM.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.Views\MedRec.Identity.Views.csproj`
- Create: `3.InterfaceAdapters\MedRec.Identity.Views\Pages\LoginPage.razor`
- Create: `3.InterfaceAdapters\MedRec.Identity.Views\_Imports.razor`

**Interfaces:**
- Consumes: `IAuthenticateUserInputPort`/`OutputPort` (Task 5), `ISessionService` (Task 5).

- [ ] **Step 1: Scaffolding de ViewModels**

Run:
```bash
dotnet new classlib -n MedRec.Identity.ViewModels -o "3.InterfaceAdapters/MedRec.Identity.ViewModels" --no-restore
rm "3.InterfaceAdapters/MedRec.Identity.ViewModels/Class1.cs"
dotnet sln MedRecSolution2025.sln add "3.InterfaceAdapters/MedRec.Identity.ViewModels/MedRec.Identity.ViewModels.csproj"
dotnet add "3.InterfaceAdapters/MedRec.Identity.ViewModels/MedRec.Identity.ViewModels.csproj" reference "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/MedRec.Identity.BusinessObjects.csproj"
```

- [ ] **Step 2: `LoginModel`**

```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.ViewModels.Models;
public class LoginModel
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public static explicit operator AuthenticateUserDto(LoginModel model) =>
        new(model.Email, model.Password);
}
```

- [ ] **Step 3: `LoginVM`**

```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.ViewModels.Models;

namespace MedRec.Identity.ViewModels.VM;
public class LoginVM(
    IAuthenticateUserInputPort interactor,
    IAuthenticateUserOutputPort presenter,
    ISessionService sessionService)
{
    public LoginModel Model { get; set; } = new();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;

    public async Task LoginAsync(CancellationToken ct = default)
    {
        IsProcessing = true;
        try
        {
            InformationMessage = string.Empty;
            await interactor.HandleAsync((AuthenticateUserDto)Model, ct);
            var result = presenter.Result;

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join(" ", result.ValidationErrors.Select(e => e.ErrorMessage));
            }
            else if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "Email o contraseña incorrectos.";
            }
            else
            {
                await sessionService.LoginAsync(result.Value!, ct);
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
```

- [ ] **Step 4: Scaffolding de Views (Razor)**

Run:
```bash
dotnet new razorclasslib -n MedRec.Identity.Views -o "3.InterfaceAdapters/MedRec.Identity.Views" --no-restore
rm "3.InterfaceAdapters/MedRec.Identity.Views/Component1.razor"
rm -rf "3.InterfaceAdapters/MedRec.Identity.Views/wwwroot"
dotnet sln MedRecSolution2025.sln add "3.InterfaceAdapters/MedRec.Identity.Views/MedRec.Identity.Views.csproj"
dotnet add "3.InterfaceAdapters/MedRec.Identity.Views/MedRec.Identity.Views.csproj" reference "3.InterfaceAdapters/MedRec.Identity.ViewModels/MedRec.Identity.ViewModels.csproj"
```

- [ ] **Step 5: `_Imports.razor`**

```razor
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Web
@using MedRec.Identity.ViewModels.Models
@using MedRec.Identity.ViewModels.VM
```

- [ ] **Step 6: `LoginPage.razor`**

Nota de alcance: no se reutiliza el componente genérico `ModelValidator` de `MedRec.CommonComponents.Views` (usado en `CreatePatientComponent`) para mantener el login autocontenido; la validación se muestra directamente vía `VM.InformationMessage`.

```razor
@page "/login"
@inject LoginVM VM

<div class="login-container">
    <EditForm Model="VM.Model" OnValidSubmit="HandleLogin" autocomplete="off">
        <h2>MedRec — Iniciar sesión</h2>

        <div class="login-row">
            <label for="email">Email</label>
            <InputText id="email" @bind-Value="VM.Model.Email" autocomplete="username" />
        </div>

        <div class="login-row">
            <label for="password">Contraseña</label>
            <InputText id="password" type="password" @bind-Value="VM.Model.Password" autocomplete="current-password" />
        </div>

        @if (!string.IsNullOrEmpty(VM.InformationMessage))
        {
            <p class="login-error">@VM.InformationMessage</p>
        }

        <button type="submit" disabled="@VM.IsProcessing">
            @(VM.IsProcessing ? "Ingresando..." : "Ingresar")
        </button>
    </EditForm>
</div>

@code {
    private async Task HandleLogin()
    {
        await VM.LoginAsync();
        if (VM.InformationMessage == string.Empty)
        {
            StateHasChanged();
        }
    }
}
```

- [ ] **Step 7: Compilar y commit**

Run: `dotnet build "3.InterfaceAdapters\MedRec.Identity.ViewModels\MedRec.Identity.ViewModels.csproj" && dotnet build "3.InterfaceAdapters\MedRec.Identity.Views\MedRec.Identity.Views.csproj"`
Expected: Build succeeded en ambos.

```bash
git add "3.InterfaceAdapters/MedRec.Identity.ViewModels/" "3.InterfaceAdapters/MedRec.Identity.Views/" MedRecSolution2025.sln
git commit -m "feat(identity): agregar LoginVM y LoginPage"
```

---

### Task 14: `AppShell.razor` — gatear la app detrás del login

**Files:**
- Create: `4.Framework&Drivers\MedRec.WPF.UI\AppShell.razor`
- Modify: `4.Framework&Drivers\MedRec.WPF.UI\MainWindow.xaml`
- Modify: `4.Framework&Drivers\MedRec.WPF.UI\Shared\NavMenu.razor`

**Interfaces:**
- Consumes: `ISessionService` (Task 5/11).

- [ ] **Step 1: `AppShell.razor`**

```razor
@using MedRec.Identity.BusinessObjects.Interfaces.Services
@using MedRec.Identity.Views.Pages
@implements IDisposable
@inject ISessionService SessionService

@if (SessionService.IsAuthenticated)
{
    <Main />
}
else
{
    <LoginPage />
}

@code {
    protected override void OnInitialized()
    {
        SessionService.OnSessionChanged += HandleSessionChanged;
    }

    private void HandleSessionChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        SessionService.OnSessionChanged -= HandleSessionChanged;
    }
}
```

- [ ] **Step 2: Registrar `AppShell` como root component en `MainWindow.xaml`**

En `4.Framework&Drivers\MedRec.WPF.UI\MainWindow.xaml`, cambiar:

```xml
                <b:RootComponent
                    ComponentType="{x:Type local:Main}"
                    Selector="#app"/>
```

por:

```xml
                <b:RootComponent
                    ComponentType="{x:Type local:AppShell}"
                    Selector="#app"/>
```

- [ ] **Step 3: Extender `NavMenu.razor` con usuario logueado y logout**

En `4.Framework&Drivers\MedRec.WPF.UI\Shared\NavMenu.razor`, agregar al inicio del archivo:

```razor
@using MedRec.Identity.BusinessObjects.Interfaces.Services
@inject ISessionService SessionService
```

Y agregar, dentro de `<div class="sidebar @NavClass">`, antes de `<ul class="nav-links">`, un bloque de usuario:

```razor
    @if (SessionService.CurrentUser is not null)
    {
        <div class="nav-user">
            @if (!_isCollapsed)
            {
                <span>@SessionService.CurrentUser.FullName</span>
            }
            <button class="logout-btn" @onclick="Logout" title="Cerrar sesión">
                <i class="bi bi-box-arrow-right"></i>
            </button>
        </div>
    }
```

Y en el bloque `@code { }` existente, agregar el método:

```razor
    private async Task Logout()
    {
        await SessionService.LogoutAsync();
    }
```

- [ ] **Step 4: Commit**

```bash
git add "4.Framework&Drivers/MedRec.WPF.UI/AppShell.razor" "4.Framework&Drivers/MedRec.WPF.UI/MainWindow.xaml" "4.Framework&Drivers/MedRec.WPF.UI/Shared/NavMenu.razor"
git commit -m "feat(identity): gatear la app con AppShell (login obligatorio) y agregar logout al NavMenu"
```

(La compilación de este proyecto se valida recién en el Task 15, cuando el DI esté completo — el WPF host no compila de forma aislada sin las referencias agregadas ahí.)

---

### Task 15: Composition root — `MedRec.IoC`, referencias del host WPF, migración y seed

**Files:**
- Modify: `4.Framework&Drivers\MedRec.IoC\DependencyContainer.cs`
- Modify: `4.Framework&Drivers\MedRec.IoC\MedRec.IoC.csproj`
- Modify: `4.Framework&Drivers\MedRec.WPF.UI\MedRec.WPF.UI.csproj`
- Create: (migración EF Core generada por la herramienta, ver Step 5)

**Interfaces:**
- Consumes: todos los `AddIdentityXxxServices()` de las Tasks 8-12.

- [ ] **Step 1: Agregar referencias de proyecto al `MedRec.IoC.csproj`**

Run:
```bash
dotnet add "4.Framework&Drivers/MedRec.IoC/MedRec.IoC.csproj" reference "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/MedRec.Identity.UseCases.csproj"
dotnet add "4.Framework&Drivers/MedRec.IoC/MedRec.IoC.csproj" reference "3.InterfaceAdapters/Repositories/MedRec.Identity.Repositories/MedRec.Identity.Repositories.csproj"
dotnet add "4.Framework&Drivers/MedRec.IoC/MedRec.IoC.csproj" reference "3.InterfaceAdapters/MedRec.Identity.Presenters/MedRec.Identity.Presenters.csproj"
dotnet add "4.Framework&Drivers/MedRec.IoC/MedRec.IoC.csproj" reference "3.InterfaceAdapters/MedRec.Identity.ViewModels/MedRec.Identity.ViewModels.csproj"
dotnet add "4.Framework&Drivers/MedRec.IoC/MedRec.IoC.csproj" reference "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/MedRec.Identity.DataContext.MySql.csproj"
```

- [ ] **Step 2: Cablear en `AddAppServices()`**

En `4.Framework&Drivers\MedRec.IoC\DependencyContainer.cs`, agregar al inicio del `using`:

```csharp
using MedRec.Identity.ViewModels.VM;
```

Y dentro de `AddAppServices()`, antes de `services.AddValidatorServices();`:

```csharp
        services.AddIdentityDataContextMySqlServices()
                .AddIdentityRepositoriesServices()
                .AddIdentityUseCasesServicesWithProxy()
                .AddIdentityPresentersServices();

        services.AddTransient<LoginVM>();
```

- [ ] **Step 3: Referenciar `MedRec.Identity.ViewModels` y `MedRec.Identity.Views` desde el host WPF**

Run:
```bash
dotnet add "4.Framework&Drivers/MedRec.WPF.UI/MedRec.WPF.UI.csproj" reference "3.InterfaceAdapters/MedRec.Identity.ViewModels/MedRec.Identity.ViewModels.csproj"
dotnet add "4.Framework&Drivers/MedRec.WPF.UI/MedRec.WPF.UI.csproj" reference "3.InterfaceAdapters/MedRec.Identity.Views/MedRec.Identity.Views.csproj"
```

Y en `4.Framework&Drivers\MedRec.WPF.UI\_Imports.razor`, agregar:
```razor
@using MedRec.Identity.ViewModels.VM
@using MedRec.Identity.Views.Pages
```

- [ ] **Step 4: Compilar toda la solución**

Run: `dotnet build MedRecSolution2025.sln`
Expected: Build succeeded, 0 errores (pueden aparecer warnings, no bloquean).

- [ ] **Step 5: Generar la migración**

Run:
```bash
dotnet ef migrations add AddIdentityAndAudit --project "4.Framework&Drivers/MedRec.DataContext.MySql" --startup-project "4.Framework&Drivers/MedRec.WPF.UI"
```

Expected: se genera `4.Framework&Drivers\MedRec.DataContext.MySql\Migrations\<timestamp>_AddIdentityAndAudit.cs` con `CreateTable` para `Users`/`Roles`/`Permissions`/`UserRoles`/`RolePermissions` y `AddColumn` para `CreatedBy`/`CreatedAt`/`UpdatedBy`/`UpdatedAt` en las 16 entidades (para las 5 tablas no confirmadas en el dump — `laboratoryresulttypes`, `medicalconditions`, `medicalconditiontypes`, `patientlaboratoryresults`, `patientmedicalconditions` — el `AddColumn` se genera igual porque el modelo las incluye; si esas tablas no existen todavía en la base real, aplicar la migración fallará en esas líneas puntuales. Verificar con `SHOW TABLES;` contra `medrecdb` antes de aplicar en producción y ajustar si hace falta).

- [ ] **Step 6: Editar la migración generada — agregar el DROP de tablas huérfanas al principio de `Up()`**

Abrir el archivo de migración generado y agregar, como primeras líneas del método `Up(MigrationBuilder migrationBuilder)`:

```csharp
            migrationBuilder.Sql("DROP TABLE IF EXISTS `userroles`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `rolepermissions`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `users`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `roles`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `permissions`;");
            migrationBuilder.Sql("DROP TABLE IF EXISTS `professionals`;");

```

(Antes de cualquier `migrationBuilder.CreateTable(...)` — son las tablas huérfanas de `D:\$Root\MedRecSolution\MedRec`, confirmadas sin uso ni historial en este repo, ver `docs/superpowers/specs/2026-08-05-identity-design.md`.)

- [ ] **Step 7: Generar el hash de la contraseña inicial del admin**

Como `PasswordHasher<TUser>` usa salt aleatorio, el hash se genera corriendo el hasher real una vez. Agregar temporalmente este test, correrlo, copiar el string impreso, y borrar el test:

```csharp
// Test temporal en Test/MedRec.Identity.UseCases.Tests/_TempHashGenerator.cs — BORRAR después de usar
using MedRec.Identity.DataContext.MySql.Services;

namespace MedRec.Identity.UseCases.Tests;
public class _TempHashGenerator
{
    [Fact]
    public void PrintHash()
    {
        var hasher = new PasswordHasherService();
        var hash = hasher.Hash("Cambiar123!");
        throw new Exception(hash); // el mensaje de la excepción muestra el hash en el output del test
    }
}
```

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj" --filter "PrintHash"`
Expected: FAIL con el hash en el mensaje de excepción — copiar ese string (arranca con `AQAAAA...`). Después, **borrar `_TempHashGenerator.cs`** (no debe quedar en el repo).

- [ ] **Step 8: Agregar el seed al final de `Up()`, después de los `CreateTable`/`AddColumn` generados**

```csharp
            migrationBuilder.Sql(@"
                INSERT INTO Permissions (Id, Code, Description, IsDeleted)
                SELECT UUID(), t.Code, t.Description, 0 FROM (
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

                INSERT INTO Roles (Id, Name, Description, IsDeleted)
                VALUES (UUID(), 'Administrador', 'Rol con todos los permisos del sistema', 0);

                INSERT INTO RolePermissions (RoleId, PermissionId)
                SELECT r.Id, p.Id FROM Roles r CROSS JOIN Permissions p WHERE r.Name = 'Administrador';

                INSERT INTO Users (Id, Email, PasswordHash, FullName, IsActive, DoctorId, IsDeleted)
                VALUES (UUID(), 'admin@medrec.local', '<PEGAR_HASH_DEL_STEP_7>', 'Administrador del sistema', 1, NULL, 0);

                INSERT INTO UserRoles (UserId, RoleId)
                SELECT u.Id, r.Id FROM Users u CROSS JOIN Roles r
                WHERE u.Email = 'admin@medrec.local' AND r.Name = 'Administrador';
            ");
```

Reemplazar `<PEGAR_HASH_DEL_STEP_7>` por el hash real obtenido en el Step 7.

- [ ] **Step 9: Aplicar la migración contra la base de desarrollo**

Run:
```bash
dotnet ef database update --project "4.Framework&Drivers/MedRec.DataContext.MySql" --startup-project "4.Framework&Drivers/MedRec.WPF.UI"
```

Expected: aplica sin errores. Verificar manualmente con un cliente MySQL: `SELECT Email, FullName FROM Users;` debe devolver `admin@medrec.local`, y `SELECT COUNT(*) FROM Permissions;` debe devolver 28.

- [ ] **Step 10: Commit**

```bash
git add "4.Framework&Drivers/MedRec.IoC/" "4.Framework&Drivers/MedRec.WPF.UI/MedRec.WPF.UI.csproj" "4.Framework&Drivers/MedRec.WPF.UI/_Imports.razor" "4.Framework&Drivers/MedRec.DataContext.MySql/Migrations/"
git commit -m "feat(identity): cablear composition root, migración AddIdentityAndAudit y seed de admin/permisos"
```

---

### Task 16: Verificación manual end-to-end

**Files:** ninguno (solo verificación).

- [ ] **Step 1: Correr toda la suite de tests**

Run: `dotnet test MedRecSolution2025.sln`
Expected: todos los tests pasan, incluidos los 8 nuevos de `MedRec.Identity.UseCases.Tests` (y los ya existentes de `MedRec.MedicalVisit.UseCases.Tests` sin regresiones).

- [ ] **Step 2: Levantar la app y probar el login**

Run: `dotnet run --project "4.Framework&Drivers\MedRec.WPF.UI\MedRec.WPF.UI.csproj"`

Verificar manualmente:
1. La app abre mostrando la pantalla de login (no la lista de pacientes).
2. Con credenciales incorrectas, muestra "Email o contraseña incorrectos." y no entra.
3. Con `admin@medrec.local` / `Cambiar123!`, entra y muestra el `NavMenu` con el nombre "Administrador del sistema" y el botón de logout.
4. Crear un paciente nuevo desde la UI y confirmar en la base (`SELECT CreatedBy, CreatedAt FROM Patients ORDER BY CreatedAt DESC LIMIT 1;`) que `CreatedBy` coincide con el `Id` del usuario admin y `CreatedAt` es reciente.
5. Editar ese mismo paciente y confirmar que `UpdatedBy`/`UpdatedAt` quedan completos.
6. Click en "Cerrar sesión" — debe volver a la pantalla de login.

- [ ] **Step 3: Reportar resultado**

Si algún paso falla, no continuar a otros planes hasta resolverlo — este plan es la base de todo lo demás (CRUD de Usuarios/Roles y enforcement de permisos en las features existentes).

---

## Qué sigue después de este plan

- **Plan 2 — Administración de Usuarios y Roles**: pantallas CRUD para gestionar `User`/`Role` y asignar permisos, usando `IUserQueriesRepository`/nuevos `IUserCommandsRepository`/`IRoleQueriesRepository`/`IRoleCommandsRepository`.
- **Plan 3 — Enforcement de permisos en features existentes**: agregar `await _authorizationService.EnsurePermissionAsync(...)` al inicio de cada interactor sensible de Patients/MedicalVisit/MedicalAppointments/HealthInsurance/DynamicTemplates, más los componentes `HasPermission` en sus Views.
- **Spec 2 y 3 del roadmap** (acceso por médico + derivaciones, campos dinámicos por profesional) y **Spec 4** (cifrado en reposo) siguen pendientes de brainstorming propio.
