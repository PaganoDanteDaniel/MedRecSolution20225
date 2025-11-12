using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.UseCases.Implementation;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddHealthInsuranceUseCasesServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateHealthInsuranceInputPort, CreateHealthInsuranceInteractor>();
        services.AddScoped<IHealthInsuranceCatalogInputPort, HealthInsuranceCatalogInteractor>();
        services.AddScoped<IDeleteHealthInsuranceInputPort, DeleteHealthInsuranceInteractor>();
        services.AddScoped<ITotalHealthInsuranceCountInputPort, TotalHealthInsuranceCountInteractor>();
        services.AddScoped<IUpdateHealthInsuranceInputPort, UpdateHealthInsuranceInteractor>();
        services.AddScoped<IGetHealthInsuranceByIdInputPort, GetHealthInsuranceByIdInteractor>();

        return services;
    }
}

