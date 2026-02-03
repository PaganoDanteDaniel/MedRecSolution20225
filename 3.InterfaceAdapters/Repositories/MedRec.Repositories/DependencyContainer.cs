using MedRec.Entity.Interfaces;
using MedRec.Repositories.Services;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddRepositoriesServices(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryUnitOfWork, RepositoryUnitOfWork>();
        return services;
    }
}

