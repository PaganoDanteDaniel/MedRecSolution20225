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

- [ ] Resuelto

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

- [ ] Resuelto

**Dónde:**
- `1.EnterpriseBusinessObjects\MedRec.Shared\Gruards\GuardBuilderInt.cs`
- `1.EnterpriseBusinessObjects\MedRec.Shared\Gruards\GuardBuilderDateTime.cs`
- `1.EnterpriseBusinessObjects\MedRec.Shared\Gruards\GuardBuilderEnum.cs`

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

- [ ] Resuelto

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

- [ ] `UpdatePatientInteractor.Handle` (`MedRec.Patients.UseCases\Implementations\UpdatePatientInteractor.cs`):
  tiene un `try { ... } catch { throw; }` que no hace nada (relanza sin transformar nada). Borrar el bloque y
  dejar el `await` directo dentro de `ExecuteInTransactionWithRetry`.
- [ ] `MedRec.Common.Repositories` (`3.InterfaceAdapters\Repositories\MedRec.Common.Repositories\`): `.csproj`
  vacío (sin una sola clase), comprometido en git, no referenciado en el `.sln`. Scaffold abandonado, borrar.
- [ ] Carpetas `MedRec.InsuranceHealth.*` bajo `2.ApplicationBusinessObjects\`: solo tienen residuos de `obj/`
  (no están en git), sobrantes de un rename `InsuranceHealth→HealthInsurance` sin limpiar localmente. Sin
  impacto real — un `git clean -xdf` selectivo en esas carpetas evita confusión al navegar el disco.

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

1. **Punto 2** — bug de validación, afecta producción hoy, fix de minutos.
2. **Punto 1** — rompe clasificación de errores en la feature que se está tocando ahora mismo (DynamicTemplates,
   rama `ModificacionCreateMedicalVisit`).
3. **Puntos 4 y 6** — limpieza de riesgo cero, se pueden hacer en cualquier momento libre.
4. **Punto 5** — decisión de diseño (¿se necesita el registro sin proxy para algo? si no, borrar).
5. **Punto 3** — requiere confirmar con el equipo que `MedRec.DataContext.EF` / `MedRec.Patients.DataContext.EF`
   realmente no se usan en ningún flujo antes de borrarlos.
