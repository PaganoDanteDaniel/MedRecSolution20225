# Diagnóstico de arquitectura — MedRecSolution2025

> Generado: 2026-08-04 · Rama en la que se generó: `ModificacionCreateMedicalVisit`
>
> Revisión de la solución completa (45 proyectos referenciados en `MedRecSolution2025.sln`, capas 1-4)
> contra el patrón Ports & Adapters documentado en `CLAUDE.md`. Cada hallazgo fue verificado leyendo el
> código real (no solo grep) y, donde aplicaba, compilando el proyecto involucrado para confirmar impacto.
>
> **Cómo usar este archivo:** marcar el checkbox al resolver cada punto. Los ítems están ordenados por
> impacto, no por facilidad — pero el orden sugerido de ataque está en la sección final.

## Conclusión general

El patrón de base está bien elegido y, donde se aplica, se aplica bien (verificado en
`CreateMedicalVisitInteractor` y en los orquestadores de MedicalVisit/MedicalAppointments — son ejemplos
correctos, sin sobre-ingeniería). El problema no es la arquitectura en sí, es **deuda de ejecución**: código
muerto sin limpiar, una feature que no siguió el patrón central de errores, y un bug funcional real en la
infraestructura compartida de validación.

---

## 1. [ALTO] Los 4 Interactores de DynamicTemplates reimplementan el manejo de errores a mano

- [x] Resuelto (rama `CorreccionesDiagnosticoArquitectura`)

**Dónde:**
- `2.ApplicationBusinessObjects\MedRec.DynamicTemplates.UseCases\Implementations\SaveDynamicFieldsInteractor.cs`
- `2.ApplicationBusinessObjects\MedRec.DynamicTemplates.UseCases\Implementations\GetTemplateFieldsBySpecialtyInteractor.cs`
- `2.ApplicationBusinessObjects\MedRec.DynamicTemplates.UseCases\Implementations\GetDynamicFieldsByVisitInteractor.cs`
- `2.ApplicationBusinessObjects\MedRec.DynamicTemplates.UseCases\Implementations\GetActiveSpecialtiesInteractor.cs` (revisar, mismo patrón probable)

**Qué rompe:** cada uno envuelve todo el `Handle` en `try { ... } catch (Exception ex) { await _outputPort.ErrorAsync(new ErrorInfo { Code = ErrorCode.DatabaseError, ... }) }`.
Esto evita que la excepción llegue nunca al `UseCaseExceptionProxy` / `DefaultExceptionToErrorInfoMapper`, que es
el único lugar de la solución que debería clasificar excepciones de infraestructura
(`ConcurrencyException`→409, `DuplicateKeyException`→409, etc.).

**Por qué importa:** cualquier error real (conflicto de concurrencia, clave duplicada, pérdida de conexión) se
aplana a un genérico "Error al guardar los campos dinámicos" en vez del mensaje/acción de UI específica que
`BaseOutputPort<T>.ErrorAsync` ya sabe generar (`ShowConcurrencyMessage`, `ShowWarning`, etc.). El usuario ve un
mensaje incorrecto y se pierde información real del error.

**Bonus encontrado de paso:** los strings tienen mojibake (`"Validaci�n"`, `"Operaci�n cancelada"`,
`"din�micos"`) — los archivos se guardaron con un encoding distinto al resto del repo (no UTF-8), así que esos
textos se ven rotos en la UI.

**Qué cambiar:** sacar los `try/catch` de infraestructura de los 4 interactores (dejar solo el flujo
`Validate → ValidationErrorsAsync/return → ExecuteInTransactionWithRetry → outputPort.Handle`, igual que
`CreateMedicalVisitInteractor.cs`, que está limpio). Re-guardar los archivos en UTF-8. Confirmar que
`AddDynamicTemplatesUseCasesWithProxy` siga siendo el registro real usado por `MedRec.IoC` (ya lo es).

---

## 2. [ALTO] Bug funcional confirmado en las Guard clauses compartidas

- [x] Resuelto (rama `CorreccionesDiagnosticoArquitectura`)

**Dónde (alcance ampliado tras revisar el código, era más que los 3 archivos originales):**
- `1.EnterpriseBusinessObjects\MedRec.Shared\Gruards\GuardBuilderInt.cs`
- `1.EnterpriseBusinessObjects\MedRec.Shared\Gruards\GuardBuilderDateTime.cs`
- `1.EnterpriseBusinessObjects\MedRec.Shared\Gruards\GuardBuilderEnum.cs`
- `1.EnterpriseBusinessObjects\MedRec.Shared\Gruards\GuardBuilderString.cs` (mismo bug, no estaba en el diagnóstico original)
- `1.EnterpriseBusinessObjects\MedRec.Shared\Gruards\GuardBuilderDecimal.cs` (mismo bug + mojibake en los mensajes, corregido también)

`GuardBuilderGuid.cs` estaba limpio (no sombreaba `_paramName`, no lo necesitaba en sus mensajes).

**Qué rompe:** cada uno redeclara un campo propio `private readonly string _paramName`, que **oculta** (shadow)
el `_paramName` heredado de `GuardBuilderBase<T>` y nunca se asigna. Confirmado por warnings del compilador:
`CS0108` ("oculta el miembro heredado... use `new` si su intención era ocultarlo") y `CS0649` ("nunca se le
asigna un valor y siempre tendrá el valor predeterminado null").

**Por qué importa:** como esta clase es la base de `Validator` de cada feature, todo mensaje de validación
generado por estos 3 builders (`LessThan`, `GreaterThan`, `NotEqualTo`, comparaciones de fecha, de enum) muestra
literalmente **"El parámetro 'null' debe ser mayor que..."** en vez del nombre real del campo. Es un bug
funcional visible para el usuario final, en infraestructura compartida por toda la solución.

**Qué cambiar:** borrar el campo `_paramName` local en las tres subclases y usar el heredado de
`GuardBuilderBase<T>` (que sí se asigna correctamente en el constructor base).

---

## 3. [MEDIO] Dos implementaciones completas de la capa de datos, comprometidas en git pero fuera del `.sln`

- [ ] Resuelto

**Dónde:**
- `4.Framework&Drivers\MedRec.DataContext.EF\` (DbContext, Configurations, Migrations propias)
- `4.Framework&Drivers\MedRec.Patients.DataContext.EF\`

Ambas con archivos `.cs` reales trackeados en git (`git ls-files` las lista completas), **ninguna referenciada
en `MedRecSolution2025.sln`**. Lo que sí usa el composition root (`MedRec.IoC`) es `MedRec.DataContext.MySql` /
`MedRec.Patients.DataContext.MySql`.

**Por qué importa:** es deuda real, no caché de build. Alguien migró de un `DbContext` "EF genérico" a uno
específico de MySQL/Pomelo y dejó el original completo (con sus propias migraciones) en el repo. Cualquiera que
abra la carpeta puede pensar que ahí vive la fuente de verdad de EF — el propio `CLAUDE.md` menciona "las
migraciones viven en `MedRec.DataContext.MySql` y en `MedRec.DataContext.EF`" sin aclarar que la segunda está
desconectada del build real.

**Qué cambiar:** si de verdad no se usa, eliminar ambas carpetas del repo (no solo del `.sln`) y actualizar
`CLAUDE.md` para que no mencione ese directorio como si fuera parte activa del esquema.

---

## 4. [MEDIO] `OperationResult<T>` duplicado, viviendo en namespace global

- [x] Resuelto (rama `CorreccionesDiagnosticoArquitectura`) — archivo borrado, confirmado que compila.

**Dónde:** `3.InterfaceAdapters\MedRec.MedicalAppointments.ViewModels\Orchestration\OperationResult.cs`

**Qué rompe:** es una segunda definición completa de `OperationResult<T>`, con el `namespace` **comentado**
(`/*namespace MedRec.MedicalAppointments.ViewModels.Orchestration*/;`), lo que la deja viviendo en el namespace
global del ensamblado. Le falta `MessageAction` y el factory `Validation(...)` que sí tiene la canónica
(`MedRec.BusinessObjects.Results.OperationResult<T>`).

**Por qué importa:** viola la regla explícita del propio estándar del proyecto ("debe haber una sola fuente de
verdad para `OperationResult<T>` en toda la solución"). Se confirmó compilando el proyecto que no rompe el build
hoy (todos los `Actions`/`Orchestrator` de esa feature importan explícitamente la canónica), así que es código
muerto, no un bug activo — pero es un riesgo latente de ambigüedad si algún archivo nuevo de esa carpeta omite
el `using`.

**Qué cambiar:** borrar el archivo. Cambio de un minuto, cero riesgo (nada lo referencia activamente).

---

## 5. [BAJO-MEDIO] Métodos de registro DI "sin proxy" muertos, guardados "por si acaso"

- [ ] Resuelto

**Dónde:** `AddPatientUseCasesServices` (`MedRec.Patients.UseCases\DependencyContainer.cs`),
`AddMedicalVisitUseCasesServices` (`MedRec.MedicalVisit.UseCases\DependencyContainer.cs`),
`AddDynamicTemplatesUseCasesDirect` (`MedRec.DynamicTemplates.UseCases\DependencyContainer.cs`), y análogos en
HealthInsurance/MedicalAppointments.

**Qué rompe:** ninguno se invoca desde `MedRec.IoC\DependencyContainer.cs`, que siempre usa la variante
`...WithProxy`. Es YAGNI de manual: cada feature carga un método de registro paralelo, comentado como
"reservado para futura API"/"para testing", que nadie llama.

**Por qué importa:** hay que mantenerlo sincronizado a mano cada vez que se agrega un caso de uso, y ya hoy hay
features donde ese método directo no tiene todos los casos de uso que sí tiene el `...WithProxy` — es una lista
que diverge en silencio y puede inducir a error si alguna vez alguien lo usa pensando que está actualizado.

**Qué cambiar:** si no hay un plan concreto de usar registro sin proxy (ej. tests unitarios de integración de
DI), borrar esos métodos. Si hace falta para tests, dejar uno solo genérico compartido, no uno por feature.

---

## 6. [BAJO] Limpieza menor

- [x] `UpdatePatientInteractor.Handle` (`MedRec.Patients.UseCases\Implementations\UpdatePatientInteractor.cs`):
  borrado el `try { ... } catch { throw; }` que no hacía nada. Resuelto (rama `CorreccionesDiagnosticoArquitectura`).
- [x] `MedRec.Common.Repositories` (`3.InterfaceAdapters\Repositories\MedRec.Common.Repositories\`): `.csproj`
  vacío borrado de git y del disco. Resuelto.
- [x] Carpetas `MedRec.InsuranceHealth.*` bajo `2.ApplicationBusinessObjects\`: residuos locales de `obj/`
  eliminados (no estaban en git). Resuelto.

---

## 7. [ALTO] `MedRec.MedicalVisit.UseCases.Tests` no compila contra `master`

- [x] Resuelto (rama `CorreccionesDiagnosticoArquitectura`)

**Dónde:** `Test\MedRec.MedicalVisit.UseCases.Tests\CreateMedicalVisitInteractorUoWTests.cs`

**Encontrado el:** 2026-08-04, al correr `dotnet test` tras resolver los puntos 1, 2, 4 y 6 en la rama
`CorreccionesDiagnosticoArquitectura`. Verificado que **no es una regresión de esta rama**: reproducido el mismo
error haciendo `git stash` y compilando el proyecto de test tal cual quedó en `master` tras el merge del PR #12.

**Qué rompe:** el PR #12 (rama `ModificacionCreateMedicalVisit`) cambió la firma de
`CreateMedicalVisitInteractor` — agregó el parámetro `dynamicFieldCommandRepository` al constructor y cambió la
firma de `ICreateMedicalVisitOutputPort.Handle` (ahora requiere el parámetro `visit`) — pero el archivo de test
no se actualizó. `dotnet build` de la solución compila bien (el proyecto de test no forma parte del `.sln` build
por defecto en ese sentido, o el orden de build lo tolera), pero `dotnet test` falla con 8 errores `CS7036`
("no se ha dado ningún argumento que corresponda al parámetro requerido").

**Por qué importa:** es el único proyecto de test que existe en la solución (`CreateMedicalVisitInteractorUoWTests`
sobre el interactor "ejemplo" que el propio `CLAUDE.md` señala como el caso limpio a imitar). Ahora mismo no hay
manera de correr esa suite — cualquier regresión futura en `CreateMedicalVisitInteractor` pasaría desapercibida.

**Qué cambiar:** actualizar los ~8 call-sites del test para pasar el nuevo `dynamicFieldCommandRepository` mock
al constructor y el nuevo argumento `visit` a `outputPort.Handle(...)`. Requiere revisar qué contrato espera
`dynamicFieldCommandRepository` (mock de `IMedicalVisitDynamicFieldCommandRepositoryUoW`) para no solo hacer
compilar sino mantener la intención original de cada test.

**Alcance real (mayor al descrito arriba, descubierto al ejecutar):** el problema no era solo de firma —
`CreateMedicalVisitInteractorUoWTests.cs` mockeaba `BeginTransaction`/`CommitTransaction`/`RollbackTransaction`
directamente sobre `IRepositoryUnitOfWork`, pero el interactor actual ya no llama a esos métodos: delega todo a
`unitOfWork.ExecuteInTransactionWithRetry(...)`, que hay que mockear para que **invoque el delegate recibido**
(si no, ninguna llamada interna del `Handle` ocurre y los asserts fallan en silencio con "0 invocations"). Dos
tests (`DuplicateKeyException`/`ConcurrencyException`) asumían que el interactor todavía atrapaba esas excepciones
y las traducía a `ErrorInfo` — comportamiento que ya no existe ahí (correctamente, según el punto 1: eso ahora es
responsabilidad del `UseCaseExceptionProxy`). Se reescribieron para verificar que la excepción se **propaga sin
ser tragada**, en vez de verificar un mapeo a `ErrorInfo` que ya no ocurre a ese nivel.

Se encontró el mismo problema (mismo síntoma, mismo interactor no relacionado) en
`GetMedicalHistoryIdInteractorUoWTests.cs` (4 tests, sobre `GetMedicalHistoryIdInteractor`): mismo fix de mockear
`ExecuteInTransactionWithRetry`, mismo cambio de "atrapa DuplicateKeyException y reintenta lectura" a "propaga sin
atrapar". Se eliminó `Handle_ShouldReturnValidationError_WhenPatientIdIsEmpty`: el interactor actual **no valida**
`patientId == Guid.Empty` en absoluto (el archivo `...InteractorUoW.cs` original que sí lo hacía fue borrado y
reemplazado). No se repuso esa validación porque cambiar comportamiento de producción está fuera del alcance de
"arreglar tests" — queda anotado como posible hallazgo nuevo (punto 8) para decidir aparte.

Ambos archivos, 8 tests en total, pasan tras el fix (`dotnet test` → `Correctas!`).

---

## 8. [MEDIO, nuevo] `GetMedicalHistoryIdInteractor` ya no valida `patientId == Guid.Empty`

- [ ] Resuelto

**Dónde:** `2.ApplicationBusinessObjects\MedRec.MedicalVisit.UseCases\Implementations\GetMedicalHistoryIdInteractor.cs`

**Encontrado el:** 2026-08-04, al resolver el punto 7 — el test `Handle_ShouldReturnValidationError_WhenPatientIdIsEmpty`
verificaba una validación (`ErrorCode.ValidationError`, HTTP 400 cuando `patientId == Guid.Empty`) que existía en
el interactor viejo (`GetMedicalHistoryIdInteractorUoW.cs`, borrado en el PR #12) pero **no se migró** al
interactor nuevo. Se eliminó el test en vez de inventar la validación, porque cambiar comportamiento de
producción no es parte de "arreglar un test roto".

**Qué rompe (potencial, no confirmado con un caso real en producción):** si algo aguas arriba (Orchestrator/Action)
no garantiza que `patientId` nunca llegue vacío, el interactor haría una consulta con `Guid.Empty` y,
dependiendo de qué devuelva `GetMedicalHistory` para ese caso, podría intentar crear un historial clínico
"huérfano" en vez de fallar con un error claro.

**Qué revisar antes de decidir el fix:** confirmar en `GetMedicalHistoryAction`/`CreateMedicalVisitOrchestrator`
si ya existe una guarda contra `patientId` vacío más arriba en el flujo (en cuyo caso la validación en el
interactor era redundante y se puede omitir a propósito) o si de verdad quedó un hueco a tapar con un
`Guard.For(patientId, ...).NotNullOrEmpty()` al inicio del `Handle`.

---

## Respuestas directas a las preguntas originales

- **¿Está bien estructurada?** Sí — la separación en 4 capas con dependencia unidireccional se respeta, y el
  mecanismo Interactor/Ports/`OperationResult<T>`/`UseCaseExceptionProxy` está bien pensado para un desktop app
  que va a seguir creciendo en feature slices.
- **¿Sobre-ingeniería?** No en el patrón central (los Orchestrator+Actions revisados están genuinamente
  componiendo pasos, no son ceremonia). La sobre-ingeniería puntual encontrada es el punto 5 (métodos DI "por si
  acaso").
- **¿Eficiencia de recursos?** No se detectaron problemas de performance en runtime — `UseCaseExceptionProxy`
  usa reflexión solo una vez, al armar el contenedor de DI, no en cada llamada. El costo real está en el peso
  muerto del repo (migraciones EF duplicadas, proyectos huérfanos: puntos 3 y 6), que no afecta al usuario final
  pero sí al build y a la carga cognitiva de quien mantiene esto.

## Orden sugerido de ataque

1. ~~**Punto 2** — bug de validación, afecta producción hoy, fix de minutos.~~ ✅ Resuelto.
2. ~~**Punto 1** — rompe clasificación de errores en la feature que se está tocando ahora mismo.~~ ✅ Resuelto.
3. ~~**Puntos 4 y 6** — limpieza de riesgo cero.~~ ✅ Resueltos.
4. ~~**Punto 7** — sin esto no había forma de verificar con tests los cambios de `CreateMedicalVisit`.~~ ✅ Resuelto.
5. **Punto 8** (nuevo) — decidir si falta reponer la validación de `patientId` vacío o si es redundante por diseño.
6. **Punto 5** — decisión de diseño (¿se necesita el registro sin proxy para algo? si no, borrar).
7. **Punto 3** — requiere confirmar con el equipo que `MedRec.DataContext.EF` / `MedRec.Patients.DataContext.EF`
   realmente no se usan en ningún flujo antes de borrarlos.
