using MedRec.HealthInsurance.ViewModels.VM;
using MedRec.MedicalVisit.ViewModels.VM;
using MedRec.Patients.ViewModels.VM;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddDataContextServices();

        services.AddPatientDataContextMySqlServices();  // Primero el DbContext
        services.AddPatientRepositoriesServices();      // Luego repositorios específicos
        services.AddPatientUseCasesServices();          // Luego casos de uso
        services.AddPatientPresentersServices();        // Luego presentadores

        services.AddTransient<CreatePatientVM>();       // Al final, ViewModels
        services.AddTransient<PatientsListVM>();
        services.AddTransient<UpdatePatientVM>();

        services.AddHealthInsuranceDataContextServices();
        services.AddHealthInsuranceRepositoriesServices();
        services.AddHealthInsuranceUseCasesServices();
        services.AddHealthInsurancePresentersServices();

        services.AddTransient<HealthInsuranceCatalogVM>();

        services.AddMedicalVisitDataContextServices();
        services.AddMedicalVisitRepositoriesServices();
        services.AddMedicalVisitUseCasesServices();
        services.AddMedicalVisitPresenterServices();

        services.AddTransient<CreateMedicalVisitVM>();
        services.AddTransient<MedicalVisitVM>();

        services.AddValidatorServices();
        return services;
    }
}


