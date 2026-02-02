using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.UseCases.Implementation;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    /// <summary>
    /// Registro para Blazor Hybrid (hoy): implementaciones + proxies que capturan excepciones y generan ErrorInfo vía mapper/_presenter.
    /// </summary>
    public static IServiceCollection AddHealthInsuranceUseCasesServicesWithProxy(
        this IServiceCollection services,
        bool rethrow = false)
    {
        // Delegar completamente al método genérico
        return services.AddUseCaseExceptionDecorators(
            [
            typeof(ICreateHealthInsuranceInputPort).Assembly,
            typeof(CreateHealthInsuranceInteractor).Assembly
            ], rethrow);
    }

    // Registro directo (resérvalo para futura API)
    public static IServiceCollection AddHealthInsuranceUseCasesServices(this IServiceCollection services)
    {

        services.AddScoped<ICreateHealthInsuranceInputPort, CreateHealthInsuranceInteractor>();
        services.AddScoped<IHealthInsuranceCatalogInputPort, HealthInsuranceCatalogInteractor>();
        services.AddScoped<IDeleteHealthInsuranceInputPort, DeleteHealthInsuranceInteractor>();
        services.AddScoped<IUpdateHealthInsuranceInputPort, UpdateHealthInsuranceInteractor>();
        services.AddScoped<IGetHealthInsuranceByIdInputPort, GetHealthInsuranceByIdInteractor>();

        return services;
    }
}

