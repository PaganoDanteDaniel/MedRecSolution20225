using MedRec.DynamicTemplates.BusinessObjects.Interfaces.Repositories;
using MedRec.DynamicTemplates.Repositories.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace MedRec.DynamicTemplates.Repositories;

public static class DependencyContainer
{
    public static IServiceCollection AddDynamicTemplatesRepositoriesServices(this IServiceCollection services)
    {
        // Queries Repositories
        services.AddScoped<IMedicalSpecialtyQueriesRepositoryUoW, MedicalSpecialtyQueriesRepository>();
        services.AddScoped<ITemplateFieldDefinitionQueriesRepositoryUoW, TemplateFieldDefinitionQueriesRepository>();
        services.AddScoped<IMedicalVisitDynamicFieldQueriesRepositoryUoW, MedicalVisitDynamicFieldQueriesRepository>();

        // Commands Repositories
        services.AddScoped<IMedicalSpecialtyCommandRepositoryUoW, MedicalSpecialtyCommandRepository>();
        services.AddScoped<ITemplateFieldDefinitionCommandRepositoryUoW, TemplateFieldDefinitionCommandRepository>();
        services.AddScoped<IMedicalVisitDynamicFieldCommandRepositoryUoW, MedicalVisitDynamicFieldCommandRepository>();

        return services;
    }
}