using MedRec.Patients.BusinessObjects.Interfaces.Ports;
using MedRec.Patients.UseCases.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddPatientUseCasesServices(this IServiceCollection services)
    {
        services.AddScoped<ICreatePatientInputPort, CreatePatientInteractor>()
                .AddScoped<ICreatePatientInputPort, CreatePatientInteractor>()
                .AddScoped<IUpdatePatientInputPort, UpdatePatientInteractor>();

        return services;
    }
}

