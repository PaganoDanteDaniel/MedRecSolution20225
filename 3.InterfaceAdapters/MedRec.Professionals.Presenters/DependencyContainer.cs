using MedRec.Professionals.BusinessObjects.Interfaces.Ports;
using MedRec.Professionals.Presenters.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddProfessionalsPresentersServices(this IServiceCollection services)
    {
        services.AddScoped<ICreateProfessionalOutputPort, CreateProfessionalPresenter>();
        services.AddScoped<IUpdateProfessionalOutputPort, UpdateProfessionalPresenter>();
        services.AddScoped<IDeleteProfessionalOutputPort, DeleteProfessionalPresenter>();
        services.AddScoped<IListProfessionalsOutputPort, ListProfessionalsPresenter>();
        services.AddScoped<IGetProfessionalByIdOutputPort, GetProfessionalByIdPresenter>();
        return services;
    }
}
