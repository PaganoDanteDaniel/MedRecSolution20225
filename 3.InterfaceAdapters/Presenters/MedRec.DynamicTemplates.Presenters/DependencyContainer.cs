using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Ports;
using MedRec.DynamicTemplates.Presenters.Implementation;


namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection container for DynamicTemplates Presenters
/// </summary>
public static class DependencyContainer
{
    /// <summary>
    /// Registers all presenter services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddDynamicTemplatesPresenters(this IServiceCollection services)
    {
        // Register presenters as transient (new instance per request)
        services.AddTransient<IGetActiveSpecialtiesOutputPort, GetActiveSpecialtiesPresenter>();
        services.AddTransient<IGetTemplateFieldsBySpecialtyOutputPort, GetTemplateFieldsBySpecialtyPresenter>();
        services.AddTransient<ISaveDynamicFieldsOutputPort, SaveDynamicFieldsPresenter>();
        services.AddTransient<IGetDynamicFieldsByVisitOutputPort, GetDynamicFieldsByVisitPresenter>();

        return services;
    }
}