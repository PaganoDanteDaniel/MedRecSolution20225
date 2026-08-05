# Identity: Usuarios, Roles, Permisos, Login y Auditoría

**Fecha:** 2026-08-05
**Estado:** Aprobado para pasar a plan de implementación
**Autor:** Sesión de brainstorming con Claude Code

## Contexto y alcance

MedRecSolution2025 hoy no tiene ningún mecanismo de autenticación ni autorización: la app WPF arranca directo en la lista de pacientes, sin login, y no queda registro de quién creó o modificó cada historia clínica. El cliente pidió implementar permisos, roles y usuarios; durante el diseño surgieron dos requerimientos adicionales de mayor alcance (acceso a historias clínicas por médico con derivaciones, y campos dinámicos configurables por profesional) que se decidió tratar como specs independientes, por depender de este pero tener un modelo de dominio propio.

**Este documento cubre únicamente el subsistema de Identity**: autenticación, sesión, catálogo de usuarios/roles/permisos, control de acceso a nivel de funcionalidad (RBAC), y auditoría de creación/modificación en todas las entidades existentes.

**Explícitamente fuera de alcance de este spec** (quedan para specs siguientes, ver [Roadmap](#roadmap)):
- Control de acceso a nivel de paciente individual (qué médico ve la historia de qué paciente) y derivaciones entre médicos.
- Configuración de campos dinámicos de visita por profesional (hoy `TemplateFieldDefinition` está scopeado por `SpecialtyId`, no por médico).
- Cifrado de datos sensibles en reposo en la base de datos.

No se realiza una reescritura general del sistema: se trabaja de forma incremental sobre la arquitectura Clean Architecture / Ports & Adapters existente, siguiendo las convenciones documentadas en `CLAUDE.md`.

## Decisiones de diseño

Resumen de las decisiones tomadas durante el brainstorming, con su motivo:

| Decisión | Elegido | Motivo |
|---|---|---|
| Alcance | Completo: login + CRUD usuarios/roles + permisos en UI | Es lo que finalmente necesita el cliente; no tiene sentido un núcleo parcial que haya que retocar enseguida |
| Roles por usuario | Múltiples roles por usuario | Un usuario puede combinar responsabilidades (ej. Médico + Administrador) |
| Relación User↔Doctor | Entidades separadas, vínculo opcional (`User.DoctorId` nullable) | Permite dar de alta personal administrativo/recepción sin forzarlos a ser un `Doctor` |
| Enforcement de permisos | UI + Interactores | Ocultar en UI no alcanza: hay que impedir la acción también del lado del caso de uso, para que no dependa solo de la capa de presentación |
| Hashing de contraseñas | `Microsoft.AspNetCore.Identity.PasswordHasher<TUser>` (paquete `Microsoft.Extensions.Identity.Core`) | No arrastra dependencias de hosting web; si en el futuro se migra a una app web con ASP.NET Core Identity, los hashes generados ahora siguen siendo válidos sin forzar reset de contraseñas |
| Bootstrap del primer admin | Seed automático en la migración inicial de EF Core | Igual que las demás migraciones del proyecto, sin pasos manuales extra |
| Granularidad de permisos | Por feature + acción CRUD (`patients.view`, `patients.create`, etc.) | Mismo patrón que el proyecto de referencia (`D:\$Root\MedRecSolution\MedRec`), aplicado a todas las features existentes |
| Sesión en la app de escritorio | Login obligatorio en cada apertura, sin "recordarme" | Más seguro en un entorno clínico donde varias personas pueden compartir la misma PC |
| Auditoría (creado/modificado por y cuándo) | Aplicada a **todas** las entidades existentes, no solo a Identity | Requisito explícito del cliente para trazabilidad de historias clínicas |
| Rewrite general del sistema | Descartado, se avanza incremental | Sistema en producción con datos reales de pacientes; el riesgo de una reescritura sin objetivo puntual supera el beneficio. Login + roles + acceso por médico + auditoría ya cubren el objetivo real de "profesionalizar la seguridad" |

## Arquitectura: nueva feature slice `Identity`

Se replica el patrón de 5 proyectos + DataContext que ya usan Patients/HealthInsurance/MedicalVisit/MedicalAppointments/DynamicTemplates (ver `CLAUDE.md`):

- **`MedRec.Identity.BusinessObjects`** (capa 2): DTOs, interfaces de puertos de entrada/salida, interfaces de repositorio, validadores.
- **`MedRec.Identity.UseCases`** (capa 2): interactores.
- **`MedRec.Identity.Presenters`** (capa 3): implementaciones de output ports.
- **`MedRec.Identity.Repositories`** (capa 3): implementaciones de los repositorios UoW.
- **`MedRec.Identity.ViewModels`** (capa 3): Models, VM, Orchestration (`AuthenticateOrchestrator`, `ManageUsersOrchestrator`, `ManageRolesOrchestrator`).
- **`MedRec.Identity.Views`** (capa 3): páginas/componentes Razor (`LoginPage`, `UsersListPage`, `CreateUserPage`, `UpdateUserPage`, `RolesListPage`, `CreateRolePage`, `UpdateRolePage`).
- **`MedRec.Identity.DataContext.MySql`** (capa 4): implementación `IIdentityDataContext`, más las implementaciones de infraestructura de sesión/auth (`SessionService`, `CurrentUserContext`, `PasswordHasher`, `AuthTokenGenerator`).

**Excepción deliberada a la regla de aislamiento entre features**: dado que la autorización es una incumbencia transversal, los proyectos `UseCases` de las demás features (Patients, MedicalVisit, MedicalAppointments, HealthInsurance, DynamicTemplates) pueden referenciar `MedRec.Identity.BusinessObjects` **únicamente** para consumir el puerto `IAuthorizationService` (ver [Enforcement de permisos](#enforcement-de-permisos)). No referencian `MedRec.Identity.UseCases` ni `MedRec.Identity.Repositories`. Es el mismo tipo de dependencia compartida que ya existe hoy hacia `MedRec.BusinessObjects`/`MedRec.Entity`.

## Modelo de dominio

Entidades POCO nuevas en `MedRec.Entity.POCOEntities` (mismo estilo plano que `Patient.cs`, sin clase base, `Guid` como Id):

```
User
  Id, Email, PasswordHash, FullName, IsActive, DoctorId (Guid?),
  IsDeleted, RowVersion

Role
  Id, Name, Description, IsDeleted, RowVersion

Permission
  Id, Code (string, ej "patients.view"), Description, IsDeleted, RowVersion

UserRole (tabla puente)
  UserId, RoleId

RolePermission (tabla puente)
  RoleId, PermissionId
```

`IsDeleted` (baja administrativa/soft-delete) y `RowVersion` (concurrencia optimista vía `timestamp(6)` MySQL) se agregan por consistencia con las 16 tablas existentes, que ya siguen ese patrón. `IsActive` en `User` es un estado de negocio distinto (cuenta bloqueada/desactivada por un admin sin necesariamente estar dada de baja).

`Permission` es un catálogo fijo, definido en código como constantes `SystemPermissions` (análogo al proyecto de referencia) y sembrado por migración — **no** es editable desde la UI. `Role` sí es administrable (crear/editar/borrar, asignar permisos). `User` es administrable (crear/editar/desactivar, asignar roles).

### Catálogo inicial de permisos

Un permiso por combinación `feature.acción`, para las features existentes más la propia gestión de Identity:

```
patients.view / patients.create / patients.edit / patients.delete
medicalvisits.view / medicalvisits.create / medicalvisits.edit / medicalvisits.delete
appointments.view / appointments.create / appointments.edit / appointments.delete
healthinsurance.view / healthinsurance.create / healthinsurance.edit / healthinsurance.delete
dynamictemplates.view / dynamictemplates.create / dynamictemplates.edit / dynamictemplates.delete
users.view / users.create / users.edit / users.delete
roles.view / roles.create / roles.edit / roles.delete
```

## Autenticación y sesión

### Login

`AuthenticateUserInteractor` (`MedRec.Identity.UseCases`):
1. Recibe `AuthenticateUserDto` (email, password).
2. Busca `User` por email; si no existe, está inactivo, o el hash no matchea (`IPasswordHasher.Verify`), llama a `outputPort.InvalidCredentials()` — mensaje genérico, sin distinguir el motivo exacto (para no filtrar si el email existe en el sistema).
3. Si es válido, carga roles y permisos del usuario (`UserRole` → `RolePermission` → `Permission`).
4. Genera un JWT (`IAuthTokenGenerator`, usando la sección `Jwt` que ya existe en `appsettings.json` — `Key` y `ExpirationMinutes`, hoy sin usar) con claims `sub`, `email`, `role[]`, `permission[]`.
5. Retorna `AuthResultDto` (UserId, Email, FullName, DoctorId, Roles, Permissions, Token) vía `Handle()`.

### Sesión en el host WPF

- `ISessionService` (puerto en `MedRec.Identity.BusinessObjects`, implementado en `MedRec.Identity.DataContext.MySql`): guarda el `AuthResultDto` actual **solo en memoria** (proceso vive mientras la app está abierta; no hay almacenamiento en disco tipo "recordarme", según lo decidido). Expone `IsAuthenticated`, `CurrentUser`, evento `OnSessionChanged`, y métodos `LoginAsync` / `LogoutAsync`.
- `ICurrentUserContext` (puerto en capa 1, `MedRec.Entity.Interfaces`, junto a `IRepositoryUnitOfWork`): expone `UserId` (`Guid?`). Implementado por `CurrentUserContext` en `MedRec.Identity.DataContext.MySql`, leyendo de `ISessionService.CurrentUser`. Es lo que usa `MedRecContext` para el stamping de auditoría (ver más abajo) y, a futuro, el spec de acceso por médico.

### UI

- `LoginPage.razor` (en `MedRec.Identity.Views`): email/password, invoca el orquestador de login, en éxito llama `ISessionService.LoginAsync(...)`.
- Nuevo `AppShell.razor` envuelve el `Router` actual (`Main.razor`). Se registra como `RootComponent` en `MainWindow.xaml` en lugar de `Main`. Inyecta `ISessionService`, se suscribe a `OnSessionChanged`: si `!IsAuthenticated` renderiza `LoginPage`, si no, renderiza `<Main />`. Como no hay persistencia de sesión entre aperturas, `IsAuthenticated` arranca en `false` en cada proceso nuevo → login obligatorio, sin tocar `App.xaml.cs` / `MainWindow.xaml.cs`.
- `NavMenu.razor` se extiende con: nombre del usuario logueado, ítems de menú "Usuarios" y "Roles" (visibles solo si el usuario tiene `users.view` / `roles.view`), y botón "Cerrar sesión" (`ISessionService.LogoutAsync()` → dispara `OnSessionChanged` → `AppShell` vuelve a `LoginPage`).
- Componente `HasPermission.razor` (en `MedRec.Identity.Views` o en un proyecto de componentes común): oculta su `ChildContent` si el usuario actual no tiene el permiso indicado. Se usa en `NavMenu` y en botones de acciones (crear/editar/borrar) de cada feature existente.

## Enforcement de permisos

Además de ocultar en la UI, cada interactor sensible de cada feature valida el permiso del usuario actual **antes** de ejecutar la acción, mediante el puerto compartido:

```csharp
// MedRec.Identity.BusinessObjects/Interfaces/Ports/IAuthorizationService.cs
public interface IAuthorizationService
{
    Task EnsurePermissionAsync(Guid? userId, string permissionCode, CancellationToken ct);
}
```

`EnsurePermissionAsync` lanza `BusinessException` (ya existente en `MedRec.Shared.Exceptions`) con un `ErrorInfo` de código `ErrorCode.Forbidden` (nuevo valor a agregar al enum existente, con `HttpStatusCode = 403`) si el usuario no tiene el permiso. Como `BusinessException` ya es reenviada "pass-through" por `DefaultExceptionToErrorInfoMapper`, y las excepciones de los interactores ya son capturadas automáticamente por `UseCaseExceptionProxy`, **no hace falta agregar try/catch manual** — se mantiene la convención existente. `BaseOutputPort<T>.ErrorAsync` se extiende con `ErrorCode.Forbidden => UserMessageAction.ShowError` en el switch existente (mismo tratamiento visual que el resto de los errores no-validación).

Ejemplo de uso en un interactor existente (ej. `CreatePatientInteractor`):

```csharp
public async Task HandleAsync(CreatePatientDto dto, CancellationToken ct)
{
    await _authorizationService.EnsurePermissionAsync(_currentUserContext.UserId, SystemPermissions.Patients_Create, ct);
    // ... resto del flujo sin cambios
}
```

Este patrón se aplica a los interactores de creación/edición/borrado de todas las features existentes (Patients, MedicalVisit, MedicalAppointments, HealthInsurance, DynamicTemplates) además de a los propios de Identity. Los interactores de sólo lectura (listados/detalle) también validan el permiso `.view` correspondiente.

## Auditoría (creado/modificado por y cuándo)

Aplicada a **todas** las entidades del sistema, existentes y nuevas, de forma centralizada:

- Nueva interfaz `IAuditableEntity` (capa 1, `MedRec.Entity.Interfaces`): `CreatedBy` (`Guid?`), `CreatedAt` (`DateTime`), `UpdatedBy` (`Guid?`), `UpdatedAt` (`DateTime?`).
- Las 16 entidades POCO existentes (`Patient`, `Doctor`, `MedicalAppointment`, `PatientMedicalVisit`, `PatientMedicalCondition`, `PatientMedicalHistory`, `MedicalCondition`, `MedicalConditionType`, `Province`, `City`, `HealthInsuranceCompany`, `LaboratoryResultType`, `PatientLaboratoryResult`, `MedicalSpecialty`, `TemplateFieldDefinition`, `MedicalVisitDynamicField`) implementan `IAuditableEntity`, agregando las propiedades que les falten (algunas ya tienen `CreatedAt`/`UpdatedAt`, ninguna tiene `CreatedBy`/`UpdatedBy`). Las nuevas de Identity (`User`, `Role`) también.
- `MedRecContext` (capa 4) pasa a recibir `ICurrentUserContext` por constructor y **sobreescribe `SaveChangesAsync`**: antes de persistir, recorre `ChangeTracker.Entries<IAuditableEntity>()`, y en cada entrada `Added` setea `CreatedAt = DateTime.UtcNow` / `CreatedBy = currentUserContext.UserId`, y en cada `Modified` setea `UpdatedAt = DateTime.UtcNow` / `UpdatedBy = currentUserContext.UserId`.
- Como `DataContextUnitOfWork.SaveChangesAsync` ya invoca `context.SaveChangesAsync` por delegado (despacho virtual), el stamping queda automático para todas las escrituras del sistema sin modificar ningún interactor existente de Patients/MedicalVisit/etc.
- Migración: se agregan las 4 columnas a las 16 tablas existentes + `Users`/`Roles`. `CreatedBy`/`UpdatedBy` son FK nullable a `Users.Id` — nullable porque los registros históricos no tienen usuario real asociado (quedan en `NULL` tras la migración); de ahí en adelante toda alta/edición los completa automáticamente.

## Migraciones y seed

### Limpieza previa: tablas huérfanas en `medrecdb`

La base `medrecdb` (dump provisto por el cliente el 2026-08-05) ya contiene tablas `users`, `roles`, `permissions`, `userroles`, `rolepermissions` y `professionals`, en `snake_case` — restos de un intento previo de implementación (del otro proyecto de referencia, `D:\$Root\MedRecSolution\MedRec`) corrido contra la misma base. Ninguna es consumida por la app actual en producción, no tienen historial en `__efmigrationshistory` de este proyecto, y no contienen datos que deban preservarse. Su esquema además es incompatible con las decisiones de este spec (`professional_id` obligatorio 1 a 1 en vez de `DoctorId` opcional, hash+salt separados en vez de un solo `PasswordHash`, naming `snake_case` en vez de `PascalCase` como el resto de la base).

La migración de Identity empieza entonces con un `DROP TABLE IF EXISTS` de esas 6 tablas (en orden que respete las FKs: `userroles`, `rolepermissions` primero, luego `users`, `roles`, `permissions`, `professionals`), antes de crear el esquema nuevo. No se usa `professionals` en ningún lado del diseño — el proyecto actual sigue usando `Doctor`/`doctors`.

### Migración `AddIdentityAndAudit`

Una única migración de EF Core que:
1. Elimina las tablas huérfanas descriptas arriba.
2. Crea las tablas `Users`, `Roles`, `Permissions`, `UserRoles`, `RolePermissions` (en `PascalCase`, consistente con el resto de `medrecdb`).
3. Siembra el catálogo de `Permissions` (constantes `SystemPermissions`).
4. Siembra un rol `Administrador` con todos los permisos.
5. Siembra un usuario admin por defecto (ej. `admin@medrec.local`, contraseña inicial conocida a cambiar en el primer ingreso — se documenta en el README de despliegue, no se fuerza un flujo de cambio obligatorio en este spec).
6. Agrega columnas `CreatedBy`/`CreatedAt`/`UpdatedBy`/`UpdatedAt` a las 16 tablas existentes (backfill `NULL` para `CreatedBy`/`UpdatedBy`, y `CreatedAt` con la fecha de la migración como valor por defecto para filas preexistentes).

## Testing

Siguiendo la convención existente (xUnit + Moq, interactores contra repos UoW mockeados, ver `Test\MedRec.MedicalVisit.UseCases.Tests`):

- Nuevo proyecto `Test\MedRec.Identity.UseCases.Tests`: casos de `AuthenticateUserInteractor` (credenciales válidas/inválidas/usuario inactivo), `CreateUserInteractor`/`UpdateUserInteractor` (validación, duplicados de email), `CreateRoleInteractor`/`UpdateRoleInteractor` (asignación de permisos), y del `EnsurePermissionAsync` de `IAuthorizationService`.
- Para las features existentes, se agregan casos puntuales que verifiquen que un interactor llama a `EnsurePermissionAsync` con el código correcto y que, si lanza `BusinessException(Forbidden)`, el flujo no continúa (no se llega a persistir).

## Roadmap

Specs siguientes, a diseñar por separado, que se apoyan en este:

- **Spec 2 — Acceso a historias clínicas por médico y derivaciones**: relación médico↔paciente (quién es el "responsable"), mecanismo de derivación explícita entre médicos con autorización, y cómo esto interactúa con el RBAC de este spec (¿es un permiso más, o un chequeo de datos aparte en cada consulta?).
- **Spec 3 — Campos dinámicos configurables por profesional**: hoy `TemplateFieldDefinition` está scopeado por `SpecialtyId`; pasar a (o combinar con) scope por `DoctorId`, y cómo migran las plantillas ya cargadas por especialidad.
- **Spec 4 — Cifrado de datos sensibles en reposo**: qué campos cifrar (diagnósticos, notas, datos de contacto del paciente), estrategia de manejo de claves (no puede ser DPAPI puro como hoy, porque las claves quedarían atadas a una sola máquina y varios equipos comparten la misma base MySQL), y su costo en queries/índices sobre esos campos.
