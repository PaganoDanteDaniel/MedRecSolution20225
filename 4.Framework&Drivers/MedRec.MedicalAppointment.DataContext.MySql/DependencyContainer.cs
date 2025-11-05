using MedRec.MedicalAppointments.DataContext.MySql.Services;
using MedRec.MedicalAppointments.Repositories.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddMedicalAppointmentDataContextServices(this IServiceCollection services)
    {
        services.AddScoped<IMedicalAppointmentCommandsDataContext, MedicalAppointmentCommandsDataContext>()
                .AddScoped<IMedicalAppointmentQueriesDataContext, MedicalAppointmentQueriesDataContext>();

        return services;
    }
}

