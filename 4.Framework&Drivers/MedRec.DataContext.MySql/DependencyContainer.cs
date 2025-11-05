using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.UnitOfWork;
using MedRec.Entity.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddDataContextServices(this IServiceCollection services)
    {
        services.AddDbContext<DataBaseContextMySql>(ServiceLifetime.Scoped);
        services.AddScoped<IDataContextUnitOfWork, DataContextUnitOfWork>();

        return services;
    }
}

