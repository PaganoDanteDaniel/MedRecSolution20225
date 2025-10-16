using MedRec.DataContext.MySql.DataContext;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddDataContextServices(this IServiceCollection services)
    {
        services.AddDbContext<DataBaseContextMySql>();

        return services;
    }
}

