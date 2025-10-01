using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.UeseCases.Implementation;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddHealthInsuranceUseCasesServices(this IServiceCollection services)
    {
        services.AddScoped<IHealthInsuranceCatalogInputPort, HealthInsuranceCatalogInteractor>();
        services.AddScoped<ITotalHealthInsuranceCountInputPort, TotalHealthInsuranceCountInteractor>();

        return services;
    }
}

