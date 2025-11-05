using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.Presenters.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddPatientPresentersServices(this IServiceCollection services)
    {
        services.AddScoped<ICreatePatientOutputPort, CreatePatientPresenter>()
                .AddScoped<IUpdatePatientOutputPort, UpdatePatientPresenter>()
                .AddScoped<IPatientDetailsOutputPort, PatientDetailPresenter>()
                .AddScoped<IDeletePatientOutputPort, DeletePatientPresenter>()
                .AddScoped<IPatientsListOutputPort, PatientsListPresenter>()
                .AddScoped<IPatientForMedicalVisitOutputPort, PatientForMedicalVisitPresenter>();

        return services;
    }
}

