using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.UseCases.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddMedicalVisitUseCasesServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateMedicalVisitInputPort, CreateMedicalVisitInteractor>()
                .AddScoped<IMedicalVisitSummaryListInputPort, MedicalVisitSummaryListInteractor>()
                .AddScoped<IGetMedicalHistoryIdInputPort, GetMedicalHistoryIdInteractor>()
                .AddScoped<IGetMedicalVisitInputPort, GetMedicalVisitInteractor>()
                .AddScoped<IUpdateMedicalVisitInputPort, UpdateMedicalVisitInteractor>();


        return services;
    }
}

