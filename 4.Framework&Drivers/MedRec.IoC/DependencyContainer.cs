using MedRec.DynamicTemplates.DataContext.MySql;
using MedRec.DynamicTemplates.Repositories;
using MedRec.DynamicTemplates.ViewModels.Orchestration;
using MedRec.DynamicTemplates.ViewModels.VM;
using MedRec.HealthInsurance.ViewModels.VM;
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
        services.AddPatientUseCasesServicesWithProxy();
        services.AddPatientPresentersServices();

        services.AddTransient<CreatePatientVM>();
        services.AddTransient<PatientsListVM>();
        services.AddTransient<UpdatePatientVM>();
        services.AddTransient<DeletePatientVM>();

        services.AddHealthInsuranceDataContextServices();
        services.AddHealthInsuranceRepositoriesServices();
        services.AddHealthInsuranceUseCasesServicesWithProxy();
        services.AddHealthInsurancePresentersServices();

        services.AddTransient<HealthInsuranceCatalogVM>();
        services.AddTransient<CreateHealthInsuranceVM>();
        services.AddTransient<UpdateHealthInsuranceVM>();

        services.AddMedicalVisitDataContextServices()
                .AddMedicalVisitRepositoriesServices()
                .AddMedicalVisitUseCasesServicesWithProxy()
                .AddMedicalVisitPresenterServices()
                .AddMedicalVisitVMServices();

        services.AddMedicalAppointmentDataContextServices()
                .AddMedicalAppointmentRepositoriesServices()
                .AddMedicalAppointmentUseCasesServicesWithProxy()
                .AddMedicalAppointmentPresentersServices()
                .AddMedicalAppointmentVMServices();

        services.AddDynamicTemplatesDataContextServices()
                .AddDynamicTemplatesRepositoriesServices()
                .AddDynamicTemplatesPresenters()
                .AddDynamicTemplatesUseCases()
                .AddDynamicTemplatesVMServices();


        services.AddValidatorServices();

        return services;
    }
}


