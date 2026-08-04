using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.UseCases.Implementations;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    /// <summary>
    /// Registro para Blazor Hybrid (hoy): implementaciones + proxies que capturan excepciones y generan ErrorInfo vía mapper/presenter.
    /// </summary>
    public static IServiceCollection AddPatientUseCasesServicesWithProxy(
        this IServiceCollection services,
        bool rethrow = false)
    {
        // Delegar completamente al método genérico
        return services.AddUseCaseExceptionDecorators(
            [
            typeof(ICreatePatientInputPort).Assembly,
            typeof(CreatePatientInteractor).Assembly
            ], rethrow);
    }
}

