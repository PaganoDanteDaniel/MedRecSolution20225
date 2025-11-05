using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.UseCases.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddMedicalAppointmentUseCasesServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateMedicalAppointmentInputPort, CreateMedicalAppointmentInteractor>()
                .AddScoped<IMoveMedicalAppointmentInputPort, MoveMedicalAppointmentInteractor>()
                .AddScoped<IReassignMedicalAppointmentInputPort, ReassignMedicalAppointmentInteractor>()
                .AddScoped<IDeleteMedicalAppointmentInputPort, DeleteMedicalAppointmentInteractor>()
                .AddScoped<IGetMedicalAppointmentsInputPort, GetMedicalAppointmentsInteractor>();

        return services;
    }
}

