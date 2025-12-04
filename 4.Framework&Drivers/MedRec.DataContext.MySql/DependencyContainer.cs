using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Infrastructure;
using MedRec.DataContext.MySql.UnitOfWork;
using MedRec.Entity.Interfaces;
using MedRec.Shared.Exceptions.SQLExceptions;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddDataContextServices(this IServiceCollection services)
    {
        services.AddDbContext<MedRecContext>(ServiceLifetime.Scoped);
        services.AddScoped<IDataContextUnitOfWork, DataContextUnitOfWork>();
        services.AddSingleton<IDbConnectionExceptionClassifier, MySqlExceptionClassifier>();

        return services;
    }
}

