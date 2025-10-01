using MedRec.Patients.BusinessObjects.Interfaces.Repositories;
using MedRec.Patients.Repositories.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddPatientRepositoriesServices(this IServiceCollection services)
    {
        services.AddScoped<IPatientCommandsRepository, PatientCommandsRepository>();

        return services;
    }
}

