using MedRec.MedicalAppointments.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalAppointments.UseCases.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    /// <summary>
    /// Registro para Blazor Hybrid (hoy): implementaciones + proxies que capturan excepciones y generan ErrorInfo vía mapper/_presenter.
    /// </summary>
    public static IServiceCollection AddMedicalAppointmentUseCasesServicesWithProxy(
        this IServiceCollection services,
        bool rethrow = false)
    {
        // Delegar completamente al método genérico
        return services.AddUseCaseExceptionDecorators(
            [
            typeof(ICreateMedicalAppointmentInputPort).Assembly,
            typeof(CreateMedicalAppointmentInteractor).Assembly
            ], rethrow);
    }
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

