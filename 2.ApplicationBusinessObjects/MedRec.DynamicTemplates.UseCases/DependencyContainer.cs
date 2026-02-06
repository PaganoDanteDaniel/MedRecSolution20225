using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.UseCases.Implementations;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection container for DynamicTemplates UseCases
/// </summary>
public static class DependencyContainer
{
    /// <summary>
    /// Registers all use case interactors using the centralized exception proxy factory.
    /// This decorates InputPorts with the UseCaseExceptionProxy so exceptions are
    /// mapped to ErrorInfo and forwarded to the OutputPort (presenter) automatically.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="rethrow">If true, proxy will rethrow exceptions after notifying presenter</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDynamicTemplatesUseCases(this IServiceCollection services, bool rethrow = false)
    {
        // Register using the generic proxy decorator (recommended).
        return services.AddDynamicTemplatesUseCasesWithProxy(rethrow);
    }

    /// <summary>
    /// Registers all use case interactors using the UseCaseExceptionProxy decorator.
    /// This mirrors the approach used in Patients DependencyContainer: the concrete
    /// interactor types are added and the InputPort interfaces are resolved through
    /// the proxy that intercepts exceptions.
    /// </summary>
    public static IServiceCollection AddDynamicTemplatesUseCasesWithProxy(this IServiceCollection services, bool rethrow = false)
    {
        // Assemblies that contain the InputPort interfaces and their concrete implementations.
        // We include the assembly that defines the InputPort interfaces and the assembly
        // that contains the interactor implementations.
        var assemblies = new[]
        {
            typeof(IGetActiveSpecialtiesInputPort).Assembly,
            typeof(GetActiveSpecialtiesInteractor).Assembly
        };

        // Use the shared AddUseCaseExceptionDecorators extension to register proxies.
        return services.AddUseCaseExceptionDecorators(assemblies, rethrow);
    }

    /// <summary>
    /// Optional: direct registration without proxy (kept for compatibility/testing).
    /// Use AddDynamicTemplatesUseCases() to register with proxy by default.
    /// </summary>
    public static IServiceCollection AddDynamicTemplatesUseCasesDirect(this IServiceCollection services)
    {
        services.AddTransient<IGetActiveSpecialtiesInputPort, GetActiveSpecialtiesInteractor>();
        services.AddTransient<IGetTemplateFieldsBySpecialtyInputPort, GetTemplateFieldsBySpecialtyInteractor>();
        services.AddTransient<ISaveDynamicFieldsInputPort, SaveDynamicFieldsInteractor>();
        services.AddTransient<IGetDynamicFieldsByVisitInputPort, GetDynamicFieldsByVisitInteractor>();

        return services;
    }
}