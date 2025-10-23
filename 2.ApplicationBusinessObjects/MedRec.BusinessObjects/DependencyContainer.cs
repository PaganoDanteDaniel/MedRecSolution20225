using MedRec.BusinessObjects.Implementations;
using MedRec.Entity.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddCommonBusinessObjectsServices(this IServiceCollection services)
    {
        services.AddScoped<IRepositoryUnitOfWork, RepositoryUnitOfWork>();

        return services;
    }
}

