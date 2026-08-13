using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.UseCases.Implementations;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddProfessionalsUseCasesServicesWithProxy(
        this IServiceCollection services,
        bool rethrow = false)
    {
        return services.AddUseCaseExceptionDecorators(
            [
                typeof(ICreateProfessionalInputPort).Assembly,
                typeof(CreateProfessionalInteractor).Assembly
            ], rethrow);
    }
}
