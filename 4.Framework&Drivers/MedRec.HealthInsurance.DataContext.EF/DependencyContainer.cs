using MedRec.HealthInsurance.DataContext.EF.DataContext;
using MedRec.HealthInsurance.DataContext.EF.Services;
using MrdRec.HealthInsurance.Repositories.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddHealthInsuranceDataContextServices(this IServiceCollection services)
    {
        services.AddDbContext<HealthInsuranceContext>();

        services.AddScoped<IHealthInsuranceQueriesDataContext, HealthInsuranceQueriesDataContext>();
        services.AddScoped<IHealthInsuranceCommandsDataContext, HealthInsuranceCommandsDataContext>();

        return services;
    }
    /*Eliminar esta clase 
     * continuar con la reacion de los DtaContext
     * Armar los ViewModels y los Models
     * Crear los componentes
     */
}
