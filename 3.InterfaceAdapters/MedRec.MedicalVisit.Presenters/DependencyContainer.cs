using MedRec.MedicalVisit.BusinessObjects.Interfaces.Ports;
using MedRec.MedicalVisit.Presenters.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddMedicalVisitPresenterServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateMedicalVisitOutputPort, CreateMedicalVisitPresenter>()
                .AddScoped<IMedicalVisitSummaryListOutputPort, MedicalVisitSummaryListPresenter>()
                .AddScoped<IGetMedicalHistoryIdOutputPort, GetMedicalHistoryIdPresenter>()
                .AddScoped<IGetMedicalVisitOutputPort, GetMedicalVisitPresenter>()
                .AddScoped<IUpdateMedicalVisitOutputPort, UpdateMedicalVisitPresenter>();

        return services;
    }
}

