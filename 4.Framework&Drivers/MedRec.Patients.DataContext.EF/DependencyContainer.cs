using MedRec.Patients.DataContext.EF.DataContext;
using MedRec.Patients.DataContext.EF.Services;
using MedRec.Patients.Repositories.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddPatientDataContextServices(this IServiceCollection services)
    {

        services.AddDbContext<PatientDataContext>();

        services.AddScoped<IPatientCommandsDataContext, PatientCommandDataContext>();
        services.AddScoped<IPatientQueriesDataContext, PatientQueriesDataContext>();

        return services;
    }
}

