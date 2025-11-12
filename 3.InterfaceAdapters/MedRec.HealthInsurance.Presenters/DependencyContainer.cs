using MedRec.HealthInsurance.BusinessObjects.Interfaces.Ports;
using MedRec.HealthInsurance.Presenters.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddHealthInsurancePresentersServices(this IServiceCollection services)
    {
        services.AddScoped<IHealthInsuranceCatalogOutputPort, HealthInsuranceCatalogPresenter>();
        services.AddScoped<ICreateHealthInsuranceOutputPort, CreateHealthInsurancePresenter>();
        services.AddScoped<IDeleteHealthInsuranceOutputPort, DeleteHealthInsurancePresenter>();
        services.AddScoped<IUpdateHealthInsuranceOutputPort, UpdateHealthInsurancePresenter>();
        services.AddScoped<IGetHealthInsuranceByIdOutputPort, GetHealthInsuranceByIdPresenter>();

        return services;
    }
}

