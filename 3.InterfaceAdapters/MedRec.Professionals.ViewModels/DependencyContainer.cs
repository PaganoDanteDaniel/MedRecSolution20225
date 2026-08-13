using MedRec.Professionals.ViewModels.Orchestration;
using MedRec.Professionals.ViewModels.Orchestration.Actions;
using MedRec.Professionals.ViewModels.Orchestration.Actions.Interfaces;
using MedRec.Professionals.ViewModels.Orchestration.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddProfessionalsVMServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateProfessionalAction, CreateProfessionalAction>()
                .AddScoped<IDeleteProfessionalAction, DeleteProfessionalAction>()
                .AddScoped<ICreateUserForProfessionalAction, CreateUserForProfessionalAction>();

        services.AddScoped<ICreateProfessionalOrchestrator, CreateProfessionalOrchestrator>();

        return services;
    }
}
