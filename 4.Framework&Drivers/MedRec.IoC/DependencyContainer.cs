using MedRec.HealthInsurance.ViewModels.VM;
using MedRec.MedicalAppointments.ViewModels.VM;
using MedRec.MedicalVisit.ViewModels.VM;
using MedRec.Patients.ViewModels.VM;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddDataContextServices();
        services.AddRepositoriesServices();

        services.AddPatientDataContextMySqlServices();
        services.AddPatientRepositoriesServices();
        services.AddPatientUseCasesServices();
        services.AddPatientPresentersServices();

        services.AddTransient<CreatePatientVM>();
        services.AddTransient<PatientsListVM>();
        services.AddTransient<UpdatePatientVM>();

        services.AddHealthInsuranceDataContextServices();
        services.AddHealthInsuranceRepositoriesServices();
        services.AddHealthInsuranceUseCasesServices();
        services.AddHealthInsurancePresentersServices();

        services.AddTransient<HealthInsuranceCatalogVM>();
        services.AddTransient<CreateHealthInsuranceVM>();
        services.AddTransient<UpdateHealthInsuranceVM>();

        services.AddMedicalVisitDataContextServices()
                .AddMedicalVisitRepositoriesServices()
                .AddMedicalVisitUseCasesServices()
                .AddMedicalVisitPresenterServices();

        services.AddTransient<CreateMedicalVisitVM>();
        services.AddTransient<UpdateMedicalVisitVM>();
        services.AddTransient<MedicalVisitVM>();

        services.AddMedicalAppointmentDataContextServices()
                .AddMedicalAppointmentRepositoriesServices()
                .AddMedicalAppointmentUseCasesServices()
                .AddMedicalAppointmentPresentersServices();

        services.AddTransient<WeeklyScheduleViewModel>();

        services.AddValidatorServices();

        return services;
    }
}


