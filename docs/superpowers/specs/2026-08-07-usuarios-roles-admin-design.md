# Administración de Usuarios y Roles

**Fecha:** 2026-08-07
**Estado:** Aprobado para pasar a plan de implementación
**Depende de:** `docs/superpowers/specs/2026-08-05-identity-design.md` (ya implementado y mergeado a `master`)

## Contexto y alcance

El spec de Identity núcleo dejó login, sesión, auditoría y el modelo de dominio (`User`/`Role`/`Permission`) funcionando, con un único usuario admin sembrado por migración. Este spec cubre el **Plan 2** anotado en el roadmap de aquel documento: pantallas de administración para crear/editar usuarios y roles, y asignar permisos — sin las cuales el único usuario del sistema sigue siendo el admin sembrado a mano.

**Fuera de alcance de este spec** (quedan para specs futuros):
- Conectar `IAuthorizationService.EnsurePermissionAsync` en los interactores de Patients/MedicalVisit/MedicalAppointments/HealthInsurance/DynamicTemplates (Plan 3).
- Cambio de contraseña voluntario desde dentro de la app cuando `MustChangePassword` es `false` (se puede agregar después reutilizando el mismo interactor de cambio de contraseña de este plan).
- Cualquier flujo de confirmación de email para autoregistro — este plan solo envía notificaciones salientes (contraseña temporal), no valida direcciones de email entrantes.

## Decisiones de diseño

| Decisión | Elegido | Motivo |
|---|---|---|
| Enforcement de permisos en las pantallas nuevas | Protegidas desde ya con `EnsurePermissionAsync`, aunque el resto de las features (Plan 3) todavía no lo tengan | Es la pantalla más sensible del sistema — cualquier usuario logueado podría auto-asignarse el rol Administrador si queda abierta. Código nuevo, barato agregar el chequeo al escribirlo. |
| Contraseña inicial de un usuario nuevo | El admin define una temporal; el usuario debe cambiarla en su primer login | Evita que el admin conozca la contraseña definitiva del usuario; consistente con la política de "el admin nunca sabe la contraseña real". |
| Notificación de la contraseña temporal | Por email, vía Gmail SMTP (cuenta dedicada, contraseña de aplicación) | Pedido explícito del cliente; deja el andamiaje (`IEmailNotificationService`) listo para cuando la futura versión web necesite confirmación de email también. |
| Reseteo de contraseña por el admin | Mismo mecanismo que el alta: nueva temporal + `MustChangePassword = true` + email | Reutiliza el flujo ya diseñado, sin duplicar lógica. |
| Edición de email de un usuario existente | No editable | El email es el identificador de login; cambiarlo a mitad de sesión o con el usuario logueado en otro dispositivo genera casos raros no justificados por este alcance. |
| Alcance de Roles | CRUD completo (crear/editar/listar/borrar), con permisos asignables por checklist | Pedido explícito; borrado bloqueado si el rol tiene usuarios asignados, para no dejar usuarios sin rol accidentalmente. |
| `HasPermission.razor` | Se construye en este plan (estaba diseñado en el spec de Identity núcleo pero nunca se implementó) | Hace falta para ocultar los ítems "Usuarios"/"Roles" del `NavMenu` según permiso. |

## Arquitectura

No se crea una feature slice nueva — se extiende `MedRec.Identity.*` (los 7 proyectos ya existentes de Identity núcleo), agregando:

- **Capa 1** (`MedRec.Entity`): `User.MustChangePassword` (bool, nuevo campo).
- **Capa 2** (`MedRec.Identity.BusinessObjects` / `MedRec.Identity.UseCases`):
  - Repositorios nuevos: `IUserCommandsRepository`, `IRoleQueriesRepository`, `IRoleCommandsRepository`, `IPermissionQueriesRepository`.
  - Puerto nuevo: `IEmailNotificationService` (`SendTemporaryPasswordAsync(string email, string fullName, string temporaryPassword, CancellationToken ct)`).
  - Interactores nuevos: `CreateUserInteractor`, `UpdateUserInteractor`, `ToggleUserActiveInteractor`, `ResetUserPasswordInteractor`, `UsersListInteractor`, `ChangePasswordInteractor`, `CreateRoleInteractor`, `UpdateRoleInteractor`, `DeleteRoleInteractor`, `RolesListInteractor`, `RoleDetailsInteractor`, `PermissionsCatalogInteractor` (lista los 28 permisos para el checklist).
- **Capa 3** (`MedRec.Identity.Presenters` / `.Repositories` / `.ViewModels` / `.Views`):
  - Presenters e implementaciones de repositorio correspondientes a lo anterior.
  - VMs y páginas: `UsersListPage`, `CreateUserPage`, `UpdateUserPage`, `RolesListPage`, `CreateRolePage`, `UpdateRolePage`, `ChangePasswordPage`.
  - Componente `HasPermission.razor` (nuevo).
- **Capa 4** (`MedRec.Identity.DataContext.MySql`): implementaciones EF de los repositorios nuevos, y `SmtpEmailNotificationService : IEmailNotificationService` (usa `System.Net.Mail.SmtpClient` contra Gmail, con host/usuario/contraseña de aplicación leídos de `appsettings.json` y encriptados con el mismo mecanismo DPAPI que la cadena de conexión y la clave JWT — nueva clase `EmailSettings` en `MedRec.Shared.Security`, análoga a `Jwt`).

## Modelo de datos

```
User (modificación)
  + MustChangePassword (bool, default true)
```

Migración `AddMustChangePasswordToUsers`: agrega la columna (`NOT NULL DEFAULT true`), y hace un `UPDATE Users SET MustChangePassword = 0 WHERE Email = 'admin@medrec.local'` para no forzar el cambio en el admin ya sembrado (su contraseña ya la conoce el cliente).

No hay tablas nuevas — `Role`/`Permission`/`UserRole`/`RolePermission` ya existen desde Identity núcleo.

## Flujo de contraseña temporal y notificación por email

**Alta de usuario / reseteo de contraseña** (mismo interactor subyacente para ambos casos, `ResetUserPasswordInteractor` reutilizado internamente por `CreateUserInteractor`):
1. Se guarda el hash de la temporal (`IPasswordHasher`) y `MustChangePassword = true`.
2. `IEmailNotificationService.SendTemporaryPasswordAsync(...)` envía el correo. Si el envío falla (SMTP caído, credenciales mal configuradas), la operación de alta/reseteo **no se revierte** — el usuario queda creado con la temporal igual, y el error de envío se informa al admin como advertencia (`UserMessageAction.ShowWarning`) para que le pase la contraseña por otro medio. El email es un "mejor esfuerzo", no una condición bloqueante de la transacción.

**Login con `MustChangePassword = true`:**
1. `AuthResultDto` suma el campo `MustChangePassword`.
2. `AppShell.razor` pasa a tener tres estados: no autenticado → `LoginPage`; autenticado con `MustChangePassword = true` → `ChangePasswordPage` (obligatoria, sin salida salvo cerrar sesión); autenticado y OK → `Main`.
3. `ChangePasswordPage` pide contraseña actual (verificada contra el hash), nueva contraseña, y confirmación. Al guardar, `MustChangePassword` pasa a `false`.

## Pantallas

**Usuarios:**
- `UsersListPage`: tabla con email, nombre, roles, estado. Acciones por fila: Editar, Activar/Desactivar, Resetear contraseña.
- `CreateUserPage`: email, nombre completo, contraseña temporal, roles (multi-select), Doctor vinculado (dropdown opcional, reutilizando el mecanismo de búsqueda de Doctor ya existente en el proyecto si lo hay — a confirmar en la etapa de plan).
- `UpdateUserPage`: nombre completo, roles, Doctor vinculado (email no editable).

**Roles:**
- `RolesListPage`: tabla con nombre, descripción, cantidad de usuarios, cantidad de permisos. Acción: Borrar (bloqueado con mensaje si tiene usuarios).
- `CreateRolePage` / `UpdateRolePage`: nombre, descripción, checklist de los 28 permisos agrupados por feature (patients.*, medicalvisits.*, appointments.*, healthinsurance.*, dynamictemplates.*, users.*, roles.*).

## Enforcement y navegación

- Cada interactor nuevo llama a `IAuthorizationService.EnsurePermissionAsync` con el permiso correspondiente (`users.view/create/edit/delete`, `roles.view/create/edit/delete`) antes de ejecutar — mismo patrón `BusinessException(Forbidden)` ya usado en Identity núcleo.
- `HasPermission.razor` oculta los ítems "Usuarios"/"Roles" del `NavMenu` si el usuario no tiene `users.view`/`roles.view`.

## Testing

TDD con xUnit + Moq en `Test\MedRec.Identity.UseCases.Tests`, siguiendo el patrón ya establecido: un test por interactor cubriendo caso feliz, validación fallida, y rechazo por falta de permiso. Caso específico para `DeleteRoleInteractor`: bloqueo cuando el rol tiene usuarios asignados. `IEmailNotificationService` se mockea en los tests — no se prueba contra Gmail real.

## Roadmap (sin cambios respecto al spec de Identity núcleo)

- **Plan 3** — conectar `IAuthorizationService.EnsurePermissionAsync` en los interactores de Patients/MedicalVisit/MedicalAppointments/HealthInsurance/DynamicTemplates.
- **Spec 2** — acceso a historias clínicas por médico y derivaciones.
- **Spec 3** — campos dinámicos configurables por profesional.
- **Spec 4** — cifrado de datos sensibles en reposo.
- **Iniciativa separada, sin fecha** — evaluar una reescritura completa a DDD en una solución nueva, una vez que este proyecto esté estable y el feedback real del cliente termine de definir el alcance. Empezar, si se decide avanzar, por un bounded context acotado (ej. Identity) probando el patrón completo (incluida migración de datos real) antes de expandirlo.
