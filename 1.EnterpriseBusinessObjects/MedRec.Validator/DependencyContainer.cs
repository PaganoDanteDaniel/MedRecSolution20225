using MedRec.Validator.Implementations;
using MedRec.Validator.Interfaces;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;
public static class DependencyContainer
{
    public static IServiceCollection AddValidatorServices(this IServiceCollection services)
    {
        services.TryAddSingleton(typeof(IModelValidatorHub<>), typeof(ModelValidatorHub<>));

        return services;
    }
}

