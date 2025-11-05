using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.Presenters.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddMedicalAppointmentPresentersServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateMedicalAppointmentOutputPort, CreateMedicalAppointmentPresenter>()
                .AddScoped<IMoveMedicalAppointmentOutputPort, MoveMedicalAppointmentPresenter>()
                .AddScoped<IReassignMedicalAppointmentOutputPort, ReassignMedicalAppointmentPresenter>()
                .AddScoped<IDeleteMedicalAppointmentOutputPort, DeleteMedicalAppointmentPresenter>()
                .AddScoped<IGetMedicalAppointmentsOutputPort, GetMedicalAppointmentsPresenter>();

        return services;
    }
}

