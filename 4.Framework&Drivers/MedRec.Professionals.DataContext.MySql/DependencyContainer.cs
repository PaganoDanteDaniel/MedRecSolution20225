using MedRec.Professionals.DataContext.MySql.Services;
using MedRec.Professionals.Repositories.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddProfessionalsDataContextMySqlServices(this IServiceCollection services)
    {
        services.AddScoped<IProfessionalDataContext, ProfessionalDataContextMySql>();
        services.AddScoped<ISpecialtyLookupDataContext, SpecialtyLookupDataContextMySql>();
        return services;
    }
}
