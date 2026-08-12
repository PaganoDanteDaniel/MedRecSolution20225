using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.Repositories.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddIdentityRepositoriesServices(this IServiceCollection services)
    {
        services.AddScoped<IUserQueriesRepository, UserQueriesRepository>();
        services.AddScoped<IUserCommandsRepository, UserCommandsRepository>();
        services.AddScoped<IProfessionalLookupRepository, ProfessionalLookupRepository>();
        services.AddScoped<IRoleLookupRepository, RoleLookupRepository>();
        return services;
    }
}
