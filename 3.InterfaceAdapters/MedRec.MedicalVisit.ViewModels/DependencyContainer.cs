using MedRec.MedicalVisit.ViewModels.Orchestration;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions;
using MedRec.MedicalVisit.ViewModels.Orchestration.Actions.Interfaces;
using MedRec.MedicalVisit.ViewModels.Orchestration.Interfaces;
using MedRec.MedicalVisit.ViewModels.VM;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddMedicalVisitVMServices(this IServiceCollection services)
    {
        services.AddScoped<IGetPatientAction, GetPatientAction>()
                .AddScoped<IGetMedicalVisitAction, GetMedicalVisitAction>()
                .AddScoped<IGetMedicalHistoryAction, GetMedicalHistoryAction>()
                .AddScoped<IGetTemplateFieldsAction, GetTemplateFieldsAction>()
                .AddScoped<ICreateMedicalVisitAction, CreateMedicalVisitAction>()
                .AddScoped<IUpdateMedicalVisitAction, UpdateMedicalVisitAction>();


        services.AddScoped<ICreateMedicalVisitOrchestrator, CreateMedicalVisitOrchestrator>()
                .AddScoped<IUpdateMedicalVisitOrchestrator, UpdateMedicalVisitOrchestrator>();

        services.AddTransient<CreateMedicalVisitVM>()
                .AddTransient<UpdateMedicalVisitVM>();

        services.AddTransient<MedicalVisitVM>();
        return services;
    }
}

