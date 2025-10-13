using MedRec.Patients.DataContext.MySql.DataContext;
using MedRec.Patients.DataContext.MySql.Services;
using MedRec.Patients.Repositories.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddPatientDataContextMySqlServices(this IServiceCollection services)
    {

        services.AddDbContext<PatientDataContext>();

        services.AddScoped<IPatientCommandsDataContext, PatientCommandDataContextMySql>();
        services.AddScoped<IPatientQueriesDataContext, PatientQueriesDataContextMySql>();

        return services;
    }
}

