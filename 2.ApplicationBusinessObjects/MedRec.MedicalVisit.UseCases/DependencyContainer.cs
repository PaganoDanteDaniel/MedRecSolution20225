using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.UseCases.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddMedicalVisitUseCasesServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateMedicalVisitInputPort, CreateMedicalVisitInteractorUoW>()
                .AddScoped<IMedicalVisitSummaryListInputPort, MedicalVisitSummaryListInteractorUoW>()
                .AddScoped<IGetMedicalHistoryIdInputPort, GetMedicalHistoryIdInteractorUoW>()
                .AddScoped<IGetMedicalVisitInputPort, GetMedicalVisitInteractorUoW>()
                .AddScoped<IUpdateMedicalVisitInputPort, UpdateMedicalVisitInteractorUoW>();


        return services;
    }
}

