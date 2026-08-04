# CLAUDE.md

Este archivo brinda guía a Claude Code (claude.ai/code) para trabajar con el código de este repositorio.

## Descripción general del proyecto

MedRecSolution2025 es una aplicación de escritorio para historias clínicas (host WPF que ejecuta una UI Blazor Hybrid) construida sobre .NET 9, usando MySQL (vía EF Core / Pomelo) como persistencia. El código está organizado como una solución estricta de **Clean Architecture** (Uncle Bob), dividida en cuatro capas numeradas, cada una en su propio directorio en la raíz de la solución, donde las dependencias solo apuntan hacia adentro.

## Comandos de build y test

Ejecutar todos los comandos desde la raíz del repositorio (`MedRecSolution2025.sln`). Requiere el SDK de .NET 9 (Windows, ya que el proyecto de UI apunta a `net9.0-windows7.0` y usa WPF).

```powershell
# Restaurar y compilar toda la solución
dotnet build MedRecSolution2025.sln

# Compilar un solo proyecto
dotnet build "2.ApplicationBusinessObjects\MedRec.MedicalVisit.UseCases\MedRec.MedicalVisit.UseCases.csproj"

# Correr todos los tests
dotnet test "Test\MedRec.MedicalVisit.UseCases.Tests\MedRec.MedicalVisit.UseCases.Tests.csproj"

# Correr un test puntual
dotnet test "Test\MedRec.MedicalVisit.UseCases.Tests\MedRec.MedicalVisit.UseCases.Tests.csproj" --filter "FullyQualifiedName~CreateMedicalVisitInteractorUoWTests"

# Ejecutar la app WPF
dotnet run --project "4.Framework&Drivers\MedRec.WPF.UI\MedRec.WPF.UI.csproj"
```

Los tests usan **xUnit** + **Moq**. Actualmente solo existe `MedRec.MedicalVisit.UseCases.Tests` (tests a nivel interactor contra repositorios Unit-of-Work mockeados); los nuevos proyectos de test para otras feature slices deberían seguir la misma convención de nombre (`MedRec.<Feature>.UseCases.Tests`) y ubicarse bajo `Test\`.

### Migraciones de EF Core

Las migraciones viven en `4.Framework&Drivers\MedRec.DataContext.MySql\Migrations` (esquema central/compartido) y en `4.Framework&Drivers\MedRec.DataContext.EF\Migrations`. `DesignTimeDbContextFactory` en `MedRec.DataContext.MySql\DataContext` provee una cadena de conexión local de desarrollo para el tooling de diseño de `dotnet ef` — al correr comandos `dotnet ef`, apuntar `--project` a `MedRec.DataContext.MySql` y `--startup-project` a `MedRec.WPF.UI`.

## Arquitectura: las cuatro capas

Los directorios están numerados para forzar la dirección de las dependencias — una capa solo puede referenciar capas de número igual o menor.

1. **`1.EnterpriseBusinessObjects`** — Reglas de negocio a nivel empresa, sin dependencias de framework.
   - `MedRec.Entity`: entidades de dominio POCO (`POCOEntities`), DTOs compartidos, enums, e interfaces de repositorio/servicio (`Interfaces`) que las capas externas implementan. También contiene `Results\Result.cs` / `Result{T}.cs`, un tipo Result más simple usado a nivel entidad (distinto de `OperationResult<T>` mencionado más abajo).
   - `MedRec.Shared`: incumbencias transversales — `Exceptions` (incluyendo `SQLExceptions` para clasificar errores de BD como `ConcurrencyException`, `DuplicateKeyException`, `LostConnectionException`), `ErrorHandling\IExceptionToErrorInfoMapper`, `Gruards` (una API fluida `Guard`/`GuardBuilder*` para validación de argumentos — notar que la carpeta está mal escrita como "Gruards"), y `Security` (`EncryptionHelper`, `Jwt`).
   - `MedRec.Validator`: abstracciones genéricas de validación (`IModelValidatorHub<T>`) y value objects, implementados por feature en la carpeta `Validator`/`Validators` de cada proyecto `*.BusinessObjects`.

2. **`2.ApplicationBusinessObjects`** — Casos de uso (reglas de negocio específicas de la aplicación), un par de proyectos por feature slice (Patients, HealthInsurance, MedicalAppointments, MedicalVisit, DynamicTemplates):
   - **`MedRec.<Feature>.BusinessObjects`**: DTOs de la feature, interfaces de puertos de entrada/salida (`Interfaces\Ports\I<UseCase>InputPort` / `I<UseCase>OutputPort`), interfaces de repositorio UoW (`Interfaces\Repositories`), constraints y validadores.
   - **`MedRec.<Feature>.UseCases`**: interactores (`Implementations\<UseCase>Interactor.cs`) que implementan los input ports — la lógica real del caso de uso.
   - **`MedRec.BusinessObjects`**: infraestructura de la capa de aplicación compartida por todas las features — `Results\OperationResult<T>` (el wrapper de resultado principal que se devuelve hacia arriba a través de orquestadores/actions hasta la UI), `Abstracts\BaseOutputPort<T>`, `ErrorHandling\UseCaseExceptionProxy` (un `DispatchProxy` que envuelve cada interactor para capturar excepciones, mapearlas vía `IExceptionToErrorInfoMapper`, y enviar un `ErrorInfo` al output port del interactor en lugar de lanzar la excepción — ver `AddXxxUseCasesServicesWithProxy()` en el `DependencyContainer` de cada feature).

3. **`3.InterfaceAdapters`** — Adapta los casos de uso hacia la UI y hacia la persistencia. Por feature:
   - **`Presenters`**: implementan los output ports (`Implementations\<UseCase>Presenter.cs`), típicamente extendiendo `BaseOutputPort<T>` y traduciendo el resultado del interactor a un `OperationResult<T>`.
   - **`Repositories`**: implementan las interfaces de repositorio UoW de la capa 2 contra interfaces `IXxxDataContext` (mantenidas del lado del adaptador para que la capa 2 siga siendo agnóstica de la persistencia).
   - **`ViewModels`**: contiene `Models` (DTOs orientados a la UI), `VM` (view-models de componentes Blazor, ej. `CreateMedicalVisitVM`), y `Orchestration` — los orquestadores (`<UseCase>Orchestrator`, ej. `CreateMedicalVisitOrchestrator`) componen una o más **Actions** (`Orchestration\Actions\<Name>Action`, cada una envolviendo un par input/output port) para implementar un flujo de UI de varios pasos, devolviendo `OperationResult<T>` en todo el recorrido. `Orchestration\<Feature>Mapper.cs` mapea entre los Models de la VM y los DTOs del caso de uso.
   - **`Views`**: componentes/páginas Razor consumidos por el host WPF.
   - Nota: algunas feature slices más nuevas (DynamicTemplates) agrupan estos elementos bajo carpetas padre compartidas `Presenters\`, `Repositories\`, `ViewModels\`, `Views\` en lugar de carpetas prefijadas por proyecto — revisar el `.sln` para conocer las rutas exactas de los `.csproj`, ya que los nombres de carpeta no siempre coinciden con los nombres de proyecto (ej. `MedRec.HealthInsurance.UeseCases` y `MrdRec.HealthInsurance.Repositories` contienen errores de tipeo preservados por historia).

4. **`4.Framework&Drivers`** — Capa más externa: frameworks, base de datos y el composition root.
   - **`MedRec.DataContext.MySql`** / **`MedRec.DataContext.EF`**: `DbContext` compartido (`MedRecContext`, `MedRecContextMySql`), `Configurations` de EF (configuración fluida de entidades por tabla), `UnitOfWork\DataContextUnitOfWork` (implementa `IRepositoryUnitOfWork`, incluyendo `ExecuteInTransactionWithRetry`), y clasificación de excepciones de MySQL.
   - **`MedRec.<Feature>.DataContext.MySql`**: implementaciones `IXxxDataContext` por feature (wrappers delgados de queries/comandos EF) usados por el proyecto Repositories de esa feature.
   - **`MedRec.IoC`**: `DependencyContainer.AddAppServices()` — el único composition root que conecta, por feature, DataContext → Repositories → UseCases (con el proxy de excepciones) → Presenters → ViewModels, en ese orden. Al agregar una nueva feature slice, registrarla acá siguiendo el patrón de bloque por feature ya existente.
   - **`MedRec.WPF.UI`**: el host WPF (`net9.0-windows7.0`) que aloja un `BlazorWebView`; `appsettings.json` contiene configuración encriptada (prefijo `ENC:` vía DPAPI, ver `MedRec.Shared\Security\EncryptionHelper`) para el JWT y las cadenas de conexión a BD — nunca escribir secretos en texto plano acá.
   - **`MedRec.Setup`**, **`MedRec.Package`**: soporte de empaquetado/instalador para `MedRec.Installer.Bundle` / `MedRec.Installer.MSI` (instaladores basados en WiX en la raíz del repo).

## Convenciones a seguir al agregar una feature slice

- Replicar el patrón existente de cinco proyectos por capa (`BusinessObjects`, `UseCases` en la capa 2; `Presenters`, `Repositories`, `ViewModels`, `Views` en la capa 3; `DataContext.MySql` en la capa 4) en lugar de introducir una forma distinta.
- Los interactores reciben un `IXxxOutputPort` y uno o más `IXxxRepositoryUoW` vía DI por constructor primario; ante un fallo de validación llamar a `outputPort.ValidationErrorsAsync(...)` y retornar; ante éxito llamar a `outputPort.Handle(...)`. Envolver las escrituras de varios pasos en `unitOfWork.ExecuteInTransactionWithRetry(...)`.
- Registrar los nuevos casos de uso a través de `AddXxxUseCasesServicesWithProxy()` para que queden envueltos automáticamente por `UseCaseExceptionProxy` — no agregar try/catch manual en los interactores para excepciones de infraestructura.
- Orquestadores/Actions/Presenters devuelven todos `OperationResult<T>` (de `MedRec.BusinessObjects.Results`) hacia la capa de ViewModels — usar `OperationResult.Ok/Fail/Validation/Unknown/Cancelled`, no excepciones, para señalar resultados a la UI.
- Los nuevos registros de DI van en el `DependencyContainer.cs` propio de cada proyecto (cada capa/feature tiene el suyo), y luego deben encadenarse en `MedRec.IoC\DependencyContainer.AddAppServices()`.
- Los comentarios y mensajes de validación/error en este código están escritos en español (ej. `"Paciente no encontrado."`); seguir esa convención para texto orientado al usuario y para el estilo de comentarios existente.

## Particularidades de ortografía/nombres (intencionales, no "corregir" sin revisar sus usos)

- `1.EnterpriseBusinessObjects\MedRec.Shared\Gruards` (no "Guards").
- `2.ApplicationBusinessObjects\MedRec.HealthInsurance.UeseCases` (no "UseCases") — aunque el `.csproj` interno sí está correctamente nombrado `MedRec.HealthInsurance.UseCases.csproj`.
- `3.InterfaceAdapters\MrdRec.HealthInsurance.Repositories` (no "MedRec").
- `3.InterfaceAdapters\MedRec.MedicalAppointmnts.ViewModels` es una carpeta obsoleta/duplicada, distinta de `MedRec.MedicalAppointments.ViewModels`.
- `3.InterfaceAdapters\WiewModels` (error de tipeo de "ViewModels") duplica `3.InterfaceAdapters\ViewModels` para DynamicTemplates — revisar el `.sln` para saber cuál ruta está realmente referenciada antes de editar.
