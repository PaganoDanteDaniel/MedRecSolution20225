using MedRec.BusinessObjects.Interfaces; // Ajusta si ICommonOutputPort vive en otro namespace
using MedRec.Shared.ErrorHandling;
using System.Reflection;

namespace MedRec.BusinessObjects.ErrorHandling;

/// <summary>
/// Proxy dinámico que envuelve cualquier interfaz de caso de uso (InputPort).
/// Objetivos:
/// 1. Ejecutar el método real del interactor.
/// 2. Interceptar excepciones (sin duplicar try/catch en cada caso de uso).
/// 3. Mapear la excepción a <see cref="ErrorInfo"/> usando <see cref="IExceptionToErrorInfoMapper"/>.
/// 4. Invocar <c>ErrorAsync(ErrorInfo)</c> sobre el OutputPort asociado (presenter).
/// 5. Preservar el resultado de métodos que devuelven <c>Task&lt;T&gt;</c> (si no hubo excepción).
/// 6. Opcionalmente relanzar la excepción (telemetría / logs) si <c>_rethrow = true</c>.
///
/// Limitaciones:
/// - Si el método devuelve <c>Task&lt;T&gt;</c> y ocurre excepción con <c>rethrow = false</c>, el caller recibe <c>default(T)</c>.
/// - Para mantener el flujo simple, la llamada a <c>ErrorAsync</c> se fuerza de manera sincrónica (GetAwaiter().GetResult()).
/// - Si existe más de un OutputPort registrado, se toma el primero encontrado (campo, propiedad o ServiceProvider).
/// </summary>
internal class UseCaseExceptionProxy<TPort> : DispatchProxy where TPort : class
{
    // Instancia real del interactor (implementación del InputPort).
    private TPort _target = default!;

    // Componente que convierte excepciones en ErrorInfo uniformes.
    private IExceptionToErrorInfoMapper _mapper = default!;

    // ServiceProvider para fallback de resolución del OutputPort si no se encuentra por reflexión.
    private IServiceProvider _services = default!;

    // Indica si tras manejar la excepción se debe relanzar (útil para telemetría externa).
    private bool _rethrow;

    /// <summary>
    /// Configura el proxy con sus dependencias. Debe llamarse inmediatamente después de crearlo.
    /// </summary>
    /// <param name="target">Implementación concreta del InputPort.</param>
    /// <param name="mapper">Mapper de excepciones a ErrorInfo.</param>
    /// <param name="services">ServiceProvider para buscar OutputPort si no está embebido.</param>
    /// <param name="rethrow">Si true, la excepción se relanza tras notificar al presenter.</param>
    public void Configure(
        TPort target,
        IExceptionToErrorInfoMapper mapper,
        IServiceProvider services,
        bool rethrow = false)
    {
        _target = target;
        _mapper = mapper;
        _services = services;
        _rethrow = rethrow;
    }

    /// <summary>
    /// Punto central de interceptación. Se ejecuta cada vez que se invoca un método de la interfaz proxificada.
    /// - Determina el tipo de retorno (Task, Task&lt;T&gt;, sincrónico).
    /// - En caso de excepción directa (TargetInvocationException o genérica) delega al manejador uniforme.
    /// </summary>
    protected override object? Invoke(MethodInfo targetMethod, object?[]? args)
    {
        if (_target is null)
            throw new InvalidOperationException("Proxy no configurado.");

        try
        {
            // Invoca el método real sobre la implementación.
            var result = targetMethod.Invoke(_target, args);

            // Método async sin resultado (Task).
            if (result is Task task && targetMethod.ReturnType == typeof(Task))
                return InterceptTask(task);

            // Método async con resultado (Task<T>).
            if (result is Task genericTask &&
                targetMethod.ReturnType.IsGenericType &&
                targetMethod.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                var innerType = targetMethod.ReturnType.GenericTypeArguments[0];
                return InterceptGenericTask(genericTask, innerType);
            }

            // Método sincrónico (poco frecuente en casos de uso, pero soportado).
            return result;
        }
        catch (TargetInvocationException tie)
        {
            // Desempaquetar excepción real y convertirla.
            return HandleExceptionReturnDefault(tie.InnerException ?? tie, targetMethod.ReturnType);
        }
        catch (Exception ex)
        {
            // Excepción directa (reflexión / otros).
            return HandleExceptionReturnDefault(ex, targetMethod.ReturnType);
        }
    }

    /// <summary>
    /// Intercepta ejecución de un método async sin valor de retorno (Task).
    /// </summary>
    private async Task InterceptTask(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            HandleException(ex);
            if (_rethrow) throw;
        }
    }

    /// <summary>
    /// Prepara la interceptación de un método async con retorno genérico (Task&lt;T&gt;).
    /// Usa reflexión para crear el método cerrado adecuado.
    /// </summary>
    private object InterceptGenericTask(Task task, Type innerType)
    {
        var method = typeof(UseCaseExceptionProxy<TPort>)
            .GetMethod(nameof(InterceptGenericTaskCore), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(innerType);

        return method.Invoke(this, new object[] { task })!;
    }

    /// <summary>
    /// Implementación real de la interceptación para Task&lt;T&gt;.
    /// Devuelve el valor si éxito; default(T) si hubo excepción y no se relanza.
    /// </summary>
    private async Task<T> InterceptGenericTaskCore<T>(Task task)
    {
        try
        {
            await task.ConfigureAwait(false);
            return ((Task<T>)task).Result;
        }
        catch (Exception ex)
        {
            HandleException(ex);
            if (_rethrow) throw;
            return default!;
        }
    }

    /// <summary>
    /// Maneja una excepción:
    /// 1. Mapea Exception → ErrorInfo con el mapper.
    /// 2. Localiza el OutputPort (presenter).
    /// 3. Invoca ErrorAsync(ErrorInfo).
    /// No relanza a menos que _rethrow == true (eso se decide fuera de este método).
    /// </summary>
    private void HandleException(Exception ex)
    {
        var error = _mapper.Map(ex);
        var outputPort = FindOutputPort();
        if (outputPort is null) return;

        var errorAsync = outputPort.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(m => m.Name == "ErrorAsync" && m.GetParameters().Length == 1);

        if (errorAsync is null) return;

        try
        {
            var task = (Task?)errorAsync.Invoke(outputPort, new object?[] { error });
            if (task is not null)
                // Sincrónico para simplificar flujo en capa UI (Blazor Hybrid / WPF).
                task.GetAwaiter().GetResult();
        }
        catch
        {
            // Se silencian errores del presenter para no perder la excepción original del caso de uso.
        }
    }

    /// <summary>
    /// Post-procesa una excepción ocurrida en un método y devuelve un valor "neutro"
    /// acorde al tipo de retorno (null, Task.CompletedTask, Task&lt;default(T)&gt;, default struct).
    /// Si _rethrow == true relanza tras invocar ErrorAsync.
    /// </summary>
    private object? HandleExceptionReturnDefault(Exception ex, Type returnType)
    {
        HandleException(ex);
        if (_rethrow) throw ex;

        // Método void
        if (returnType == typeof(void))
            return null;

        // Método Task
        if (returnType == typeof(Task))
            return Task.CompletedTask;

        // Método Task<T>: crear un Task completado con default(T)
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
        {
            var innerType = returnType.GenericTypeArguments[0];
            var tcsType = typeof(TaskCompletionSource<>).MakeGenericType(innerType);
            dynamic tcs = Activator.CreateInstance(tcsType)!;
            var defaultValue = innerType.IsValueType ? Activator.CreateInstance(innerType) : null;
            tcs.SetResult(defaultValue);
            return tcs.Task;
        }

        // Método sincrónico que retorna T
        return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
    }

    /// <summary>
    /// Intenta localizar el OutputPort asociado para notificar errores.
    /// Orden de búsqueda:
    /// 1. Campos (privados/públicos) que implementen ICommonOutputPort.
    /// 2. Propiedades (privadas/públicas) que implementen ICommonOutputPort.
    /// 3. ServiceProvider (primer registro de ICommonOutputPort).
    /// Devuelve null si no se encuentra.
    /// </summary>
    private object? FindOutputPort()
    {
        // Campo privado/público
        var field = _target.GetType()
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .FirstOrDefault(f => typeof(IBaseOutputPort).IsAssignableFrom(f.FieldType));
        if (field is not null) return field.GetValue(_target);

        // Propiedad
        var prop = _target.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .FirstOrDefault(p => typeof(IBaseOutputPort).IsAssignableFrom(p.PropertyType));
        if (prop is not null) return prop.GetValue(_target);

        // Fallback: ServiceProvider
        return _services.GetService(typeof(IBaseOutputPort));
    }
}