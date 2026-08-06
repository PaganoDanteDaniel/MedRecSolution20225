using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.BusinessObjects.Interfaces.Repositories;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.UseCases.Implementations;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddIdentityUseCasesServicesWithProxy(
        this IServiceCollection services,
        bool rethrow = false)
    {
        services.AddScoped<IAuthorizationService, AuthorizationService>();

        return services.AddUseCaseExceptionDecorators(
            [
                typeof(IAuthenticateUserInputPort).Assembly,
                typeof(AuthenticateUserInteractor).Assembly
            ], rethrow);
    }
}
