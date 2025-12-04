using MedRec.MedicalVisit.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalVisit.Repositories.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddMedicalVisitRepositoriesServices(this IServiceCollection services)
    {
        services.AddScoped<IMedicalVisitCommandRepositoryUoW, MedicalVisitCommandRepositoryUoW>();
        services.AddScoped<IMedicalVisitQueriesRepositoryUoW, MedicalVisitQueriesRepositoryUoW>();

        return services;
    }
}

