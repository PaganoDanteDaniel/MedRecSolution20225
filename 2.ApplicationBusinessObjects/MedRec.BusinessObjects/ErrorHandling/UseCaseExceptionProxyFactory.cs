using MedRec.Shared.ErrorHandling;
using System.Reflection;

namespace MedRec.BusinessObjects.ErrorHandling;
internal static class UseCaseExceptionProxyFactory
{
    /// <summary>
    /// Crea un proxy dinámico que implementa <paramref name="interfaceType"/>
    /// y usa <see cref="UseCaseExceptionProxy{T}"/> como manejador.
    /// </summary>
    public static object Create(
        Type interfaceType,
        object target,
        IExceptionToErrorInfoMapper mapper,
        IServiceProvider serviceProvider,
        bool rethrow = false)
    {
        if (!interfaceType.IsInterface)
            throw new ArgumentException("El tipo debe ser una interfaz.", nameof(interfaceType));

        var proxyType = typeof(UseCaseExceptionProxy<>).MakeGenericType(interfaceType);

        // Esta es la forma CORRECTA de crear el proxy
        var proxy = DispatchProxy.Create(interfaceType, proxyType);

        // Configurar el proxy (usamos reflexión porque no conocemos T en tiempo de ejecución)
        var configureMethod = proxyType.GetMethod(nameof(UseCaseExceptionProxy<object>.Configure))!;
        configureMethod.Invoke(proxy, new object[] { target, mapper, serviceProvider, rethrow });

        return proxy;
    }
}
