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
        services.AddScoped<IUserCommandsDataContext, UserCommandsDataContextMySql>();
        services.AddScoped<IDoctorLookupDataContext, DoctorLookupDataContextMySql>();
        services.AddScoped<IRoleLookupDataContext, RoleLookupDataContextMySql>();
        services.AddSingleton<IPasswordHasher, PasswordHasherService>();
        services.AddSingleton<IAuthTokenGenerator, JwtAuthTokenGenerator>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<ICurrentUserContext, CurrentUserContext>();
        services.AddSingleton<IEmailNotificationService, SmtpEmailNotificationService>();

        return services;
    }
}
