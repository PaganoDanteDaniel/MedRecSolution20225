using MedRec.MedicalVisit.ViewModels.Orchestration;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;
using MedRec.MedicalVisit.ViewModels.VM;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddMedicalVisitVMServices(this IServiceCollection services)
    {
        services.AddScoped<IGetPatientAction, GetPatientAction>()
                .AddScoped<IGetMedicalHistoryAction, GetMedicalHistoryAction>()
                .AddScoped<ICreateMedicalVisitAction, CreateMedicalVisitAction>();


        services.AddScoped<ICreateVisitOrchestrator, CreateVisitOrchestrator>();

        services.AddTransient<CreateVisitVMOrchestrator>();

        return services;
    }
}

