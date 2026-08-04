using MedRec.DynamicTemplates.ViewModels.Orchestration;
using MedRec.DynamicTemplates.ViewModels.Orchestration.Interfaces;
using MedRec.DynamicTemplates.ViewModels.VM;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddDynamicTemplatesVMServices(this IServiceCollection services)
    {
        // Orchestrators de DynamicTemplates
        services.AddTransient<IListSpecialtiesOrchestrator, ListSpecialtiesOrchestrator>();
        services.AddTransient<IGetTemplateFieldsOrchestrator, GetTemplateFieldsOrchestrator>();
        services.AddTransient<IGetDynamicFieldsOrchestrator, GetDynamicFieldsOrchestrator>();
        services.AddTransient<ISaveDynamicFieldsOrchestrator, SaveDynamicFieldsOrchestrator>();

        // ViewModels de DynamicTemplates (si no lo hiciste ya)
        services.AddTransient<ListSpecialtiesVM>();
        services.AddTransient<GetTemplateFieldsVM>();
        services.AddTransient<GetDynamicFieldsVM>();
        services.AddTransient<SaveDynamicFieldsVM>();

        return services;
    }
}

