using MedRec.BusinessObjects.ErrorHandling;
using MedRec.Shared.ErrorHandling;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddUseCaseExceptionDecorators(
    this IServiceCollection services,
    IEnumerable<Assembly> assemblies,
    bool rethrow = false)
    {
        if (!services.Any(s => s.ServiceType == typeof(IExceptionToErrorInfoMapper)))
            services.AddScoped<IExceptionToErrorInfoMapper, DefaultExceptionToErrorInfoMapper>();

        var inputPortInterfaces = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsInterface && t.Name.EndsWith("InputPort"))
            .ToList();

        var concreteTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract)
            .ToList();

        foreach (var inputInterface in inputPortInterfaces)
        {
            var impl = concreteTypes
                .FirstOrDefault(c => inputInterface.IsAssignableFrom(c));

            if (impl is null) continue;

            services.AddScoped(impl);

            services.AddScoped(inputInterface, sp =>
            {
                var mapper = sp.GetRequiredService<IExceptionToErrorInfoMapper>();
                var target = sp.GetRequiredService(impl);
                return UseCaseExceptionProxyFactory.Create(
                    inputInterface,
                    target,
                    mapper,
                    sp,
                    rethrow
                );
            });
        }

        return services;
    }

}

