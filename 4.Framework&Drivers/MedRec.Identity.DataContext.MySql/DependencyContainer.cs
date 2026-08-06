using MedRec.Entity.Interfaces;
using MedRec.Identity.BusinessObjects.Interfaces.Services;
using MedRec.Identity.DataContext.MySql.Services;
using MedRec.Identity.Repositories.Interfaces;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyContainer
{
    public static IServiceCollection AddIdentityDataContextMySqlServices(this IServiceCollection services)
    {
        services.AddScoped<IUserQueriesDataContext, UserQueriesDataContextMySql>();
        services.AddSingleton<IPasswordHasher, PasswordHasherService>();
        services.AddSingleton<IAuthTokenGenerator, JwtAuthTokenGenerator>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<ICurrentUserContext, CurrentUserContext>();

        return services;
    }
}
