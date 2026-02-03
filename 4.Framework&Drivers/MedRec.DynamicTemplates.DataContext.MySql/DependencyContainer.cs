using MedRec.DynamicTemplates.DataContext.MySql.Services;
using MedRec.DynamicTemplates.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace MedRec.DynamicTemplates.DataContext.MySql;

public static class DependencyContainer
{
    public static IServiceCollection AddDynamicTemplatesDataContextServices(this IServiceCollection services)
    {
        // Queries DataContext
        services.AddScoped<IMedicalSpecialtyQueriesDataContext, MedicalSpecialtyQueriesDataContextMySql>();
        services.AddScoped<ITemplateFieldDefinitionQueriesDataContext, TemplateFieldDefinitionQueriesDataContextMySql>();
        services.AddScoped<IMedicalVisitDynamicFieldQueriesDataContext, MedicalVisitDynamicFieldQueriesDataContextMySql>();

        // Commands DataContext
        services.AddScoped<IMedicalSpecialtyCommandsDataContext, MedicalSpecialtyCommandsDataContextMySql>();
        services.AddScoped<ITemplateFieldDefinitionCommandsDataContext, TemplateFieldDefinitionCommandsDataContextMySql>();
        services.AddScoped<IMedicalVisitDynamicFieldCommandsDataContext, MedicalVisitDynamicFieldCommandsDataContextMySql>();

        return services;
    }
}