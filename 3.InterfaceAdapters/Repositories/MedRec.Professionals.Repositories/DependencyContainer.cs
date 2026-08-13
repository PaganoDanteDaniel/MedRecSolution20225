using MedRec.Professionals.BusinessObjects.Interfaces.Repositories;
using MedRec.Professionals.Repositories.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddProfessionalsRepositoriesServices(this IServiceCollection services)
    {
        services.AddScoped<IProfessionalRepositoryUoW, ProfessionalRepository>();
        services.AddScoped<ISpecialtyLookupRepository, SpecialtyLookupRepository>();
        return services;
    }
}
