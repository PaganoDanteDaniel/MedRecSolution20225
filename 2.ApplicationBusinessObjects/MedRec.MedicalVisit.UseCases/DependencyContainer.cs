using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.UseCases.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    /// <summary>
    /// Registro para Blazor Hybrid (hoy): implementaciones + proxies que capturan excepciones y generan ErrorInfo vía mapper/presenter.
    /// </summary>
    public static IServiceCollection AddMedicalVisitUseCasesServicesWithProxy(
        this IServiceCollection services,
        bool rethrow = false)
    {
        // Delegar completamente al método genérico
        return services.AddUseCaseExceptionDecorators(
            [
            typeof(ICreateMedicalVisitInputPort).Assembly,
            typeof(CreateMedicalVisitInteractor).Assembly
            ], rethrow);
    }
}

