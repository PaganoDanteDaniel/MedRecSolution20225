using MedRec.HealthInsurance.BusinessObjects.Interfaces.Repositories;
using MrdRec.HealthInsurance.Repositories.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddHealthInsuranceRepositoriesServices(this IServiceCollection services)
    {
        services.AddScoped<IHealtInsuranceQueriesRepository, HealthInsuranceQueriesRepository>();
        services.AddScoped<IHealthInsuranceCommandRepository, HealthInsuranceCommandRepository>();

        return services;
    }
}

