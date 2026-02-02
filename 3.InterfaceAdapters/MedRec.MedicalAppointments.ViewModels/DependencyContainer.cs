using MedRec.MedicalAppointments.ViewModels.Orchestration;
using MedRec.MedicalAppointments.ViewModels.Orchestration.Actions;
using MedRec.MedicalAppointments.ViewModels.Orchestration.Actions.Interfaces;
using MedRec.MedicalAppointments.ViewModels.VM;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddMedicalAppointmentVMServices(this IServiceCollection services)
    {
        // Acciones
        services.AddScoped<ICreateAppointmentAction, CreateAppointmentAction>();
        services.AddScoped<IMoveAppointmentAction, MoveAppointmentAction>();
        services.AddScoped<IReassignAppointmentAction, ReassignAppointmentAction>();
        services.AddScoped<IDeleteAppointmentAction, DeleteAppointmentAction>();
        services.AddScoped<IGetAppointmentsAction, GetAppointmentsAction>();

        // Orquestador
        services.AddScoped<IAppointmentOrchestrator, AppointmentOrchestrator>();

        // ViewModels basados en Orquestador
        services.AddTransient<WeeklyScheduleViewModelOrchestrator>();

        return services;
    }
}

