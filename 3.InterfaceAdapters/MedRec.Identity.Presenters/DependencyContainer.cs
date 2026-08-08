using MedRec.Identity.BusinessObjects.Interfaces.Ports;
using MedRec.Identity.Presenters.Implementations;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddIdentityPresentersServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthenticateUserOutputPort, AuthenticateUserPresenter>();
        services.AddScoped<ICreateUserOutputPort, CreateUserPresenter>();
        services.AddScoped<IUpdateUserOutputPort, UpdateUserPresenter>();
        services.AddScoped<IToggleUserActiveOutputPort, ToggleUserActivePresenter>();
        services.AddScoped<IResetUserPasswordOutputPort, ResetUserPasswordPresenter>();
        services.AddScoped<IChangePasswordOutputPort, ChangePasswordPresenter>();
        services.AddScoped<IUsersListOutputPort, UsersListPresenter>();
        return services;
    }
}
