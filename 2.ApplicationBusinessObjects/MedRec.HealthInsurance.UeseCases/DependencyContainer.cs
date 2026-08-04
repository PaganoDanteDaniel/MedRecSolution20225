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
}

