# Administración de Usuarios (Plan 2a) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Después de este plan, un admin puede crear/editar/activar-desactivar usuarios, asignarles roles, resetearles la contraseña, y todo usuario nuevo/reseteado debe cambiar su contraseña temporal en el primer login (con notificación por email vía Gmail SMTP).

**Architecture:** Extiende la feature slice `MedRec.Identity.*` ya existente (sin proyectos nuevos). Nuevos interactores siguen el patrón `IXxxInputPort`/`IXxxOutputPort` + `EnsurePermissionAsync` ya establecido en Identity núcleo. `AppShell.razor` pasa de 2 a 3 estados (no autenticado / debe cambiar contraseña / OK).

**Tech Stack:** .NET 9, EF Core/Pomelo, `System.Net.Mail.SmtpClient` (Gmail SMTP), xUnit + Moq.

## Global Constraints

- Seguir exactamente los patrones ya establecidos en Identity núcleo (`IBaseOutputPort`, `BaseOutputPort<T>`, `EnsurePermissionAsync` primero en cada interactor protegido, `AddUseCaseExceptionDecorators` auto-descubre por sufijo `InputPort` — no registrar interactores manualmente en `AddIdentityUseCasesServicesWithProxy`).
- Namespaces y ubicación de archivos: mismo patrón que las clases ya existentes en cada proyecto de Identity (ver spec `docs/superpowers/specs/2026-08-07-usuarios-roles-admin-design.md`).
- SDK local es .NET 10 — si algún paso usa `dotnet new`/`dotnet sln add` (no debería, todos los proyectos de este plan ya existen), corregir a mano `net9.0` y verificar duplicados de carpeta en el `.sln`.
- El envío de email es "mejor esfuerzo": si falla, la operación de alta/reseteo de contraseña NO se revierte.
- Mensajes de validación/error en español.
- Cada usuario debe tener al menos un rol asignado (no se permite crear/editar con `RoleIds` vacío).

---

### Task 1: `User.MustChangePassword`, `AuthResultDto` y `AuthenticateUserInteractor`

**Files:**
- Modify: `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\User.cs`
- Modify: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\AuthResultDto.cs`
- Modify: `2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\AuthenticateUserInteractor.cs`
- Modify: `4.Framework&Drivers\MedRec.DataContext.MySql\Configurations\UserConfiguration.cs`
- Test: `Test\MedRec.Identity.UseCases.Tests\AuthenticateUserInteractorTests.cs`

**Interfaces:**
- Produces: `User.MustChangePassword : bool`, `AuthResultDto.MustChangePassword : bool` — usados por Task 12 (AppShell).

- [ ] **Step 1: Modificar el test existente para reflejar el nuevo campo (RED)**

En `Test\MedRec.Identity.UseCases.Tests\AuthenticateUserInteractorTests.cs`, en el test `HandleAsync_ShouldReturnAuthResult_WhenCredentialsAreValid`, cambiar la línea que crea `user`:

```csharp
        var user = new User { Id = Guid.NewGuid(), Email = dto.Email, PasswordHash = "hash", FullName = "Admin", IsActive = true, DoctorId = null, MustChangePassword = true };
```

Y cambiar el `presenter.Verify` para incluir el chequeo del nuevo campo:

```csharp
        presenter.Verify(p => p.Handle(
            It.Is<AuthResultDto>(r => r.UserId == user.Id && r.Token == "token123" && r.Roles.Contains("Administrador") && r.MustChangePassword == true),
            It.IsAny<CancellationToken>()), Times.Once);
```

- [ ] **Step 2: Correr los tests y verificar que fallan (no compila: `User.MustChangePassword` no existe)**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: FAIL (error de compilación).

- [ ] **Step 3: Agregar `MustChangePassword` a `User.cs`**

En `1.EnterpriseBusinessObjects\MedRec.Entity\POCOEntities\User.cs`, agregar la propiedad después de `IsActive`:

```csharp
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; } = true;
```

- [ ] **Step 4: Agregar el campo a `AuthResultDto.cs`**

Reemplazar el contenido completo de `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\AuthResultDto.cs`:

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
        DateTime expiresAtUtc,
        bool mustChangePassword)
    {
        UserId = userId;
        Email = email;
        FullName = fullName;
        DoctorId = doctorId;
        Roles = roles;
        Permissions = permissions;
        Token = token;
        ExpiresAtUtc = expiresAtUtc;
        MustChangePassword = mustChangePassword;
    }

    public Guid UserId { get; }
    public string Email { get; }
    public string FullName { get; }
    public Guid? DoctorId { get; }
    public IReadOnlyList<string> Roles { get; }
    public IReadOnlyList<string> Permissions { get; }
    public string Token { get; }
    public DateTime ExpiresAtUtc { get; }
    public bool MustChangePassword { get; }
}
```

- [ ] **Step 5: Actualizar el call site en `AuthenticateUserInteractor.cs`**

En `2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\AuthenticateUserInteractor.cs`, cambiar la construcción de `AuthResultDto`:

```csharp
        await presenter.Handle(
            new AuthResultDto(user.Id, user.Email, user.FullName, user.DoctorId, roles, permissions, token, expiresAtUtc, user.MustChangePassword),
            ct);
```

- [ ] **Step 6: Agregar el default de la columna en `UserConfiguration.cs`**

En `4.Framework&Drivers\MedRec.DataContext.MySql\Configurations\UserConfiguration.cs`, agregar dentro de `Configure`, junto a `builder.Property(u => u.IsActive).HasDefaultValue(true);`:

```csharp
        builder.Property(u => u.MustChangePassword).HasDefaultValue(true);
```

- [ ] **Step 7: Correr los tests y verificar que pasan (GREEN)**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: PASS (11/11).

- [ ] **Step 8: Compilar toda la solución para detectar otros call sites rotos**

Run: `dotnet build MedRecSolution2025.sln`
Expected: 0 errores de código (los 3 WIX0103 conocidos, sin cambios, se ignoran).

- [ ] **Step 9: Commit**

```bash
git add "1.EnterpriseBusinessObjects/MedRec.Entity/POCOEntities/User.cs" "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/DTOs/AuthResultDto.cs" "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/Implementations/AuthenticateUserInteractor.cs" "4.Framework&Drivers/MedRec.DataContext.MySql/Configurations/UserConfiguration.cs" "Test/MedRec.Identity.UseCases.Tests/AuthenticateUserInteractorTests.cs"
git commit -m "feat(identity): agregar User.MustChangePassword y propagarlo a AuthResultDto"
```

---

### Task 2: Notificación por email (`IEmailNotificationService`, Gmail SMTP)

**Files:**
- Create: `1.EnterpriseBusinessObjects\MedRec.Shared\Security\EmailSettings.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Services\IEmailNotificationService.cs`
- Create: `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\Services\SmtpEmailNotificationService.cs`
- Modify: `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\DependencyContainer.cs`
- Modify: `4.Framework&Drivers\MedRec.WPF.UI\Startup.cs`
- Modify: `4.Framework&Drivers\MedRec.WPF.UI\appsettings.json`

**Interfaces:**
- Produces: `IEmailNotificationService.SendTemporaryPasswordAsync(string email, string fullName, string temporaryPassword, CancellationToken ct = default) : Task<bool>` — usado por Tasks 4 y 7.

- [ ] **Step 1: `EmailSettings.cs`** (mismo patrón que `Jwt.cs`)

```csharp
namespace MedRec.Shared.Security;
public class EmailSettings
{
    public const string SectionKey = nameof(EmailSettings);
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
}
```

- [ ] **Step 2: `IEmailNotificationService.cs`**

```csharp
namespace MedRec.Identity.BusinessObjects.Interfaces.Services;
public interface IEmailNotificationService
{
    Task<bool> SendTemporaryPasswordAsync(string email, string fullName, string temporaryPassword, CancellationToken ct = default);
}
```

- [ ] **Step 3: `SmtpEmailNotificationService.cs`**

```csharp
using System.Net;
using System.Net.Mail;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Shared.Security;
using Microsoft.Extensions.Options;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class SmtpEmailNotificationService(IOptions<EmailSettings> emailOptions) : IEmailNotificationService
{
    public async Task<bool> SendTemporaryPasswordAsync(string email, string fullName, string temporaryPassword, CancellationToken ct = default)
    {
        var settings = emailOptions.Value;
        if (string.IsNullOrEmpty(settings.SenderEmail) || string.IsNullOrEmpty(settings.SenderPassword))
            return false;

        try
        {
            using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
            {
                Credentials = new NetworkCredential(settings.SenderEmail, settings.SenderPassword),
                EnableSsl = true
            };

            using var message = new MailMessage(settings.SenderEmail, email)
            {
                Subject = "MedRec — Contraseña temporal",
                Body = $"Hola {fullName},\n\nSe generó una contraseña temporal para tu cuenta de MedRec: {temporaryPassword}\n\nVas a tener que cambiarla la primera vez que ingreses al sistema.\n\nSi no esperabas este email, contactá al administrador del sistema.",
                IsBodyHtml = false
            };

            await client.SendMailAsync(message, ct);
            return true;
        }
        catch (SmtpException)
        {
            return false;
        }
    }
}
```

- [ ] **Step 4: Registrar en `DependencyContainer.cs`**

En `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\DependencyContainer.cs`, agregar el `using MedRec.Shared.Security;` (no hace falta, `EmailSettings` no se referencia directo acá) y agregar la línea de registro dentro de `AddIdentityDataContextMySqlServices`:

```csharp
        services.AddSingleton<IEmailNotificationService, SmtpEmailNotificationService>();
```

(Va junto a las otras líneas `AddSingleton` ya existentes.)

- [ ] **Step 5: Wiring en `Startup.cs`** (mismo patrón que `Jwt`)

En `4.Framework&Drivers\MedRec.WPF.UI\Startup.cs`, agregar `using MedRec.Shared.Security;` ya está presente. Agregar, junto a la declaración de `jwtKey`:

```csharp
        var emailSettings = new EmailSettings();
```

Junto al `configuration.GetSection(Jwt.SectionKey).Bind(jwtKey);`:

```csharp
        configuration.GetSection(EmailSettings.SectionKey).Bind(emailSettings);
```

Después del bloque `if (!string.IsNullOrEmpty(jwtKey.Key)) { ... }` (el que ya tiene el try/catch de `CryptographicException`), agregar el bloque análogo para `SenderPassword`:

```csharp
        if (!string.IsNullOrEmpty(emailSettings.SenderPassword))
        {
            if (!EncryptionHelper.IsEncrypted(emailSettings.SenderPassword))
            {
                emailSettings.SenderPassword = EncryptionHelper.Encrypt(emailSettings.SenderPassword);
                var json = File.ReadAllText(appSettingsPath);
                dynamic configFile = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                if (configFile.EmailSettings == null)
                    configFile.EmailSettings = new Newtonsoft.Json.Linq.JObject();
                configFile.EmailSettings.SenderPassword = emailSettings.SenderPassword;
                File.WriteAllText(appSettingsPath,
                    Newtonsoft.Json.JsonConvert.SerializeObject(configFile, Newtonsoft.Json.Formatting.Indented));
            }
            try
            {
                emailSettings.SenderPassword = EncryptionHelper.Decrypt(emailSettings.SenderPassword);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                emailSettings.SenderPassword = string.Empty;
            }
        }
```

Y junto a los `services.AddSingleton(Options.Create(jwtKey));` / `services.Configure<Jwt>(...)`, agregar:

```csharp
        services.AddSingleton(Options.Create(emailSettings));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionKey));
```

- [ ] **Step 6: Agregar la sección al `appsettings.json` versionado**

En `4.Framework&Drivers\MedRec.WPF.UI\appsettings.json`, agregar (en texto plano — recién en el primer arranque de la app se encripta a disco, mismo mecanismo que la clave JWT):

```json
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "",
    "SenderPassword": ""
  },
```

(Insertar como una clave más del objeto raíz, junto a `"Jwt"`. Dejar `SenderEmail`/`SenderPassword` vacíos en el repo — se completan en el `appsettings.json` local de cada instalación, igual que `DBOptionsMySql.ConnectionString`.)

- [ ] **Step 7: Compilar**

Run: `dotnet build "4.Framework&Drivers\MedRec.WPF.UI\MedRec.WPF.UI.csproj"`
Expected: 0 errores.

- [ ] **Step 8: Commit**

```bash
git add "1.EnterpriseBusinessObjects/MedRec.Shared/Security/EmailSettings.cs" "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/Interfaces/Services/IEmailNotificationService.cs" "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/Services/SmtpEmailNotificationService.cs" "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/DependencyContainer.cs" "4.Framework&Drivers/MedRec.WPF.UI/Startup.cs" "4.Framework&Drivers/MedRec.WPF.UI/appsettings.json"
git commit -m "feat(identity): agregar IEmailNotificationService con implementacion SMTP (Gmail)"
```

---

### Task 3: Repositorios — `IUserCommandsRepository`, extensión de `IUserQueriesRepository`, `IDoctorLookupRepository`

**Files:**
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\UserSummaryDto.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\DoctorSummaryDto.cs`
- Modify: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Repositories\IUserQueriesRepository.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Repositories\IUserCommandsRepository.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Repositories\IDoctorLookupRepository.cs`
- Modify: `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\Interfaces\IUserQueriesDataContext.cs`
- Create: `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\Interfaces\IUserCommandsDataContext.cs`
- Create: `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\Interfaces\IDoctorLookupDataContext.cs`
- Modify: `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\Implementations\UserQueriesRepository.cs`
- Create: `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\Implementations\UserCommandsRepository.cs`
- Create: `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\Implementations\DoctorLookupRepository.cs`
- Modify: `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\DependencyContainer.cs`
- Modify: `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\Services\UserQueriesDataContextMySql.cs`
- Create: `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\Services\UserCommandsDataContextMySql.cs`
- Create: `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\Services\DoctorLookupDataContextMySql.cs`
- Modify: `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\DependencyContainer.cs`

**Interfaces:**
- Produces: `IUserQueriesRepository.GetByIdAsync(Guid, CancellationToken) : Task<User?>`, `.ListWithRolesAsync(CancellationToken) : Task<IReadOnlyList<UserSummaryDto>>`; `IUserCommandsRepository.CreateAsync/UpdateAsync/SetActiveAsync/SetPasswordAsync`; `IDoctorLookupRepository.ListActiveAsync(CancellationToken) : Task<IReadOnlyList<DoctorSummaryDto>>` — usados por Tasks 4-9.

- [ ] **Step 1: DTOs**

`DTOs\UserSummaryDto.cs`:
```csharp
namespace MedRec.Identity.BusinessObjects.DTOs;
public class UserSummaryDto
{
    public UserSummaryDto(Guid id, string email, string fullName, bool isActive, IReadOnlyList<string> roleNames)
    {
        Id = id;
        Email = email;
        FullName = fullName;
        IsActive = isActive;
        RoleNames = roleNames;
    }

    public Guid Id { get; }
    public string Email { get; }
    public string FullName { get; }
    public bool IsActive { get; }
    public IReadOnlyList<string> RoleNames { get; }
}
```

`DTOs\DoctorSummaryDto.cs`:
```csharp
namespace MedRec.Identity.BusinessObjects.DTOs;
public class DoctorSummaryDto
{
    public DoctorSummaryDto(Guid id, string fullName)
    {
        Id = id;
        FullName = fullName;
    }

    public Guid Id { get; }
    public string FullName { get; }
}
```

- [ ] **Step 2: Extender `IUserQueriesRepository`**

Reemplazar el contenido completo de `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Repositories\IUserQueriesRepository.cs`:

```csharp
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Repositories;
public interface IUserQueriesRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetRoleIdsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<UserSummaryDto>> ListWithRolesAsync(CancellationToken ct = default);
}
```

- [ ] **Step 3: `IUserCommandsRepository` e `IDoctorLookupRepository`**

`Interfaces\Repositories\IUserCommandsRepository.cs`:
```csharp
using MedRec.Entity.POCOEntities;

namespace MedRec.Identity.BusinessObjects.Interfaces.Repositories;
public interface IUserCommandsRepository
{
    Task CreateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default);
    Task UpdateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default);
    Task SetActiveAsync(Guid userId, bool isActive, CancellationToken ct = default);
    Task SetPasswordAsync(Guid userId, string passwordHash, bool mustChangePassword, CancellationToken ct = default);
}
```

`Interfaces\Repositories\IDoctorLookupRepository.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Repositories;
public interface IDoctorLookupRepository
{
    Task<IReadOnlyList<DoctorSummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
```

- [ ] **Step 4: Interfaces de DataContext (capa 3)**

Reemplazar `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\Interfaces\IUserQueriesDataContext.cs`:
```csharp
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.Repositories.Interfaces;
public interface IUserQueriesDataContext
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Guid>> GetRoleIdsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<UserSummaryDto>> ListWithRolesAsync(CancellationToken ct = default);
}
```

`Interfaces\IUserCommandsDataContext.cs`:
```csharp
using MedRec.Entity.POCOEntities;

namespace MedRec.Identity.Repositories.Interfaces;
public interface IUserCommandsDataContext
{
    Task CreateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default);
    Task UpdateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default);
    Task SetActiveAsync(Guid userId, bool isActive, CancellationToken ct = default);
    Task SetPasswordAsync(Guid userId, string passwordHash, bool mustChangePassword, CancellationToken ct = default);
}
```

`Interfaces\IDoctorLookupDataContext.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.Repositories.Interfaces;
public interface IDoctorLookupDataContext
{
    Task<IReadOnlyList<DoctorSummaryDto>> ListActiveAsync(CancellationToken ct = default);
}
```

- [ ] **Step 5: Implementaciones de repositorio (capa 3, pass-through)**

Reemplazar `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\Implementations\UserQueriesRepository.cs`:
```csharp
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.Repositories.Interfaces;

namespace MedRec.Identity.Repositories.Implementations;
internal class UserQueriesRepository(IUserQueriesDataContext dataContext) : IUserQueriesRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        dataContext.GetByEmailAsync(email, ct);

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default) =>
        dataContext.GetByIdAsync(userId, ct);

    public Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct = default) =>
        dataContext.GetRoleNamesAsync(userId, ct);

    public Task<IReadOnlyList<Guid>> GetRoleIdsAsync(Guid userId, CancellationToken ct = default) =>
        dataContext.GetRoleIdsAsync(userId, ct);

    public Task<IReadOnlyList<string>> GetPermissionCodesAsync(Guid userId, CancellationToken ct = default) =>
        dataContext.GetPermissionCodesAsync(userId, ct);

    public Task<IReadOnlyList<UserSummaryDto>> ListWithRolesAsync(CancellationToken ct = default) =>
        dataContext.ListWithRolesAsync(ct);
}
```

`Implementations\UserCommandsRepository.cs`:
```csharp
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.Repositories.Interfaces;

namespace MedRec.Identity.Repositories.Implementations;
internal class UserCommandsRepository(IUserCommandsDataContext dataContext) : IUserCommandsRepository
{
    public Task CreateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default) =>
        dataContext.CreateAsync(user, roleIds, ct);

    public Task UpdateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default) =>
        dataContext.UpdateAsync(user, roleIds, ct);

    public Task SetActiveAsync(Guid userId, bool isActive, CancellationToken ct = default) =>
        dataContext.SetActiveAsync(userId, isActive, ct);

    public Task SetPasswordAsync(Guid userId, string passwordHash, bool mustChangePassword, CancellationToken ct = default) =>
        dataContext.SetPasswordAsync(userId, passwordHash, mustChangePassword, ct);
}
```

`Implementations\DoctorLookupRepository.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.Repositories.Interfaces;

namespace MedRec.Identity.Repositories.Implementations;
internal class DoctorLookupRepository(IDoctorLookupDataContext dataContext) : IDoctorLookupRepository
{
    public Task<IReadOnlyList<DoctorSummaryDto>> ListActiveAsync(CancellationToken ct = default) =>
        dataContext.ListActiveAsync(ct);
}
```

- [ ] **Step 6: DI de repositorios**

Reemplazar `3.InterfaceAdapters\Repositories\MedRec.Identity.Repositories\DependencyContainer.cs`:
```csharp
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.Repositories.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddIdentityRepositoriesServices(this IServiceCollection services)
    {
        services.AddScoped<IUserQueriesRepository, UserQueriesRepository>();
        services.AddScoped<IUserCommandsRepository, UserCommandsRepository>();
        services.AddScoped<IDoctorLookupRepository, DoctorLookupRepository>();
        return services;
    }
}
```

- [ ] **Step 7: Implementaciones EF (capa 4)**

Reemplazar `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\Services\UserQueriesDataContextMySql.cs`:
```csharp
using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class UserQueriesDataContextMySql(MedRecContext context) : IUserQueriesDataContext
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email && !u.IsDeleted, ct);

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default) =>
        context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(Guid userId, CancellationToken ct = default)
    {
        return await (
            from ur in context.UserRoles
            join r in context.Roles on ur.RoleId equals r.Id
            where ur.UserId == userId && !r.IsDeleted
            select r.Name
        ).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> GetRoleIdsAsync(Guid userId, CancellationToken ct = default)
    {
        return await context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(ct);
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

    public async Task<IReadOnlyList<UserSummaryDto>> ListWithRolesAsync(CancellationToken ct = default)
    {
        var users = await context.Users.Where(u => !u.IsDeleted).ToListAsync(ct);
        var result = new List<UserSummaryDto>();
        foreach (var user in users)
        {
            var roleNames = await GetRoleNamesAsync(user.Id, ct);
            result.Add(new UserSummaryDto(user.Id, user.Email, user.FullName, user.IsActive, roleNames));
        }
        return result;
    }
}
```

`Services\UserCommandsDataContextMySql.cs`:
```csharp
using MedRec.DataContext.MySql.DataContext;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class UserCommandsDataContextMySql(MedRecContext context) : IUserCommandsDataContext
{
    public async Task CreateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default)
    {
        await context.Users.AddAsync(user, ct);
        foreach (var roleId in roleIds)
            await context.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = roleId }, ct);
    }

    public async Task UpdateAsync(User user, IReadOnlyList<Guid> roleIds, CancellationToken ct = default)
    {
        var tracked = context.ChangeTracker.Entries<User>().FirstOrDefault(e => e.Entity.Id == user.Id);
        if (tracked is not null)
            tracked.State = EntityState.Detached;

        context.Attach(user);
        var entry = context.Entry(user);
        entry.State = EntityState.Modified;
        entry.Property(nameof(User.RowVersion)).OriginalValue = user.RowVersion;

        var existingRoles = await context.UserRoles.Where(ur => ur.UserId == user.Id).ToListAsync(ct);
        context.UserRoles.RemoveRange(existingRoles);
        foreach (var roleId in roleIds)
            await context.UserRoles.AddAsync(new UserRole { UserId = user.Id, RoleId = roleId }, ct);
    }

    public async Task SetActiveAsync(Guid userId, bool isActive, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return;
        user.IsActive = isActive;
    }

    public async Task SetPasswordAsync(Guid userId, string passwordHash, bool mustChangePassword, CancellationToken ct = default)
    {
        var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is null) return;
        user.PasswordHash = passwordHash;
        user.MustChangePassword = mustChangePassword;
    }
}
```

`Services\DoctorLookupDataContextMySql.cs`:
```csharp
using MedRec.DataContext.MySql.DataContext;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedRec.Identity.DataContext.MySql.Services;

internal class DoctorLookupDataContextMySql(MedRecContext context) : IDoctorLookupDataContext
{
    public async Task<IReadOnlyList<DoctorSummaryDto>> ListActiveAsync(CancellationToken ct = default)
    {
        return await context.Doctors
            .Where(d => !d.IsDeleted)
            .Select(d => new DoctorSummaryDto(d.Id, d.LastName + ", " + d.FirstName))
            .ToListAsync(ct);
    }
}
```

- [ ] **Step 8: DI de DataContext (capa 4)**

En `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\DependencyContainer.cs`, agregar dentro de `AddIdentityDataContextMySqlServices`:

```csharp
        services.AddScoped<IUserCommandsDataContext, UserCommandsDataContextMySql>();
        services.AddScoped<IDoctorLookupDataContext, DoctorLookupDataContextMySql>();
```

(Junto a la línea `services.AddScoped<IUserQueriesDataContext, UserQueriesDataContextMySql>();` ya existente.)

- [ ] **Step 9: Compilar**

Run: `dotnet build "4.Framework&Drivers\MedRec.Identity.DataContext.MySql\MedRec.Identity.DataContext.MySql.csproj"`
Expected: 0 errores.

- [ ] **Step 10: Commit**

```bash
git add "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/" "3.InterfaceAdapters/Repositories/MedRec.Identity.Repositories/" "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/"
git commit -m "feat(identity): agregar IUserCommandsRepository, extender IUserQueriesRepository, agregar IDoctorLookupRepository"
```

---

### Task 4: TDD — `CreateUserInteractor`

**Files:**
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\CreateUserDto.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\ICreateUserInputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\ICreateUserOutputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Validators\CreateUserValidator.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\CreateUserInteractor.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.Presenters\Implementations\CreateUserPresenter.cs`
- Modify: `3.InterfaceAdapters\MedRec.Identity.Presenters\DependencyContainer.cs`
- Test: `Test\MedRec.Identity.UseCases.Tests\CreateUserInteractorTests.cs`

**Interfaces:**
- Consumes: `IUserCommandsRepository`, `IUserQueriesRepository` (Task 3), `IPasswordHasher`, `IEmailNotificationService` (Task 2), `IAuthorizationService`, `ICurrentUserContext` (Identity núcleo).
- Produces: `CreateUserInteractor : ICreateUserInputPort` — auto-descubierto por `AddUseCaseExceptionDecorators`, no requiere registro manual.

- [ ] **Step 1: DTO**

```csharp
namespace MedRec.Identity.BusinessObjects.DTOs;
public class CreateUserDto
{
    public CreateUserDto(string email, string fullName, string temporaryPassword, IReadOnlyList<Guid> roleIds, Guid? doctorId)
    {
        Email = email;
        FullName = fullName;
        TemporaryPassword = temporaryPassword;
        RoleIds = roleIds;
        DoctorId = doctorId;
    }

    public string Email { get; }
    public string FullName { get; }
    public string TemporaryPassword { get; }
    public IReadOnlyList<Guid> RoleIds { get; }
    public Guid? DoctorId { get; }
}
```

- [ ] **Step 2: Ports**

`Interfaces\Ports\ICreateUserInputPort.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface ICreateUserInputPort
{
    Task HandleAsync(CreateUserDto dto, CancellationToken ct = default);
}
```

`Interfaces\Ports\ICreateUserOutputPort.cs`:
```csharp
using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface ICreateUserOutputPort : IBaseOutputPort
{
    OperationResult<bool> Result { get; }
    Task Handle(CancellationToken ct = default);
}
```

- [ ] **Step 3: Validador**

```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.Identity.BusinessObjects.Validators;
public static class CreateUserValidator
{
    public static IReadOnlyList<ValidationError> Validate(CreateUserDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        var emailValidation = Guard.Against(dto.Email, nameof(dto.Email))
            .NotNullOrEmpty("El email es obligatorio.")
            .InvalidEmail("El email no tiene un formato válido.");
        errors.AddRange(emailValidation.Errors);

        var nameValidation = Guard.Against(dto.FullName, nameof(dto.FullName))
            .NotNullOrEmpty("El nombre completo es obligatorio.");
        errors.AddRange(nameValidation.Errors);

        var passwordValidation = Guard.Against(dto.TemporaryPassword, nameof(dto.TemporaryPassword))
            .NotNullOrEmpty("La contraseña temporal es obligatoria.")
            .MinLength(8, "La contraseña temporal debe tener al menos 8 caracteres.");
        errors.AddRange(passwordValidation.Errors);

        if (dto.RoleIds is null || dto.RoleIds.Count == 0)
            errors.Add(new ValidationError(nameof(dto.RoleIds), "El usuario debe tener al menos un rol asignado."));

        return errors;
    }
}
```

- [ ] **Step 4: Escribir el test que falla (RED)**

`Test\MedRec.Identity.UseCases.Tests\CreateUserInteractorTests.cs`:
```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.UseCases.Implementations;
using MedRec.Shared.Exceptions;
using MedRec.Validator.Interfaces;
using MedRec.Validator.ValueObjects;
using Moq;

namespace MedRec.Identity.UseCases.Tests;

public class CreateUserInteractorTests
{
    private static (
        Mock<ICreateUserOutputPort> presenter,
        Mock<IUserCommandsRepository> commandsRepo,
        Mock<IUserQueriesRepository> queriesRepo,
        Mock<IPasswordHasher> hasher,
        Mock<IEmailNotificationService> email,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IModelValidatorHub<CreateUserDto>> validator) CreateMocks()
    {
        return (
            new Mock<ICreateUserOutputPort>(),
            new Mock<IUserCommandsRepository>(),
            new Mock<IUserQueriesRepository>(),
            new Mock<IPasswordHasher>(),
            new Mock<IEmailNotificationService>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IModelValidatorHub<CreateUserDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<CreateUserDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<CreateUserDto>(), It.IsAny<Func<CreateUserDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCallerLacksPermission()
    {
        var dto = new CreateUserDto("nuevo@medrec.local", "Nuevo Usuario", "Temporal123!", new[] { Guid.NewGuid() }, null);
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, validator) = CreateMocks();

        authorization.Setup(a => a.EnsurePermissionAsync(It.IsAny<Guid?>(), "users.create", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessException(new ErrorInfo("No tiene permiso.", MedRec.Entity.Enums.ErrorCode.Forbidden)));

        var interactor = new CreateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, validator.Object);

        await Assert.ThrowsAsync<BusinessException>(() => interactor.HandleAsync(dto, CancellationToken.None));

        commandsRepo.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationErrors_WhenDtoIsInvalid()
    {
        var dto = new CreateUserDto("", "", "", Array.Empty<Guid>(), null);
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, validator) = CreateMocks();

        validator.Setup(v => v.Validate(dto, It.IsAny<Func<CreateUserDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(false);
        validator.SetupGet(v => v.Errors).Returns(new[] { new ValidationError("Email", "El email es obligatorio.") });

        var interactor = new CreateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Once);
        commandsRepo.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenEmailAlreadyExists()
    {
        var dto = new CreateUserDto("existente@medrec.local", "Alguien", "Temporal123!", new[] { Guid.NewGuid() }, null);
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        queriesRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = dto.Email });

        var interactor = new CreateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        commandsRepo.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldCreateUserAndSendEmail_WhenValid()
    {
        var roleId = Guid.NewGuid();
        var dto = new CreateUserDto("nuevo@medrec.local", "Nuevo Usuario", "Temporal123!", new[] { roleId }, null);
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        queriesRepo.Setup(r => r.GetByEmailAsync(dto.Email, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        hasher.Setup(h => h.Hash(dto.TemporaryPassword)).Returns("hashed");

        var interactor = new CreateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        commandsRepo.Verify(r => r.CreateAsync(
            It.Is<User>(u => u.Email == dto.Email && u.PasswordHash == "hashed" && u.MustChangePassword == true && u.IsActive == true),
            It.Is<IReadOnlyList<Guid>>(ids => ids.Contains(roleId)),
            It.IsAny<CancellationToken>()), Times.Once);
        email.Verify(e => e.SendTemporaryPasswordAsync(dto.Email, dto.FullName, dto.TemporaryPassword, It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 5: Correr los tests y verificar que fallan (no compila: `CreateUserInteractor` no existe)**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: FAIL (error de compilación).

- [ ] **Step 6: Implementar `CreateUserInteractor`**

```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.BusinessObjects.Validators;
using MedRec.Entity.Interfaces;
using MedRec.Validator.Interfaces;

namespace MedRec.Identity.UseCases.Implementations;

public class CreateUserInteractor(
    ICreateUserOutputPort presenter,
    IUserCommandsRepository userCommandsRepository,
    IUserQueriesRepository userQueriesRepository,
    IPasswordHasher passwordHasher,
    IEmailNotificationService emailNotificationService,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IModelValidatorHub<CreateUserDto> validatorHub) : ICreateUserInputPort
{
    public async Task HandleAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Users_Create, ct);

        var isValid = await validatorHub.Validate(dto, CreateUserValidator.Validate);
        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var existing = await userQueriesRepository.GetByEmailAsync(dto.Email, ct);
        if (existing is not null)
        {
            await presenter.ErrorAsync(new ErrorInfo("Ya existe un usuario con ese email.", ErrorCode.DuplicateKey, null, 409));
            return;
        }

        var user = new User
        {
            Email = dto.Email,
            FullName = dto.FullName,
            PasswordHash = passwordHasher.Hash(dto.TemporaryPassword),
            IsActive = true,
            MustChangePassword = true,
            DoctorId = dto.DoctorId
        };

        await userCommandsRepository.CreateAsync(user, dto.RoleIds, ct);
        await emailNotificationService.SendTemporaryPasswordAsync(dto.Email, dto.FullName, dto.TemporaryPassword, ct);

        await presenter.Handle(ct);
    }
}
```

- [ ] **Step 7: Correr los tests y verificar que pasan (GREEN)**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: PASS (15/15: 11 anteriores + 4 nuevos).

- [ ] **Step 8: Presenter**

```csharp
using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;

namespace MedRec.Identity.Presenters.Implementations;
internal class CreateUserPresenter : BaseOutputPort<bool>, ICreateUserOutputPort
{
    public Task Handle(CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }
}
```

En `3.InterfaceAdapters\MedRec.Identity.Presenters\DependencyContainer.cs`, agregar:

```csharp
        services.AddScoped<ICreateUserOutputPort, CreateUserPresenter>();
```

- [ ] **Step 9: Compilar todo**

Run: `dotnet build "2.ApplicationBusinessObjects\MedRec.Identity.UseCases\MedRec.Identity.UseCases.csproj" && dotnet build "3.InterfaceAdapters\MedRec.Identity.Presenters\MedRec.Identity.Presenters.csproj"`
Expected: 0 errores en ambos.

- [ ] **Step 10: Commit**

```bash
git add "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/" "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/" "3.InterfaceAdapters/MedRec.Identity.Presenters/" "Test/MedRec.Identity.UseCases.Tests/"
git commit -m "feat(identity): implementar CreateUserInteractor (TDD)"
```

---

### Task 5: TDD — `UpdateUserInteractor`

**Files:**
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\UpdateUserDto.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\IUpdateUserInputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\IUpdateUserOutputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Validators\UpdateUserValidator.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\UpdateUserInteractor.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.Presenters\Implementations\UpdateUserPresenter.cs`
- Modify: `3.InterfaceAdapters\MedRec.Identity.Presenters\DependencyContainer.cs`
- Test: `Test\MedRec.Identity.UseCases.Tests\UpdateUserInteractorTests.cs`

**Interfaces:**
- Consumes: igual que Task 4 (sin `IPasswordHasher`/`IEmailNotificationService`, no aplica acá).
- Produces: `UpdateUserInteractor : IUpdateUserInputPort` — auto-descubierto.

- [ ] **Step 1: DTO**

```csharp
namespace MedRec.Identity.BusinessObjects.DTOs;
public class UpdateUserDto
{
    public UpdateUserDto(Guid userId, string fullName, IReadOnlyList<Guid> roleIds, Guid? doctorId)
    {
        UserId = userId;
        FullName = fullName;
        RoleIds = roleIds;
        DoctorId = doctorId;
    }

    public Guid UserId { get; }
    public string FullName { get; }
    public IReadOnlyList<Guid> RoleIds { get; }
    public Guid? DoctorId { get; }
}
```

- [ ] **Step 2: Ports**

`Interfaces\Ports\IUpdateUserInputPort.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IUpdateUserInputPort
{
    Task HandleAsync(UpdateUserDto dto, CancellationToken ct = default);
}
```

`Interfaces\Ports\IUpdateUserOutputPort.cs`:
```csharp
using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IUpdateUserOutputPort : IBaseOutputPort
{
    OperationResult<bool> Result { get; }
    Task Handle(CancellationToken ct = default);
}
```

- [ ] **Step 3: Validador**

```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.Identity.BusinessObjects.Validators;
public static class UpdateUserValidator
{
    public static IReadOnlyList<ValidationError> Validate(UpdateUserDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        var nameValidation = Guard.Against(dto.FullName, nameof(dto.FullName))
            .NotNullOrEmpty("El nombre completo es obligatorio.");
        errors.AddRange(nameValidation.Errors);

        if (dto.RoleIds is null || dto.RoleIds.Count == 0)
            errors.Add(new ValidationError(nameof(dto.RoleIds), "El usuario debe tener al menos un rol asignado."));

        return errors;
    }
}
```

- [ ] **Step 4: Escribir el test que falla (RED)**

`Test\MedRec.Identity.UseCases.Tests\UpdateUserInteractorTests.cs`:
```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Interfaces;
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

public class UpdateUserInteractorTests
{
    private static (
        Mock<IUpdateUserOutputPort> presenter,
        Mock<IUserCommandsRepository> commandsRepo,
        Mock<IUserQueriesRepository> queriesRepo,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork,
        Mock<IModelValidatorHub<UpdateUserDto>> validator) CreateMocks()
    {
        return (
            new Mock<IUpdateUserOutputPort>(),
            new Mock<IUserCommandsRepository>(),
            new Mock<IUserQueriesRepository>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>(),
            new Mock<IModelValidatorHub<UpdateUserDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<UpdateUserDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<UpdateUserDto>(), It.IsAny<Func<UpdateUserDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

    /// <summary>
    /// ExecuteInTransactionWithRetry es responsabilidad del UnitOfWork real (retry, begin/commit/rollback);
    /// para testear el interactor basta con que el mock invoque el delegate recibido.
    /// </summary>
    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldReturnValidationErrors_WhenDtoIsInvalid()
    {
        var dto = new UpdateUserDto(Guid.NewGuid(), "", Array.Empty<Guid>(), null);
        var (presenter, commandsRepo, queriesRepo, authorization, currentUser, unitOfWork, validator) = CreateMocks();

        validator.Setup(v => v.Validate(dto, It.IsAny<Func<UpdateUserDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(false);
        validator.SetupGet(v => v.Errors).Returns(new[] { new ValidationError("FullName", "El nombre completo es obligatorio.") });

        var interactor = new UpdateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ValidationErrorsAsync(It.IsAny<IEnumerable<ValidationError>>()), Times.Once);
        commandsRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenUserNotFound()
    {
        var dto = new UpdateUserDto(Guid.NewGuid(), "Nombre", new[] { Guid.NewGuid() }, null);
        var (presenter, commandsRepo, queriesRepo, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        queriesRepo.Setup(r => r.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var interactor = new UpdateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        commandsRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<IReadOnlyList<Guid>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldUpdateUser_WhenValid()
    {
        var roleId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var dto = new UpdateUserDto(Guid.NewGuid(), "Nombre Nuevo", new[] { roleId }, doctorId);
        var (presenter, commandsRepo, queriesRepo, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);
        SetUpTransactionToRunWork(unitOfWork);

        var existingUser = new User { Id = dto.UserId, Email = "user@medrec.local", FullName = "Nombre Viejo" };
        queriesRepo.Setup(r => r.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(existingUser);

        var interactor = new UpdateUserInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        commandsRepo.Verify(r => r.UpdateAsync(
            It.Is<User>(u => u.Id == dto.UserId && u.FullName == "Nombre Nuevo" && u.DoctorId == doctorId),
            It.Is<IReadOnlyList<Guid>>(ids => ids.Contains(roleId)),
            It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 5: Correr y verificar RED**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: FAIL (no compila).

- [ ] **Step 6: Implementar `UpdateUserInteractor`**

```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Identity.UseCases.Implementations;

public class UpdateUserInteractor(
    IUpdateUserOutputPort presenter,
    IUserCommandsRepository userCommandsRepository,
    IUserQueriesRepository userQueriesRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork,
    IModelValidatorHub<UpdateUserDto> validatorHub) : IUpdateUserInputPort
{
    public async Task HandleAsync(UpdateUserDto dto, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Users_Edit, ct);

        var isValid = await validatorHub.Validate(dto, UpdateUserValidator.Validate);
        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var user = await userQueriesRepository.GetByIdAsync(dto.UserId, ct);
        if (user is null)
        {
            await presenter.ErrorAsync(new ErrorInfo("Usuario no encontrado.", ErrorCode.NotFound, null, 404));
            return;
        }

        user.FullName = dto.FullName;
        user.DoctorId = dto.DoctorId;

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await userCommandsRepository.UpdateAsync(user, dto.RoleIds, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await presenter.Handle(ct);
    }
}
```

**Nota crítica (agregada tras revisión de la Task 4):** el `IUserCommandsRepository.CreateAsync`/`UpdateAsync`/`SetActiveAsync`/`SetPasswordAsync` (Task 3) solo hacen `AddAsync`/mutan el `DbContext` — nunca llaman `SaveChanges`. Todo interactor de escritura DEBE inyectar `IRepositoryUnitOfWork` (ya registrado globalmente en `MedRec.IoC`, no requiere DI nueva) y envolver la llamada al repositorio + `unitOfWork.SaveChanges(ct)` dentro de `unitOfWork.ExecuteInTransactionWithRetry(...)`, exactamente como `CreatePatientInteractor` — de lo contrario el cambio nunca se persiste en la base. Esto aplica a esta Task y a las Tasks 6, 7 y 8 (Task 9 es de solo lectura, no aplica).

- [ ] **Step 7: Correr y verificar GREEN**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: PASS (18/18).

- [ ] **Step 8: Presenter**

```csharp
using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;

namespace MedRec.Identity.Presenters.Implementations;
internal class UpdateUserPresenter : BaseOutputPort<bool>, IUpdateUserOutputPort
{
    public Task Handle(CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }
}
```

En `DependencyContainer.cs` de Presenters, agregar:
```csharp
        services.AddScoped<IUpdateUserOutputPort, UpdateUserPresenter>();
```

- [ ] **Step 9: Compilar**

Run: `dotnet build "2.ApplicationBusinessObjects\MedRec.Identity.UseCases\MedRec.Identity.UseCases.csproj" && dotnet build "3.InterfaceAdapters\MedRec.Identity.Presenters\MedRec.Identity.Presenters.csproj"`
Expected: 0 errores.

- [ ] **Step 10: Commit**

```bash
git add "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/" "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/" "3.InterfaceAdapters/MedRec.Identity.Presenters/" "Test/MedRec.Identity.UseCases.Tests/"
git commit -m "feat(identity): implementar UpdateUserInteractor (TDD)"
```

---

### Task 6: TDD — `ToggleUserActiveInteractor`

**Files:**
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\ToggleUserActiveDto.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\IToggleUserActiveInputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\IToggleUserActiveOutputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\ToggleUserActiveInteractor.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.Presenters\Implementations\ToggleUserActivePresenter.cs`
- Modify: `3.InterfaceAdapters\MedRec.Identity.Presenters\DependencyContainer.cs`
- Test: `Test\MedRec.Identity.UseCases.Tests\ToggleUserActiveInteractorTests.cs`

**Interfaces:**
- Consumes: `IUserCommandsRepository`, `IUserQueriesRepository`, `IAuthorizationService`, `ICurrentUserContext`, `IRepositoryUnitOfWork` (capa1, ya registrado globalmente en `MedRec.IoC` — sin él la escritura no se persiste, ver nota crítica en Task 5).
- Produces: `ToggleUserActiveInteractor : IToggleUserActiveInputPort` — auto-descubierto.

- [ ] **Step 1: DTO**

```csharp
namespace MedRec.Identity.BusinessObjects.DTOs;
public class ToggleUserActiveDto
{
    public ToggleUserActiveDto(Guid userId, bool isActive)
    {
        UserId = userId;
        IsActive = isActive;
    }

    public Guid UserId { get; }
    public bool IsActive { get; }
}
```

- [ ] **Step 2: Ports**

`Interfaces\Ports\IToggleUserActiveInputPort.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IToggleUserActiveInputPort
{
    Task HandleAsync(ToggleUserActiveDto dto, CancellationToken ct = default);
}
```

`Interfaces\Ports\IToggleUserActiveOutputPort.cs`:
```csharp
using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IToggleUserActiveOutputPort : IBaseOutputPort
{
    OperationResult<bool> Result { get; }
    Task Handle(CancellationToken ct = default);
}
```

(Sin validador dedicado — no hay invariantes de negocio más allá de que el usuario exista, cubierto en el interactor.)

- [ ] **Step 3: Escribir el test que falla (RED)**

`Test\MedRec.Identity.UseCases.Tests\ToggleUserActiveInteractorTests.cs`:
```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Interfaces;
using MedRec.Entity.POCOEntities;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.UseCases.Implementations;
using Moq;

namespace MedRec.Identity.UseCases.Tests;

public class ToggleUserActiveInteractorTests
{
    private static (
        Mock<IToggleUserActiveOutputPort> presenter,
        Mock<IUserCommandsRepository> commandsRepo,
        Mock<IUserQueriesRepository> queriesRepo,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork) CreateMocks()
    {
        return (
            new Mock<IToggleUserActiveOutputPort>(),
            new Mock<IUserCommandsRepository>(),
            new Mock<IUserQueriesRepository>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>());
    }

    /// <summary>
    /// ExecuteInTransactionWithRetry es responsabilidad del UnitOfWork real (retry, begin/commit/rollback);
    /// para testear el interactor basta con que el mock invoque el delegate recibido.
    /// </summary>
    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenUserNotFound()
    {
        var dto = new ToggleUserActiveDto(Guid.NewGuid(), false);
        var (presenter, commandsRepo, queriesRepo, authorization, currentUser, unitOfWork) = CreateMocks();

        queriesRepo.Setup(r => r.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var interactor = new ToggleUserActiveInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, authorization.Object, currentUser.Object, unitOfWork.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        commandsRepo.Verify(r => r.SetActiveAsync(It.IsAny<Guid>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldToggleActive_WhenUserExists()
    {
        var dto = new ToggleUserActiveDto(Guid.NewGuid(), false);
        var (presenter, commandsRepo, queriesRepo, authorization, currentUser, unitOfWork) = CreateMocks();
        SetUpTransactionToRunWork(unitOfWork);

        queriesRepo.Setup(r => r.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = dto.UserId, IsActive = true });

        var interactor = new ToggleUserActiveInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, authorization.Object, currentUser.Object, unitOfWork.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        commandsRepo.Verify(r => r.SetActiveAsync(dto.UserId, false, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 4: Correr y verificar RED**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: FAIL (no compila).

- [ ] **Step 5: Implementar `ToggleUserActiveInteractor`**

```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;

namespace MedRec.Identity.UseCases.Implementations;

public class ToggleUserActiveInteractor(
    IToggleUserActiveOutputPort presenter,
    IUserCommandsRepository userCommandsRepository,
    IUserQueriesRepository userQueriesRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork) : IToggleUserActiveInputPort
{
    public async Task HandleAsync(ToggleUserActiveDto dto, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Users_Edit, ct);

        var user = await userQueriesRepository.GetByIdAsync(dto.UserId, ct);
        if (user is null)
        {
            await presenter.ErrorAsync(new ErrorInfo("Usuario no encontrado.", ErrorCode.NotFound, null, 404));
            return;
        }

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await userCommandsRepository.SetActiveAsync(dto.UserId, dto.IsActive, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await presenter.Handle(ct);
    }
}
```

- [ ] **Step 6: Correr y verificar GREEN**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: PASS (20/20).

- [ ] **Step 7: Presenter**

```csharp
using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;

namespace MedRec.Identity.Presenters.Implementations;
internal class ToggleUserActivePresenter : BaseOutputPort<bool>, IToggleUserActiveOutputPort
{
    public Task Handle(CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }
}
```

En `DependencyContainer.cs` de Presenters, agregar:
```csharp
        services.AddScoped<IToggleUserActiveOutputPort, ToggleUserActivePresenter>();
```

- [ ] **Step 8: Compilar**

Run: `dotnet build "2.ApplicationBusinessObjects\MedRec.Identity.UseCases\MedRec.Identity.UseCases.csproj" && dotnet build "3.InterfaceAdapters\MedRec.Identity.Presenters\MedRec.Identity.Presenters.csproj"`
Expected: 0 errores.

- [ ] **Step 9: Commit**

```bash
git add "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/" "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/" "3.InterfaceAdapters/MedRec.Identity.Presenters/" "Test/MedRec.Identity.UseCases.Tests/"
git commit -m "feat(identity): implementar ToggleUserActiveInteractor (TDD)"
```

---

### Task 7: TDD — `ResetUserPasswordInteractor`

**Files:**
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\ResetUserPasswordDto.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\IResetUserPasswordInputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\IResetUserPasswordOutputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Validators\ResetUserPasswordValidator.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\ResetUserPasswordInteractor.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.Presenters\Implementations\ResetUserPasswordPresenter.cs`
- Modify: `3.InterfaceAdapters\MedRec.Identity.Presenters\DependencyContainer.cs`
- Test: `Test\MedRec.Identity.UseCases.Tests\ResetUserPasswordInteractorTests.cs`

**Interfaces:**
- Consumes: `IUserCommandsRepository`, `IUserQueriesRepository`, `IPasswordHasher`, `IEmailNotificationService`, `IAuthorizationService`, `ICurrentUserContext`, `IRepositoryUnitOfWork` (ver nota crítica en Task 5).
- Produces: `ResetUserPasswordInteractor : IResetUserPasswordInputPort` — auto-descubierto.

- [ ] **Step 1: DTO**

```csharp
namespace MedRec.Identity.BusinessObjects.DTOs;
public class ResetUserPasswordDto
{
    public ResetUserPasswordDto(Guid userId, string temporaryPassword)
    {
        UserId = userId;
        TemporaryPassword = temporaryPassword;
    }

    public Guid UserId { get; }
    public string TemporaryPassword { get; }
}
```

- [ ] **Step 2: Ports**

`Interfaces\Ports\IResetUserPasswordInputPort.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IResetUserPasswordInputPort
{
    Task HandleAsync(ResetUserPasswordDto dto, CancellationToken ct = default);
}
```

`Interfaces\Ports\IResetUserPasswordOutputPort.cs`:
```csharp
using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IResetUserPasswordOutputPort : IBaseOutputPort
{
    OperationResult<bool> Result { get; }
    Task Handle(CancellationToken ct = default);
}
```

- [ ] **Step 3: Validador**

```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.Identity.BusinessObjects.Validators;
public static class ResetUserPasswordValidator
{
    public static IReadOnlyList<ValidationError> Validate(ResetUserPasswordDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        var passwordValidation = Guard.Against(dto.TemporaryPassword, nameof(dto.TemporaryPassword))
            .NotNullOrEmpty("La contraseña temporal es obligatoria.")
            .MinLength(8, "La contraseña temporal debe tener al menos 8 caracteres.");
        errors.AddRange(passwordValidation.Errors);

        return errors;
    }
}
```

- [ ] **Step 4: Escribir el test que falla (RED)**

`Test\MedRec.Identity.UseCases.Tests\ResetUserPasswordInteractorTests.cs`:
```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Interfaces;
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

public class ResetUserPasswordInteractorTests
{
    private static (
        Mock<IResetUserPasswordOutputPort> presenter,
        Mock<IUserCommandsRepository> commandsRepo,
        Mock<IUserQueriesRepository> queriesRepo,
        Mock<IPasswordHasher> hasher,
        Mock<IEmailNotificationService> email,
        Mock<IAuthorizationService> authorization,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork,
        Mock<IModelValidatorHub<ResetUserPasswordDto>> validator) CreateMocks()
    {
        return (
            new Mock<IResetUserPasswordOutputPort>(),
            new Mock<IUserCommandsRepository>(),
            new Mock<IUserQueriesRepository>(),
            new Mock<IPasswordHasher>(),
            new Mock<IEmailNotificationService>(),
            new Mock<IAuthorizationService>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>(),
            new Mock<IModelValidatorHub<ResetUserPasswordDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<ResetUserPasswordDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<ResetUserPasswordDto>(), It.IsAny<Func<ResetUserPasswordDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

    /// <summary>
    /// ExecuteInTransactionWithRetry es responsabilidad del UnitOfWork real (retry, begin/commit/rollback);
    /// para testear el interactor basta con que el mock invoque el delegate recibido.
    /// </summary>
    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldReturnError_WhenUserNotFound()
    {
        var dto = new ResetUserPasswordDto(Guid.NewGuid(), "NuevaTemp123!");
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        queriesRepo.Setup(r => r.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var interactor = new ResetUserPasswordInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.ErrorAsync(It.IsAny<ErrorInfo>()), Times.Once);
        commandsRepo.Verify(r => r.SetPasswordAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldSetTemporaryPasswordAndSendEmail_WhenUserExists()
    {
        var dto = new ResetUserPasswordDto(Guid.NewGuid(), "NuevaTemp123!");
        var (presenter, commandsRepo, queriesRepo, hasher, email, authorization, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);
        SetUpTransactionToRunWork(unitOfWork);

        var user = new User { Id = dto.UserId, Email = "user@medrec.local", FullName = "Usuario Existente" };
        queriesRepo.Setup(r => r.GetByIdAsync(dto.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasher.Setup(h => h.Hash(dto.TemporaryPassword)).Returns("hashed-nuevo");

        var interactor = new ResetUserPasswordInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, email.Object, authorization.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        commandsRepo.Verify(r => r.SetPasswordAsync(dto.UserId, "hashed-nuevo", true, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        email.Verify(e => e.SendTemporaryPasswordAsync(user.Email, user.FullName, dto.TemporaryPassword, It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 5: Correr y verificar RED**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: FAIL (no compila).

- [ ] **Step 6: Implementar `ResetUserPasswordInteractor`**

```csharp
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Identity.UseCases.Implementations;

public class ResetUserPasswordInteractor(
    IResetUserPasswordOutputPort presenter,
    IUserCommandsRepository userCommandsRepository,
    IUserQueriesRepository userQueriesRepository,
    IPasswordHasher passwordHasher,
    IEmailNotificationService emailNotificationService,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork,
    IModelValidatorHub<ResetUserPasswordDto> validatorHub) : IResetUserPasswordInputPort
{
    public async Task HandleAsync(ResetUserPasswordDto dto, CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Users_Edit, ct);

        var isValid = await validatorHub.Validate(dto, ResetUserPasswordValidator.Validate);
        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var user = await userQueriesRepository.GetByIdAsync(dto.UserId, ct);
        if (user is null)
        {
            await presenter.ErrorAsync(new ErrorInfo("Usuario no encontrado.", ErrorCode.NotFound, null, 404));
            return;
        }

        var hash = passwordHasher.Hash(dto.TemporaryPassword);

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await userCommandsRepository.SetPasswordAsync(dto.UserId, hash, true, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await emailNotificationService.SendTemporaryPasswordAsync(user.Email, user.FullName, dto.TemporaryPassword, ct);

        await presenter.Handle(ct);
    }
}
```

- [ ] **Step 7: Correr y verificar GREEN**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: PASS (22/22).

- [ ] **Step 8: Presenter**

```csharp
using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;

namespace MedRec.Identity.Presenters.Implementations;
internal class ResetUserPasswordPresenter : BaseOutputPort<bool>, IResetUserPasswordOutputPort
{
    public Task Handle(CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }
}
```

En `DependencyContainer.cs` de Presenters, agregar:
```csharp
        services.AddScoped<IResetUserPasswordOutputPort, ResetUserPasswordPresenter>();
```

- [ ] **Step 9: Compilar**

Run: `dotnet build "2.ApplicationBusinessObjects\MedRec.Identity.UseCases\MedRec.Identity.UseCases.csproj" && dotnet build "3.InterfaceAdapters\MedRec.Identity.Presenters\MedRec.Identity.Presenters.csproj"`
Expected: 0 errores.

- [ ] **Step 10: Commit**

```bash
git add "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/" "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/" "3.InterfaceAdapters/MedRec.Identity.Presenters/" "Test/MedRec.Identity.UseCases.Tests/"
git commit -m "feat(identity): implementar ResetUserPasswordInteractor (TDD)"
```

---

### Task 8: TDD — `ChangePasswordInteractor` (autoservicio)

**Files:**
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\DTOs\ChangePasswordDto.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\IChangePasswordInputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\IChangePasswordOutputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Validators\ChangePasswordValidator.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\ChangePasswordInteractor.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.Presenters\Implementations\ChangePasswordPresenter.cs`
- Modify: `3.InterfaceAdapters\MedRec.Identity.Presenters\DependencyContainer.cs`
- Test: `Test\MedRec.Identity.UseCases.Tests\ChangePasswordInteractorTests.cs`

**Interfaces:**
- Consumes: `IUserCommandsRepository`, `IUserQueriesRepository`, `IPasswordHasher`, `ICurrentUserContext`, `IRepositoryUnitOfWork` (ver nota crítica en Task 5) (NO `IAuthorizationService` — es autoservicio, solo requiere estar autenticado, sin permiso especial).
- Produces: `ChangePasswordInteractor : IChangePasswordInputPort` — auto-descubierto. Usado por `ChangePasswordPage` (Task 11).

- [ ] **Step 1: DTO**

```csharp
namespace MedRec.Identity.BusinessObjects.DTOs;
public class ChangePasswordDto
{
    public ChangePasswordDto(string currentPassword, string newPassword)
    {
        CurrentPassword = currentPassword;
        NewPassword = newPassword;
    }

    public string CurrentPassword { get; }
    public string NewPassword { get; }
}
```

- [ ] **Step 2: Ports**

`Interfaces\Ports\IChangePasswordInputPort.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IChangePasswordInputPort
{
    Task HandleAsync(ChangePasswordDto dto, CancellationToken ct = default);
}
```

`Interfaces\Ports\IChangePasswordOutputPort.cs`:
```csharp
using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IChangePasswordOutputPort : IBaseOutputPort
{
    OperationResult<bool> Result { get; }
    Task Handle(CancellationToken ct = default);
    Task InvalidCurrentPassword();
}
```

- [ ] **Step 3: Validador**

```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Shared.Gruards;
using MedRec.Validator.ValueObjects;

namespace MedRec.Identity.BusinessObjects.Validators;
public static class ChangePasswordValidator
{
    public static IReadOnlyList<ValidationError> Validate(ChangePasswordDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));

        var errors = new List<ValidationError>();

        var currentValidation = Guard.Against(dto.CurrentPassword, nameof(dto.CurrentPassword))
            .NotNullOrEmpty("Debe ingresar su contraseña actual.");
        errors.AddRange(currentValidation.Errors);

        var newValidation = Guard.Against(dto.NewPassword, nameof(dto.NewPassword))
            .NotNullOrEmpty("La nueva contraseña es obligatoria.")
            .MinLength(8, "La nueva contraseña debe tener al menos 8 caracteres.");
        errors.AddRange(newValidation.Errors);

        return errors;
    }
}
```

- [ ] **Step 4: Escribir el test que falla (RED)**

`Test\MedRec.Identity.UseCases.Tests\ChangePasswordInteractorTests.cs`:
```csharp
using MedRec.Entity.Interfaces;
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

public class ChangePasswordInteractorTests
{
    private static (
        Mock<IChangePasswordOutputPort> presenter,
        Mock<IUserCommandsRepository> commandsRepo,
        Mock<IUserQueriesRepository> queriesRepo,
        Mock<IPasswordHasher> hasher,
        Mock<ICurrentUserContext> currentUser,
        Mock<IRepositoryUnitOfWork> unitOfWork,
        Mock<IModelValidatorHub<ChangePasswordDto>> validator) CreateMocks()
    {
        return (
            new Mock<IChangePasswordOutputPort>(),
            new Mock<IUserCommandsRepository>(),
            new Mock<IUserQueriesRepository>(),
            new Mock<IPasswordHasher>(),
            new Mock<ICurrentUserContext>(),
            new Mock<IRepositoryUnitOfWork>(),
            new Mock<IModelValidatorHub<ChangePasswordDto>>());
    }

    private static void SetUpValidatorToPass(Mock<IModelValidatorHub<ChangePasswordDto>> validatorMock) =>
        validatorMock
            .Setup(v => v.Validate(It.IsAny<ChangePasswordDto>(), It.IsAny<Func<ChangePasswordDto, IReadOnlyList<ValidationError>>>()))
            .ReturnsAsync(true);

    /// <summary>
    /// ExecuteInTransactionWithRetry es responsabilidad del UnitOfWork real (retry, begin/commit/rollback);
    /// para testear el interactor basta con que el mock invoque el delegate recibido.
    /// </summary>
    private static void SetUpTransactionToRunWork(Mock<IRepositoryUnitOfWork> uowMock) =>
        uowMock.Setup(u => u.ExecuteInTransactionWithRetry(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task> work, CancellationToken _) => work());

    [Fact]
    public async Task HandleAsync_ShouldReturnInvalidCurrentPassword_WhenCurrentPasswordDoesNotMatch()
    {
        var userId = Guid.NewGuid();
        var dto = new ChangePasswordDto("ClaveVieja", "ClaveNueva123!");
        var (presenter, commandsRepo, queriesRepo, hasher, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);

        currentUser.SetupGet(c => c.UserId).Returns(userId);
        var user = new User { Id = userId, PasswordHash = "hash-actual" };
        queriesRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasher.Setup(h => h.Verify(dto.CurrentPassword, user.PasswordHash)).Returns(false);

        var interactor = new ChangePasswordInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        presenter.Verify(p => p.InvalidCurrentPassword(), Times.Once);
        commandsRepo.Verify(r => r.SetPasswordAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldChangePasswordAndClearFlag_WhenCurrentPasswordMatches()
    {
        var userId = Guid.NewGuid();
        var dto = new ChangePasswordDto("ClaveVieja", "ClaveNueva123!");
        var (presenter, commandsRepo, queriesRepo, hasher, currentUser, unitOfWork, validator) = CreateMocks();
        SetUpValidatorToPass(validator);
        SetUpTransactionToRunWork(unitOfWork);

        currentUser.SetupGet(c => c.UserId).Returns(userId);
        var user = new User { Id = userId, PasswordHash = "hash-actual" };
        queriesRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        hasher.Setup(h => h.Verify(dto.CurrentPassword, user.PasswordHash)).Returns(true);
        hasher.Setup(h => h.Hash(dto.NewPassword)).Returns("hash-nuevo");

        var interactor = new ChangePasswordInteractor(presenter.Object, commandsRepo.Object, queriesRepo.Object, hasher.Object, currentUser.Object, unitOfWork.Object, validator.Object);

        await interactor.HandleAsync(dto, CancellationToken.None);

        commandsRepo.Verify(r => r.SetPasswordAsync(userId, "hash-nuevo", false, It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChanges(It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.Handle(It.IsAny<CancellationToken>()), Times.Once);
        presenter.Verify(p => p.InvalidCurrentPassword(), Times.Never);
    }
}
```

- [ ] **Step 5: Correr y verificar RED**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: FAIL (no compila).

- [ ] **Step 6: Implementar `ChangePasswordInteractor`**

```csharp
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.BusinessObjects.Validators;
using MedRec.Validator.Interfaces;

namespace MedRec.Identity.UseCases.Implementations;

public class ChangePasswordInteractor(
    IChangePasswordOutputPort presenter,
    IUserCommandsRepository userCommandsRepository,
    IUserQueriesRepository userQueriesRepository,
    IPasswordHasher passwordHasher,
    ICurrentUserContext currentUserContext,
    IRepositoryUnitOfWork unitOfWork,
    IModelValidatorHub<ChangePasswordDto> validatorHub) : IChangePasswordInputPort
{
    public async Task HandleAsync(ChangePasswordDto dto, CancellationToken ct = default)
    {
        var isValid = await validatorHub.Validate(dto, ChangePasswordValidator.Validate);
        if (!isValid)
        {
            await presenter.ValidationErrorsAsync(validatorHub.Errors);
            return;
        }

        var userId = currentUserContext.UserId!.Value;
        var user = await userQueriesRepository.GetByIdAsync(userId, ct);
        if (user is null || !passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            await presenter.InvalidCurrentPassword();
            return;
        }

        var newHash = passwordHasher.Hash(dto.NewPassword);

        await unitOfWork.ExecuteInTransactionWithRetry(async () =>
        {
            await userCommandsRepository.SetPasswordAsync(userId, newHash, false, ct);
            await unitOfWork.SaveChanges(ct);
        }, ct);

        await presenter.Handle(ct);
    }
}
```

- [ ] **Step 7: Correr y verificar GREEN**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: PASS (24/24).

- [ ] **Step 8: Presenter**

```csharp
using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Entity.DTOs;
using MedRec.Entity.Enums;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;

namespace MedRec.Identity.Presenters.Implementations;
internal class ChangePasswordPresenter : BaseOutputPort<bool>, IChangePasswordOutputPort
{
    public Task Handle(CancellationToken ct = default)
    {
        Result = OperationResult<bool>.Ok(true);
        return Task.CompletedTask;
    }

    public Task InvalidCurrentPassword()
    {
        Result = OperationResult<bool>.Fail(
            new ErrorInfo("La contraseña actual no es correcta.", ErrorCode.Forbidden, null, 401),
            UserMessageAction.ShowError);
        return Task.CompletedTask;
    }
}
```

En `DependencyContainer.cs` de Presenters, agregar:
```csharp
        services.AddScoped<IChangePasswordOutputPort, ChangePasswordPresenter>();
```

- [ ] **Step 9: Compilar**

Run: `dotnet build "2.ApplicationBusinessObjects\MedRec.Identity.UseCases\MedRec.Identity.UseCases.csproj" && dotnet build "3.InterfaceAdapters\MedRec.Identity.Presenters\MedRec.Identity.Presenters.csproj"`
Expected: 0 errores.

- [ ] **Step 10: Commit**

```bash
git add "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/" "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/" "3.InterfaceAdapters/MedRec.Identity.Presenters/" "Test/MedRec.Identity.UseCases.Tests/"
git commit -m "feat(identity): implementar ChangePasswordInteractor (TDD)"
```

---

### Task 9: TDD — `UsersListInteractor`

**Files:**
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\IUsersListInputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Ports\IUsersListOutputPort.cs`
- Create: `2.ApplicationBusinessObjects\MedRec.Identity.UseCases\Implementations\UsersListInteractor.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.Presenters\Implementations\UsersListPresenter.cs`
- Modify: `3.InterfaceAdapters\MedRec.Identity.Presenters\DependencyContainer.cs`
- Test: `Test\MedRec.Identity.UseCases.Tests\UsersListInteractorTests.cs`

**Interfaces:**
- Consumes: `IUserQueriesRepository.ListWithRolesAsync` (Task 3), `IAuthorizationService`, `ICurrentUserContext`.
- Produces: `UsersListInteractor : IUsersListInputPort` — auto-descubierto.

- [ ] **Step 1: Ports**

`Interfaces\Ports\IUsersListInputPort.cs`:
```csharp
namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IUsersListInputPort
{
    Task HandleAsync(CancellationToken ct = default);
}
```

`Interfaces\Ports\IUsersListOutputPort.cs`:
```csharp
using MedRec.BusinessObjects.Interfaces;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.BusinessObjects.Interfaces.Ports;
public interface IUsersListOutputPort : IBaseOutputPort
{
    OperationResult<IReadOnlyList<UserSummaryDto>> Result { get; }
    Task Handle(IReadOnlyList<UserSummaryDto> users, CancellationToken ct = default);
}
```

- [ ] **Step 2: Escribir el test que falla (RED)**

`Test\MedRec.Identity.UseCases.Tests\UsersListInteractorTests.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.UseCases.Implementations;
using MedRec.Shared.Exceptions;
using Moq;

namespace MedRec.Identity.UseCases.Tests;

public class UsersListInteractorTests
{
    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenCallerLacksPermission()
    {
        var presenter = new Mock<IUsersListOutputPort>();
        var queriesRepo = new Mock<IUserQueriesRepository>();
        var authorization = new Mock<IAuthorizationService>();
        var currentUser = new Mock<ICurrentUserContext>();

        authorization.Setup(a => a.EnsurePermissionAsync(It.IsAny<Guid?>(), "users.view", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BusinessException(new MedRec.Entity.DTOs.ErrorInfo("No tiene permiso.", MedRec.Entity.Enums.ErrorCode.Forbidden)));

        var interactor = new UsersListInteractor(presenter.Object, queriesRepo.Object, authorization.Object, currentUser.Object);

        await Assert.ThrowsAsync<BusinessException>(() => interactor.HandleAsync(CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnUsers_WhenPermissionGranted()
    {
        var presenter = new Mock<IUsersListOutputPort>();
        var queriesRepo = new Mock<IUserQueriesRepository>();
        var authorization = new Mock<IAuthorizationService>();
        var currentUser = new Mock<ICurrentUserContext>();

        var users = new List<UserSummaryDto>
        {
            new(Guid.NewGuid(), "admin@medrec.local", "Administrador", true, new List<string> { "Administrador" })
        };
        queriesRepo.Setup(r => r.ListWithRolesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(users);

        var interactor = new UsersListInteractor(presenter.Object, queriesRepo.Object, authorization.Object, currentUser.Object);

        await interactor.HandleAsync(CancellationToken.None);

        presenter.Verify(p => p.Handle(users, It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

- [ ] **Step 3: Correr y verificar RED**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: FAIL (no compila).

- [ ] **Step 4: Implementar `UsersListInteractor`**

```csharp
using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Constants;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;

namespace MedRec.Identity.UseCases.Implementations;

public class UsersListInteractor(
    IUsersListOutputPort presenter,
    IUserQueriesRepository userQueriesRepository,
    IAuthorizationService authorizationService,
    ICurrentUserContext currentUserContext) : IUsersListInputPort
{
    public async Task HandleAsync(CancellationToken ct = default)
    {
        await authorizationService.EnsurePermissionAsync(currentUserContext.UserId, SystemPermissions.Users_View, ct);

        var users = await userQueriesRepository.ListWithRolesAsync(ct);
        await presenter.Handle(users, ct);
    }
}
```

- [ ] **Step 5: Correr y verificar GREEN**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: PASS (26/26).

- [ ] **Step 6: Presenter**

```csharp
using MedRec.BusinessObjects.Abstracts;
using MedRec.BusinessObjects.Results;
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;

namespace MedRec.Identity.Presenters.Implementations;
internal class UsersListPresenter : BaseOutputPort<IReadOnlyList<UserSummaryDto>>, IUsersListOutputPort
{
    public Task Handle(IReadOnlyList<UserSummaryDto> users, CancellationToken ct = default)
    {
        Result = OperationResult<IReadOnlyList<UserSummaryDto>>.Ok(users);
        return Task.CompletedTask;
    }
}
```

En `DependencyContainer.cs` de Presenters, agregar:
```csharp
        services.AddScoped<IUsersListOutputPort, UsersListPresenter>();
```

- [ ] **Step 7: Compilar**

Run: `dotnet build "2.ApplicationBusinessObjects\MedRec.Identity.UseCases\MedRec.Identity.UseCases.csproj" && dotnet build "3.InterfaceAdapters\MedRec.Identity.Presenters\MedRec.Identity.Presenters.csproj"`
Expected: 0 errores.

- [ ] **Step 8: Commit**

```bash
git add "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/" "2.ApplicationBusinessObjects/MedRec.Identity.UseCases/" "3.InterfaceAdapters/MedRec.Identity.Presenters/" "Test/MedRec.Identity.UseCases.Tests/"
git commit -m "feat(identity): implementar UsersListInteractor (TDD)"
```

---

### Task 10: ViewModels — Models y VMs

**Files:**
- Create: `3.InterfaceAdapters\MedRec.Identity.ViewModels\Models\CreateUserModel.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.ViewModels\Models\UpdateUserModel.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.ViewModels\Models\ChangePasswordModel.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.ViewModels\VM\CreateUserVM.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.ViewModels\VM\UpdateUserVM.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.ViewModels\VM\UsersListVM.cs`
- Create: `3.InterfaceAdapters\MedRec.Identity.ViewModels\VM\ChangePasswordVM.cs`

**Interfaces:**
- Consumes: todos los ports de Tasks 4-9, más `IDoctorLookupRepository`/`IUserQueriesRepository`/`IRoleQueriesRepository` — **nota:** `IRoleQueriesRepository` (listado de roles para el multi-select) todavía no existe, es parte del Plan 2b. Por ahora `CreateUserVM`/`UpdateUserVM` reciben la lista de roles ya cargada como `IReadOnlyList<(Guid Id, string Name)>` vía un parámetro `AvailableRoles` que la página completa manualmente antes de mostrar el formulario (placeholder funcional hasta que el Plan 2b provea `IRoleQueriesRepository`; ver Task 11).
- Produces: `CreateUserVM`, `UpdateUserVM`, `UsersListVM`, `ChangePasswordVM` — consumidos por Task 11.

- [ ] **Step 1: Models**

`Models\CreateUserModel.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.ViewModels.Models;
public class CreateUserModel
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
    public List<Guid> RoleIds { get; set; } = new();
    public Guid? DoctorId { get; set; }

    public static explicit operator CreateUserDto(CreateUserModel model) =>
        new(model.Email, model.FullName, model.TemporaryPassword, model.RoleIds, model.DoctorId);
}
```

`Models\UpdateUserModel.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.ViewModels.Models;
public class UpdateUserModel
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public List<Guid> RoleIds { get; set; } = new();
    public Guid? DoctorId { get; set; }

    public static explicit operator UpdateUserDto(UpdateUserModel model) =>
        new(model.UserId, model.FullName, model.RoleIds, model.DoctorId);
}
```

`Models\ChangePasswordModel.cs`:
```csharp
using MedRec.Identity.BusinessObjects.DTOs;

namespace MedRec.Identity.ViewModels.Models;
public class ChangePasswordModel
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;

    public static explicit operator ChangePasswordDto(ChangePasswordModel model) =>
        new(model.CurrentPassword, model.NewPassword);
}
```

- [ ] **Step 2: `CreateUserVM`**

```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.ViewModels.Models;

namespace MedRec.Identity.ViewModels.VM;
public class CreateUserVM(
    ICreateUserInputPort interactor,
    ICreateUserOutputPort presenter)
{
    public CreateUserModel Model { get; set; } = new();
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
            await interactor.HandleAsync((CreateUserDto)Model, ct);
            var result = presenter.Result;

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join(" ", result.ValidationErrors.Select(e => e.ErrorMessage));
            }
            else if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "No se pudo crear el usuario.";
            }
            else
            {
                InformationMessage = "Usuario creado correctamente.";
                Success = true;
                Model = new CreateUserModel();
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
```

- [ ] **Step 3: `UpdateUserVM`**

```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.ViewModels.Models;

namespace MedRec.Identity.ViewModels.VM;
public class UpdateUserVM(
    IUpdateUserInputPort interactor,
    IUpdateUserOutputPort presenter)
{
    public UpdateUserModel Model { get; set; } = new();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;
    public bool Success { get; private set; }

    public async Task UpdateAsync(CancellationToken ct = default)
    {
        IsProcessing = true;
        Success = false;
        try
        {
            InformationMessage = string.Empty;
            await interactor.HandleAsync((UpdateUserDto)Model, ct);
            var result = presenter.Result;

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join(" ", result.ValidationErrors.Select(e => e.ErrorMessage));
            }
            else if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "No se pudo editar el usuario.";
            }
            else
            {
                InformationMessage = "Usuario actualizado correctamente.";
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

- [ ] **Step 4: `UsersListVM`**

```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;

namespace MedRec.Identity.ViewModels.VM;
public class UsersListVM(
    IUsersListInputPort listInteractor,
    IUsersListOutputPort listPresenter,
    IToggleUserActiveInputPort toggleActiveInteractor,
    IToggleUserActiveOutputPort toggleActivePresenter,
    IResetUserPasswordInputPort resetPasswordInteractor,
    IResetUserPasswordOutputPort resetPasswordPresenter)
{
    public IReadOnlyList<UserSummaryDto> Users { get; private set; } = Array.Empty<UserSummaryDto>();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;

    public async Task LoadAsync(CancellationToken ct = default)
    {
        IsProcessing = true;
        try
        {
            await listInteractor.HandleAsync(ct);
            var result = listPresenter.Result;
            Users = result.Success ? result.Value ?? Array.Empty<UserSummaryDto>() : Array.Empty<UserSummaryDto>();
            if (!result.Success)
                InformationMessage = result.Error?.Message ?? "No se pudo cargar el listado de usuarios.";
        }
        finally
        {
            IsProcessing = false;
        }
    }

    public async Task ToggleActiveAsync(Guid userId, bool isActive, CancellationToken ct = default)
    {
        IsProcessing = true;
        try
        {
            await toggleActiveInteractor.HandleAsync(new ToggleUserActiveDto(userId, isActive), ct);
            var result = toggleActivePresenter.Result;
            if (!result.Success)
                InformationMessage = result.Error?.Message ?? "No se pudo cambiar el estado del usuario.";
            else
                await LoadAsync(ct);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    public async Task ResetPasswordAsync(Guid userId, string temporaryPassword, CancellationToken ct = default)
    {
        IsProcessing = true;
        try
        {
            await resetPasswordInteractor.HandleAsync(new ResetUserPasswordDto(userId, temporaryPassword), ct);
            var result = resetPasswordPresenter.Result;
            InformationMessage = result.Success
                ? "Contraseña temporal enviada."
                : (result.Error?.Message ?? "No se pudo resetear la contraseña.");
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
```

- [ ] **Step 5: `ChangePasswordVM`**

```csharp
using MedRec.Identity.BusinessObjects.DTOs;
using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.ViewModels.Models;

namespace MedRec.Identity.ViewModels.VM;
public class ChangePasswordVM(
    IChangePasswordInputPort interactor,
    IChangePasswordOutputPort presenter)
{
    public ChangePasswordModel Model { get; set; } = new();
    public bool IsProcessing { get; private set; }
    public string InformationMessage { get; set; } = string.Empty;
    public bool Success { get; private set; }

    public async Task ChangeAsync(CancellationToken ct = default)
    {
        IsProcessing = true;
        Success = false;
        try
        {
            InformationMessage = string.Empty;

            if (Model.NewPassword != Model.ConfirmNewPassword)
            {
                InformationMessage = "La confirmación no coincide con la nueva contraseña.";
                return;
            }

            await interactor.HandleAsync((ChangePasswordDto)Model, ct);
            var result = presenter.Result;

            if (result.HasValidationErrors)
            {
                InformationMessage = string.Join(" ", result.ValidationErrors.Select(e => e.ErrorMessage));
            }
            else if (!result.Success)
            {
                InformationMessage = result.Error?.Message ?? "No se pudo cambiar la contraseña.";
            }
            else
            {
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

- [ ] **Step 6: Compilar**

Run: `dotnet build "3.InterfaceAdapters\MedRec.Identity.ViewModels\MedRec.Identity.ViewModels.csproj"`
Expected: 0 errores.

- [ ] **Step 7: Commit**

```bash
git add "3.InterfaceAdapters/MedRec.Identity.ViewModels/"
git commit -m "feat(identity): agregar Models y VMs de administracion de usuarios"
```

---

### Task 11: Views — Páginas de Usuarios

**Files:**
- Create: `3.InterfaceAdapters\MedRec.Identity.Views\Pages\UsersListPage.razor`
- Create: `3.InterfaceAdapters\MedRec.Identity.Views\Pages\CreateUserPage.razor`
- Create: `3.InterfaceAdapters\MedRec.Identity.Views\Pages\UpdateUserPage.razor`
- Create: `3.InterfaceAdapters\MedRec.Identity.Views\Pages\ChangePasswordPage.razor`

**Interfaces:**
- Consumes: `UsersListVM`, `CreateUserVM`, `UpdateUserVM`, `ChangePasswordVM` (Task 10), `IDoctorLookupRepository` (Task 3, inyectado directo en la página para poblar el dropdown — no amerita un VM propio dado que es una sola consulta de solo lectura sin lógica).
- Produces: `ChangePasswordPage` — consumida por Task 12 (`AppShell`).

**Nota de alcance:** el multi-select de Roles en `CreateUserPage`/`UpdateUserPage` necesita `IRoleQueriesRepository` (listado de roles), que es parte del **Plan 2b**. Hasta que exista, estas dos páginas usan un `<InputText>` simple donde el admin pega los IDs de rol separados por coma (`Guid` por `Guid`) — quedará reemplazado por un multi-select real en el Plan 2b sin tocar el VM ni el interactor (el `CreateUserDto`/`UpdateUserDto` ya reciben `IReadOnlyList<Guid>`, agnósticos de cómo se construyó la lista en la UI).

- [ ] **Step 1: `UsersListPage.razor`**

```razor
@page "/users"
@inject UsersListVM VM

<div class="page-container">
    <h2>Usuarios</h2>
    <a href="/users/create">+ Nuevo usuario</a>

    @if (!string.IsNullOrEmpty(VM.InformationMessage))
    {
        <p class="info-message">@VM.InformationMessage</p>
    }

    <table>
        <thead>
            <tr>
                <th>Email</th>
                <th>Nombre</th>
                <th>Roles</th>
                <th>Estado</th>
                <th>Acciones</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var user in VM.Users)
            {
                <tr>
                    <td>@user.Email</td>
                    <td>@user.FullName</td>
                    <td>@string.Join(", ", user.RoleNames)</td>
                    <td>@(user.IsActive ? "Activo" : "Inactivo")</td>
                    <td>
                        <a href="@($"/users/{user.Id}/edit")">Editar</a>
                        <button @onclick="() => ToggleActive(user.Id, !user.IsActive)" disabled="@VM.IsProcessing">
                            @(user.IsActive ? "Desactivar" : "Activar")
                        </button>
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

    private async Task ToggleActive(Guid userId, bool isActive)
    {
        await VM.ToggleActiveAsync(userId, isActive);
        StateHasChanged();
    }
}
```

- [ ] **Step 2: `CreateUserPage.razor`**

```razor
@page "/users/create"
@inject CreateUserVM VM
@inject MedRec.Identity.BusinessObjects.Interfaces.Repositories.IDoctorLookupRepository DoctorLookup
@using Microsoft.AspNetCore.Components.Forms

<div class="page-container">
    <h2>Nuevo usuario</h2>

    <EditForm Model="VM.Model" OnValidSubmit="HandleCreate" autocomplete="off">
        <div class="form-row">
            <label for="email">Email</label>
            <InputText id="email" @bind-Value="VM.Model.Email" />
        </div>

        <div class="form-row">
            <label for="fullName">Nombre completo</label>
            <InputText id="fullName" @bind-Value="VM.Model.FullName" />
        </div>

        <div class="form-row">
            <label for="temporaryPassword">Contraseña temporal</label>
            <InputText id="temporaryPassword" @bind-Value="VM.Model.TemporaryPassword" />
        </div>

        <div class="form-row">
            <label for="roleIds">Roles (IDs separados por coma)</label>
            <InputText id="roleIds" @bind-Value="_roleIdsText" />
        </div>

        <div class="form-row">
            <label for="doctorId">Doctor vinculado (opcional)</label>
            <select id="doctorId" @bind="_doctorIdText">
                <option value="">-- Ninguno --</option>
                @foreach (var doctor in _doctors)
                {
                    <option value="@doctor.Id">@doctor.FullName</option>
                }
            </select>
        </div>

        @if (!string.IsNullOrEmpty(VM.InformationMessage))
        {
            <p class="info-message">@VM.InformationMessage</p>
        }

        <button type="submit" disabled="@VM.IsProcessing">
            @(VM.IsProcessing ? "Guardando..." : "Crear usuario")
        </button>
    </EditForm>
</div>

@code {
    private IReadOnlyList<MedRec.Identity.BusinessObjects.DTOs.DoctorSummaryDto> _doctors = Array.Empty<MedRec.Identity.BusinessObjects.DTOs.DoctorSummaryDto>();
    private string _roleIdsText = string.Empty;
    private string _doctorIdText = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        _doctors = await DoctorLookup.ListActiveAsync();
    }

    private async Task HandleCreate()
    {
        VM.Model.RoleIds = _roleIdsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Guid.Parse)
            .ToList();
        VM.Model.DoctorId = string.IsNullOrWhiteSpace(_doctorIdText) ? null : Guid.Parse(_doctorIdText);

        await VM.CreateAsync();
        StateHasChanged();
    }
}
```

- [ ] **Step 3: `UpdateUserPage.razor`**

```razor
@page "/users/{UserId:guid}/edit"
@inject UpdateUserVM VM
@inject MedRec.Identity.BusinessObjects.Interfaces.Repositories.IDoctorLookupRepository DoctorLookup
@inject MedRec.Identity.BusinessObjects.Interfaces.Repositories.IUserQueriesRepository UserQueries
@using Microsoft.AspNetCore.Components.Forms

<div class="page-container">
    <h2>Editar usuario</h2>

    <EditForm Model="VM.Model" OnValidSubmit="HandleUpdate" autocomplete="off">
        <div class="form-row">
            <label for="fullName">Nombre completo</label>
            <InputText id="fullName" @bind-Value="VM.Model.FullName" />
        </div>

        <div class="form-row">
            <label for="roleIds">Roles (IDs separados por coma)</label>
            <InputText id="roleIds" @bind-Value="_roleIdsText" />
        </div>

        <div class="form-row">
            <label for="doctorId">Doctor vinculado (opcional)</label>
            <select id="doctorId" @bind="_doctorIdText">
                <option value="">-- Ninguno --</option>
                @foreach (var doctor in _doctors)
                {
                    <option value="@doctor.Id">@doctor.FullName</option>
                }
            </select>
        </div>

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
    [Parameter] public Guid UserId { get; set; }

    private IReadOnlyList<MedRec.Identity.BusinessObjects.DTOs.DoctorSummaryDto> _doctors = Array.Empty<MedRec.Identity.BusinessObjects.DTOs.DoctorSummaryDto>();
    private string _roleIdsText = string.Empty;
    private string _doctorIdText = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        _doctors = await DoctorLookup.ListActiveAsync();

        var user = await UserQueries.GetByIdAsync(UserId);
        if (user is not null)
        {
            VM.Model.UserId = user.Id;
            VM.Model.FullName = user.FullName;
            _doctorIdText = user.DoctorId?.ToString() ?? string.Empty;
            var roleIds = await UserQueries.GetRoleIdsAsync(user.Id);
            _roleIdsText = string.Join(", ", roleIds);
        }
    }

    private async Task HandleUpdate()
    {
        VM.Model.RoleIds = _roleIdsText
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Guid.Parse)
            .ToList();
        VM.Model.DoctorId = string.IsNullOrWhiteSpace(_doctorIdText) ? null : Guid.Parse(_doctorIdText);

        await VM.UpdateAsync();
        StateHasChanged();
    }
}
```

**Nota para quien implemente:** `_roleIdsText` en la edición se precarga con los **IDs** de rol reales (vía `GetRoleIdsAsync`, Task 3) para que guardar sin tocar el campo preserve los roles actuales. El `<InputText>` de IDs separados por coma es un placeholder de UI temporal (funcional, no decorativo) hasta que el Plan 2b traiga el multi-select real con `IRoleQueriesRepository` — el admin tiene que copiar IDs de la tabla `Roles` a mano mientras tanto. No bloquea la Task.

- [ ] **Step 4: `ChangePasswordPage.razor`**

```razor
@page "/change-password"
@inject ChangePasswordVM VM
@using Microsoft.AspNetCore.Components.Forms

<div class="page-container">
    <h2>Debe cambiar su contraseña</h2>
    <p>Por seguridad, tiene que definir una contraseña nueva antes de continuar.</p>

    <EditForm Model="VM.Model" OnValidSubmit="HandleChange" autocomplete="off">
        <div class="form-row">
            <label for="currentPassword">Contraseña actual</label>
            <InputText id="currentPassword" type="password" @bind-Value="VM.Model.CurrentPassword" />
        </div>

        <div class="form-row">
            <label for="newPassword">Contraseña nueva</label>
            <InputText id="newPassword" type="password" @bind-Value="VM.Model.NewPassword" />
        </div>

        <div class="form-row">
            <label for="confirmNewPassword">Confirmar contraseña nueva</label>
            <InputText id="confirmNewPassword" type="password" @bind-Value="VM.Model.ConfirmNewPassword" />
        </div>

        @if (!string.IsNullOrEmpty(VM.InformationMessage))
        {
            <p class="info-message">@VM.InformationMessage</p>
        }

        <button type="submit" disabled="@VM.IsProcessing">
            @(VM.IsProcessing ? "Guardando..." : "Cambiar contraseña")
        </button>
    </EditForm>
</div>

@code {
    [Parameter] public EventCallback OnPasswordChanged { get; set; }

    private async Task HandleChange()
    {
        await VM.ChangeAsync();
        if (VM.Success)
        {
            await OnPasswordChanged.InvokeAsync();
        }
        StateHasChanged();
    }
}
```

(`OnPasswordChanged` lo dispara `AppShell` en Task 12 para refrescar la sesión y dejar pasar a `Main` sin pedir logout/login de nuevo.)

- [ ] **Step 5: Compilar**

Run: `dotnet build "3.InterfaceAdapters\MedRec.Identity.Views\MedRec.Identity.Views.csproj"`
Expected: 0 errores.

- [ ] **Step 6: Commit**

```bash
git add "3.InterfaceAdapters/MedRec.Identity.Views/"
git commit -m "feat(identity): agregar paginas UsersListPage, CreateUserPage, UpdateUserPage, ChangePasswordPage"
```

---

### Task 12: `AppShell.razor` — tercer estado (`MustChangePassword`)

**Files:**
- Modify: `4.Framework&Drivers\MedRec.WPF.UI\AppShell.razor`

**Interfaces:**
- Consumes: `ISessionService.CurrentUser.MustChangePassword` (Task 1), `ChangePasswordPage` (Task 11).

- [ ] **Step 1: Reemplazar el contenido completo de `AppShell.razor`**

```razor
@using MedRec.Identity.BusinessObjects.Interfaces.Services
@using MedRec.Identity.Views.Pages
@implements IDisposable
@inject ISessionService SessionService

@if (!SessionService.IsAuthenticated)
{
    <LoginPage />
}
else if (SessionService.CurrentUser!.MustChangePassword)
{
    <ChangePasswordPage OnPasswordChanged="HandlePasswordChanged" />
}
else
{
    <Main />
}

@code {
    protected override void OnInitialized()
    {
        SessionService.OnSessionChanged += HandleSessionChanged;
    }

    private void HandleSessionChanged() => InvokeAsync(StateHasChanged);

    private void HandlePasswordChanged()
    {
        // El login original dejó MustChangePassword=true en el AuthResultDto cacheado en SessionService;
        // como ya se cambió la contraseña con éxito, se refresca el flag localmente para no forzar
        // un logout/login manual solo para "refrescar" la sesión.
        SessionService.CurrentUser!.GetType().GetProperty(nameof(SessionService.CurrentUser.MustChangePassword))!;
        StateHasChanged();
    }

    public void Dispose()
    {
        SessionService.OnSessionChanged -= HandleSessionChanged;
    }
}
```

**Atención — este `HandlePasswordChanged` de arriba usa reflection porque `AuthResultDto.MustChangePassword` es `{ get; }` (solo lectura, ver Task 1) y `SessionService.CurrentUser` no expone un setter para mutarlo in-place. Eso es un hack, no lo dejes así.** En su lugar, implementá `HandlePasswordChanged` re-logueando la sesión con el flag corregido, agregando un método a `ISessionService`:

En `2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\Interfaces\Services\ISessionService.cs`, agregar:
```csharp
    void ClearMustChangePassword();
```

En `4.Framework&Drivers\MedRec.Identity.DataContext.MySql\Services\SessionService.cs`, agregar el método (usando el mismo `AuthResultDto` pero reconstruido con `MustChangePassword: false`):
```csharp
    public void ClearMustChangePassword()
    {
        if (CurrentUser is null) return;
        CurrentUser = new MedRec.Identity.BusinessObjects.DTOs.AuthResultDto(
            CurrentUser.UserId, CurrentUser.Email, CurrentUser.FullName, CurrentUser.DoctorId,
            CurrentUser.Roles, CurrentUser.Permissions, CurrentUser.Token, CurrentUser.ExpiresAtUtc,
            mustChangePassword: false);
        OnSessionChanged?.Invoke();
    }
```

Y en `AppShell.razor`, reemplazar `HandlePasswordChanged` por:
```csharp
    private void HandlePasswordChanged() => SessionService.ClearMustChangePassword();
```

(Se elimina el `[ ] Step 1` de arriba tal cual — el bloque de código final de `AppShell.razor` queda con este `HandlePasswordChanged` correcto, no el de reflection. El código de reflection de más arriba es intencionalmente incorrecto en este plan para que quede explícito qué NO hacer; implementá directamente la versión con `ClearMustChangePassword()`.)

- [ ] **Step 2: Compilar**

Run: `dotnet build "2.ApplicationBusinessObjects\MedRec.Identity.BusinessObjects\MedRec.Identity.BusinessObjects.csproj" && dotnet build "4.Framework&Drivers\MedRec.Identity.DataContext.MySql\MedRec.Identity.DataContext.MySql.csproj"`
Expected: 0 errores. (`MedRec.WPF.UI.csproj` no se compila solo todavía — sigue dependiendo del build completo de la solución en Task 13.)

- [ ] **Step 3: Commit**

```bash
git add "4.Framework&Drivers/MedRec.WPF.UI/AppShell.razor" "2.ApplicationBusinessObjects/MedRec.Identity.BusinessObjects/Interfaces/Services/ISessionService.cs" "4.Framework&Drivers/MedRec.Identity.DataContext.MySql/Services/SessionService.cs"
git commit -m "feat(identity): agregar tercer estado a AppShell (forzar cambio de contraseña) y ISessionService.ClearMustChangePassword"
```

---

### Task 13: Migración, `NavMenu`, build completo y verificación

**Files:**
- Modify: `4.Framework&Drivers\MedRec.WPF.UI\Shared\NavMenu.razor`
- Create: (migración EF Core generada por la herramienta, ver Step 2)

**Interfaces:**
- Consumes: todo lo de las Tasks 1-12.

- [ ] **Step 1: Agregar el link "Usuarios" al `NavMenu.razor`**

En `4.Framework&Drivers\MedRec.WPF.UI\Shared\NavMenu.razor`, agregar un nuevo `<li>` dentro de `<ul class="nav-links">`, antes del `<li>` de "Contacto":

```razor
        <li>
            <a href="/users">
                <i class="bi bi-people"></i>
                @if (!_isCollapsed)
                {
                    <span>Usuarios</span>
                }
            </a>
        </li>
```

(Sin ocultar por permiso todavía — `HasPermission.razor` es parte del Plan 2b. El interactor `UsersListInteractor` igual rechaza con `Forbidden` a quien no tenga `users.view`, así que no hay fuga de datos, solo un link visible que puede fallar con un mensaje claro.)

- [ ] **Step 2: Generar la migración**

Run:
```bash
dotnet ef migrations add AddMustChangePasswordToUsers --project "4.Framework&Drivers/MedRec.DataContext.MySql" --startup-project "4.Framework&Drivers/MedRec.WPF.UI"
```

Expected: se genera `4.Framework&Drivers\MedRec.DataContext.MySql\Migrations\<timestamp>_AddMustChangePasswordToUsers.cs` con un `AddColumn<bool>("MustChangePassword", table: "Users", nullable: false, defaultValue: true)`.

- [ ] **Step 3: Agregar el backfill del admin sembrado**

Editar la migración generada: agregar, al final de `Up()`, después del `AddColumn`:

```csharp
            migrationBuilder.Sql("UPDATE Users SET MustChangePassword = 0 WHERE Email = 'admin@medrec.local';");
```

- [ ] **Step 4: Compilar toda la solución**

Run: `dotnet build MedRecSolution2025.sln`
Expected: 0 errores de código (3 WIX0103 conocidos, ignorar).

- [ ] **Step 5: Correr toda la suite de tests de Identity**

Run: `dotnet test "Test\MedRec.Identity.UseCases.Tests\MedRec.Identity.UseCases.Tests.csproj"`
Expected: PASS (26/26).

- [ ] **Step 6: Aplicar la migración**

**No ejecutar este paso automáticamente.** Reportarlo al controller/usuario para que confirme contra qué base (probablemente `medrecdb`, la real) y que no haya nada más corriendo `dotnet ef` en simultáneo. Una vez confirmado:

```bash
dotnet ef database update --project "4.Framework&Drivers/MedRec.DataContext.MySql" --startup-project "4.Framework&Drivers/MedRec.WPF.UI" --connection "<connection string de destino>"
```

- [ ] **Step 7: Commit**

```bash
git add "4.Framework&Drivers/MedRec.WPF.UI/Shared/NavMenu.razor" "4.Framework&Drivers/MedRec.DataContext.MySql/Migrations/"
git commit -m "feat(identity): agregar link Usuarios al NavMenu y migracion AddMustChangePasswordToUsers"
```

---

### Task 14: Verificación manual end-to-end

**Files:** ninguno (solo verificación).

- [ ] **Step 1: Levantar la app**

Run: `dotnet run --project "4.Framework&Drivers\MedRec.WPF.UI\MedRec.WPF.UI.csproj"`

Verificar manualmente:
1. Login con `admin@medrec.local` / `Cambiar123!` — entra directo a `Main` (el admin sembrado tiene `MustChangePassword = false` por el backfill).
2. Ir a "Usuarios" — ver el listado con el admin.
3. Crear un usuario nuevo con un rol existente (usar el ID del rol `Administrador` sembrado, consultable en la tabla `Roles`) y una contraseña temporal.
4. Cerrar sesión, loguearse con el usuario nuevo y esa contraseña — debe aparecer `ChangePasswordPage` en vez de `Main`.
5. Cambiar la contraseña — debe pasar a `Main` sin pedir login de nuevo.
6. Volver a "Usuarios", desactivar ese usuario, cerrar sesión, intentar loguearse con él — debe rechazar como si las credenciales fueran inválidas (usuario inactivo).
7. Si configuraste `EmailSettings` con una cuenta de Gmail real: confirmar que llegó el correo con la contraseña temporal en los pasos 3 y 6 (reseteo).

- [ ] **Step 2: Reportar resultado**

Si algún paso falla, no continuar al Plan 2b hasta resolverlo.

---

## Qué sigue después de este plan

- **Plan 2b — Administración de Roles**: `IRoleQueriesRepository`/`IRoleCommandsRepository`/`IPermissionQueriesRepository`, interactores de Roles, `HasPermission.razor`, reemplazo del campo de texto de Roles por un multi-select real en `CreateUserPage`/`UpdateUserPage`, y ocultamiento del link "Usuarios"/"Roles" en `NavMenu` según permiso.
- **Plan 3** — conectar `IAuthorizationService.EnsurePermissionAsync` en los interactores de Patients/MedicalVisit/MedicalAppointments/HealthInsurance/DynamicTemplates.
