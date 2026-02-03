using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Repositories;
using MedRec.MedicalAppointments.Repositories.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddMedicalAppointmentRepositoriesServices(this IServiceCollection services)
    {
        services.AddScoped<IMedicalAppointmentCommandRepository, MedicalAppointmentCommandRepository>()
                .AddScoped<IMedicalAppointmentQueriesRepository, MedicalAppointmentQueriesRepository>();

        return services;
    }
}

