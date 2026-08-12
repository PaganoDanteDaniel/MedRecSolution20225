# CRUD de Profesionales (generalización de Doctor)

**Fecha:** 2026-08-12
**Estado:** Aprobado para pasar a plan de implementación
**Depende de:** `docs/superpowers/specs/2026-08-07-usuarios-roles-admin-design.md` (ya implementado y mergeado a `master`) — reutiliza `RoleCheckboxList`, `SystemPermissions`, y el `ICreateUserInputPort` de Identity.

## Contexto y alcance

Hoy `Doctor` es una entidad simple (tabla `Doctors`) pensada únicamente para médicos: `FirstName`, `LastName`, `LicenseNumber` (obligatorio), `SpecialtyId` (obligatorio), `Phone`, `Email`, `HireDate`. Se referencia como `Guid` suelto desde `User.DoctorId` (opcional, 1 usuario por doctor), `MedicalAppointment.DoctorId` (obligatorio) y `PatientMedicalVisit.DoctorId` (opcional). No existe ningún CRUD en la UI — solo un lookup de solo lectura (`IDoctorLookupRepository`) usado en combos de `CreateUserPage`/`UpdateUserPage`.

El sistema necesita dar de alta otros tipos de profesional (Enfermero, Recepcionista, Administrador) además de Médico, con un CRUD completo (alta, edición, listado, baja lógica), y permitir crear el `User` asociado en el mismo flujo de alta del profesional, sin reingresar datos ya cargados (email, nombre).

**Fuera de alcance de este spec** (quedan para specs futuros — ver también "Decisiones consideradas y diferidas" más abajo):
- Aislamiento de pacientes/historias clínicas por profesional dueño y mecanismo de derivación hacia otro profesional del sistema.
- Entidad `Institution`/`Clinica` para una futura migración a app web con base de datos compartida entre varias clínicas.
- Permitir asignar turnos/visitas a profesionales que no sean de tipo Doctor (hoy la regla de negocio sigue restringida a Doctor).

## Decisiones de diseño

| Decisión | Elegido | Motivo |
|---|---|---|
| Alcance del cambio | Rename completo `Doctor` → `Professional` (tabla, entidad, y las FKs `DoctorId` en `User`/`MedicalAppointment`/`PatientMedicalVisit` pasan a `ProfessionalId`) | Evita que convivan dos conceptos confusos; coherente con que el CRUD ahora maneja varios tipos de profesional. |
| Tipos de profesional | `ProfessionalType`: `Doctor`, `Nurse`, `Receptionist`, `Administrator` | Coincide con los roles ya creados en la tabla `Roles` (Médico, Recepcionista, Enfermero) más un tipo Administrador para personal de gestión. |
| Matrícula (`LicenseNumber`) | Obligatoria para `Doctor` y `Nurse`; nula para `Receptionist`/`Administrator` | Ambos tienen matrícula profesional real en la práctica. |
| Especialidad (`SpecialtyId`) | Obligatoria solo para `Doctor`; nula para el resto | Solo los médicos tienen especialidad médica. |
| Asignación de turnos/visitas | `MedicalAppointment.ProfessionalId`/`PatientMedicalVisit.ProfessionalId` se renombran por consistencia, pero el interactor sigue validando `Type == Doctor` al asignar | No cambia el comportamiento actual de negocio, solo el nombre/tipo de la FK. Se relaja más adelante si se necesita agendar con enfermería. |
| Patrón de la entidad `Professional` | POCO simple con setters públicos (igual que `Doctor` hoy), validación condicional por tipo en un `Validator` externo (API `Guard`) + en el interactor | Consistente con las ~20 entidades existentes del proyecto (`User`, `Patient`, etc.); un patrón DDD con setters privados y factory methods habría sido el único caso así en todo el codebase. |
| Alta combinada Profesional + Usuario | Checkbox "Crear también un usuario" en `CreateProfessionalPage`; si se tilda, Email y Nombre completo del usuario se copian del profesional y quedan de solo lectura; solo se piden Contraseña temporal y Roles (`RoleCheckboxList`) | Evita reingresar datos ya cargados (pedido explícito), sin duplicar campos editables que podrían divergir. |
| Transacción de la creación combinada | `CreateProfessionalOrchestrator` (capa ViewModels) compone `CreateProfessionalAction` + `CreateUserAction` (reutiliza el `ICreateUserInputPort` de Identity tal cual existe hoy); si falla la creación del usuario, se dispara una acción de compensación que elimina el profesional recién creado | Mantiene la regla de que una capa `UseCases` nunca depende de otra feature slice (sin precedente en el proyecto); la ventana de inconsistencia es acotada y se resuelve con una compensación simple, en vez de crear la primera dependencia cruzada de `UseCases` del proyecto. |
| Lookup de especialidades | Se agrega `ISpecialtyLookupRepository`/`SpecialtyLookupRepository` (nuevo) | Hoy no existe ningún lookup de `MedicalSpecialty`; hace falta para el combo "Especialidad" del formulario cuando `Type == Doctor`. |
| Migración de datos existentes | `Type` se agrega con `DEFAULT 0` (`Doctor`) — todo registro existente en `Doctors` hoy es un médico | Rename + relajar constraints, sin `DROP`, sin backfill manual necesario. |

## Arquitectura

Se crea la feature slice nueva `MedRec.Professionals.*`, siguiendo el patrón de 5 proyectos por capa ya establecido (`BusinessObjects`/`UseCases` en capa 2; `Presenters`/`Repositories`/`ViewModels`/`Views` en capa 3; `DataContext.MySql` en capa 4), y se actualizan los consumidores existentes de `Doctor`.

### Capa 1 (`MedRec.Entity`)
- `POCOEntities/Doctor.cs` → `POCOEntities/Professional.cs`: agrega `Type` (`ProfessionalType`, nuevo enum en `MedRec.Entity.Enums`). `LicenseNumber` y `SpecialtyId` pasan a nullable (`string?`, `Guid?`).
- `User.DoctorId` → `User.ProfessionalId`.
- `MedicalAppointment.DoctorId` → `MedicalAppointment.ProfessionalId` (sigue no-nullable).
- `PatientMedicalVisit.DoctorId` → `PatientMedicalVisit.ProfessionalId` (sigue nullable).

### Capa 2 (`MedRec.Professionals.BusinessObjects` / `MedRec.Professionals.UseCases`)
- DTOs: `CreateProfessionalDto`, `UpdateProfessionalDto`, `ProfessionalDto` (listado/detalle), y el sub-objeto opcional para el alta combinada (`CreateUserForProfessionalDto`: `TemporaryPassword`, `RoleIds`).
- `IProfessionalRepositoryUoW`: `GetByIdAsync`, `GetByEmailAsync`, `ListAsync(filtro por Type / IsDeleted)`, `CreateAsync`, `UpdateAsync`, `SoftDeleteAsync`.
- `CreateProfessionalValidator`/`UpdateProfessionalValidator` (API `Guard`, condicional por `Type`: exige `LicenseNumber` si `Doctor`/`Nurse`, exige `SpecialtyId` solo si `Doctor`).
- Ports de entrada/salida: `ICreateProfessionalInputPort`/`OutputPort`, y análogos para Update/Delete/List/GetById.
- Interactores: `CreateProfessionalInteractor`, `UpdateProfessionalInteractor`, `DeleteProfessionalInteractor` (soft-delete), `ListProfessionalsInteractor`, `GetProfessionalByIdInteractor`. Mismo patrón que el resto del proyecto: `EnsurePermissionAsync` al inicio, `ExecuteInTransactionWithRetry` en las escrituras, registrados vía `AddProfessionalsUseCasesServicesWithProxy()`.

### Capa 3 (`MedRec.Professionals.Presenters` / `.Repositories` / `.ViewModels` / `.Views`)
- Presenters: uno por caso de uso, extendiendo `BaseOutputPort<T>`.
- Repositories: `ProfessionalRepository` (pass-through a `IProfessionalDataContext`), `ISpecialtyLookupRepository`/`SpecialtyLookupRepository` (nuevo).
- ViewModels: `ProfessionalModel`, `CreateProfessionalModel` (incluye el sub-modelo opcional de usuario), `ProfessionalMapper`, y `CreateProfessionalOrchestrator` (ver tabla de decisiones — compone `CreateProfessionalAction` + `CreateUserAction` + compensación `DeleteProfessionalAction`).
- Views: `ProfessionalsListPage.razor`, `CreateProfessionalPage.razor`, `UpdateProfessionalPage.razor`, componente `ProfessionalTypeFields.razor` (muestra/oculta Matrícula y Especialidad según el `Type` elegido). Guard de permisos igual al patrón ya aplicado en `Users` (`SessionService.CurrentUser?.Permissions.Contains(...)`, oculta menú/botón/página completa).

### Capa 4 (`MedRec.Professionals.DataContext.MySql`)
- `ProfessionalDataContextMySql`, `SpecialtyLookupDataContextMySql`.
- `ProfessionalConfiguration` (reemplaza `DoctorConfiguration`): tabla `Professionals`, `LicenseNumber`/`SpecialtyId` nullable (unique index en `LicenseNumber` se mantiene — MySQL permite múltiples `NULL` en un índice único), `Type` como `int` requerido.

### Actualización de consumidores existentes de Doctor
- `IDoctorLookupRepository`/`DoctorSummaryDto`/`DoctorLookupRepository`/`DoctorLookupDataContextMySql` → `IProfessionalLookupRepository`/`ProfessionalSummaryDto`/etc., filtrando por `Type == Doctor` (no cambia el comportamiento actual de los combos en `CreateUserPage`/`UpdateUserPage`).
- `CreateUserPage.razor`/`UpdateUserPage.razor`: `DoctorLookup` → `ProfessionalLookup`, `VM.Model.DoctorId` → `VM.Model.ProfessionalId`.
- Interactores/repositorios de `MedicalAppointments`/`MedicalVisit` que referencian `DoctorId`: rename de propiedad únicamente, sin cambio de comportamiento.
- `MedRec.IoC.DependencyContainer.AddAppServices()`: nuevo bloque `Professionals` (DataContext → Repositories → UseCases con proxy → Presenters → ViewModels).
- `NavMenu.razor`: nuevo ítem "Profesionales" con guard `Professionals_View`.

## Permisos nuevos

`SystemPermissions`: agregar `Professionals_View`/`Professionals_Create`/`Professionals_Edit`/`Professionals_Delete` (mismo patrón que `Users_*`/`Roles_*`), incluidos en `All` para que aparezcan como checkboxes al crear/editar Roles.

## Modelo de datos y migración

```
Doctors (rename) → Professionals
  + Type (int, NOT NULL, DEFAULT 0)          -- 0 = Doctor
  ~ LicenseNumber (nullable ahora)
  ~ SpecialtyId (nullable ahora)

Users
  DoctorId → ProfessionalId (rename de columna)

MedicalAppointments
  DoctorId → ProfessionalId (rename de columna, sigue NOT NULL)

PatientMedicalVisits
  DoctorId → ProfessionalId (rename de columna, sigue nullable)
```

Migración `AddProfessionalsWithTypes` (`dotnet ef migrations add AddProfessionalsWithTypes --project "4.Framework&Drivers\MedRec.DataContext.MySql" --startup-project "4.Framework&Drivers\MedRec.WPF.UI"`): solo `RenameTable`/`RenameColumn`/`AlterColumn`, sin `DROP`, sin pérdida de datos. Todo registro existente en `Doctors` queda con `Type = Doctor` por el default, que es correcto porque hoy solo hay médicos en esa tabla.

## Flujo de alta combinada (Profesional + Usuario)

1. `CreateProfessionalPage`: Nombre, Apellido, Email, Teléfono, Fecha de ingreso, Tipo (select). Según el `Type` elegido, `ProfessionalTypeFields` muestra Matrícula (Doctor/Enfermero) y/o Especialidad (solo Doctor).
2. Checkbox "Crear también un usuario para este profesional". Al tildarlo aparecen Contraseña temporal + `RoleCheckboxList`; Email y Nombre completo del usuario se toman del profesional y se muestran de solo lectura.
3. Al guardar, `CreateProfessionalOrchestrator`:
   a. Ejecuta `CreateProfessionalAction` (interactor de Professionals, con su propia transacción).
   b. Si se pidió usuario, ejecuta `CreateUserAction` (interactor de Identity ya existente, pasándole `ProfessionalId` = el recién creado).
   c. Si (b) falla, ejecuta una acción de compensación que elimina el profesional creado en (a), y propaga el error original al usuario.
4. Guard de permiso `Professionals_Create` oculta el ítem de menú, el botón "+ Nuevo profesional" y bloquea el acceso directo a la página, igual que el patrón ya aplicado en Usuarios.

## Testing

- Nuevo proyecto `Test\MedRec.Professionals.UseCases.Tests` (xUnit + Moq), mismo patrón que `MedRec.MedicalVisit.UseCases.Tests`: alta Doctor con/sin especialidad (falla), alta Enfermero con/sin matrícula (falla), alta Recepcionista/Administrador (sin exigir matrícula/especialidad), edición, soft-delete, listado filtrado por tipo.
- Tests del `CreateProfessionalOrchestrator`: éxito con y sin usuario, y el camino de compensación (falla la creación del usuario → el profesional recién creado queda eliminado).

## Decisiones consideradas y diferidas

**Multi-tenant por profesional (aislamiento de pacientes + derivación):** cada profesional vería solo sus propios pacientes/historias, con un mecanismo para derivar un paciente a otro profesional del sistema. Se difiere a una spec propia porque toca autorización/visibilidad en `Patient`, `PatientMedicalHistory`, `PatientMedicalVisit` y `MedicalAppointment` — una superficie de cambio mucho mayor que este CRUD, y este spec ya deja listo el catálogo de profesionales con tipos que esa spec futura va a necesitar como base.

**Entidad `Institution`/`Clinica` (multi-tenant por institución, para una futura migración a app web con base compartida):** se evaluó agregarla ahora "por las dudas", pero se decide no hacerlo. Motivos:
- Hoy no hay ningún consumidor real: sería una entidad y una FK sin ningún flujo que la use, el mismo patrón de código muerto especulativo que el diagnóstico de arquitectura del proyecto (`DIAGNOSTICO_ARQUITECTURA.md`) ya viene marcando como hallazgo a evitar.
- Es una migración a medias: para que sirva de verdad hace falta definir la estrategia completa (¿`InstitutionId` en cada entidad?, ¿filtro global de EF?, ¿`ICurrentInstitutionContext`?, ¿un profesional pertenece a una sola institución o a varias?) — decisiones que no se pueden tomar bien hoy sin el contexto real de una migración a web, así que lo que se agregue ahora probablemente haya que rehacerlo entonces.
- El aislamiento entre clínicas ya existe hoy a nivel de infraestructura (una base de datos MySQL distinta por instalación), que es una estrategia de tenancy válida y ya funcionando — no hay una necesidad de negocio actual sin resolver.

Cuando se decida encarar la migración a web, esa es su propia iniciativa de arquitectura (amerita diseño dedicado, tocando todo el modelo de dominio, no solo `Professionals`), no un agregado incremental a este CRUD.
