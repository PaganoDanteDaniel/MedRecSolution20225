using MedRec.MedicalVisit.DataContext.MySql.Services;
using MedRec.MedicalVisit.Repositories.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddMedicalVisitDataContextServices(this IServiceCollection services)
    {
        services.AddScoped<IMedicalVisitCommandDataContext, MedicalVisitCommandDataContextMySql>();
        services.AddScoped<IMedicalVisitQueriesDataContext, MedicalVisitQueriesDataContextMySql>();

        return services;
    }
}

