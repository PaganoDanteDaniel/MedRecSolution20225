using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.UseCases.Implementations;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection container for DynamicTemplates UseCases
/// </summary>
public static class DependencyContainer
{
    /// <summary>
    /// Registers all use case interactors
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDynamicTemplatesUseCases(this IServiceCollection services)
    {
        // Register interactors as transient (new instance per request)
        services.AddTransient<IGetActiveSpecialtiesInputPort, GetActiveSpecialtiesInteractor>();
        services.AddTransient<IGetTemplateFieldsBySpecialtyInputPort, GetTemplateFieldsBySpecialtyInteractor>();
        services.AddTransient<ISaveDynamicFieldsInputPort, SaveDynamicFieldsInteractor>();
        services.AddTransient<IGetDynamicFieldsByVisitInputPort, GetDynamicFieldsByVisitInteractor>();

        return services;
    }
}