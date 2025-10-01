using MedRec.DataContext.EF.Options;
using MedRec.Shared.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.IO;

namespace MedRec.WPF.UI;
public static class Startup
{
    public static IServiceProvider? Services { get; private set; }

    public static void Init()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                WireupServices(services, context.Configuration);
            })
            .Build();

        Services = host.Services;
    }

    private static void WireupServices(IServiceCollection services, IConfiguration configuration)
    {
        // 1. Crear instancia de DBOptions y bindear desde configuración
        var dbOptions = new DBOptions();
        var jwtKey = new Jwt();
        configuration.GetSection(DBOptions.SectionKey).Bind(dbOptions);
        configuration.GetSection(Jwt.SectionKey).Bind(jwtKey);

        if (!string.IsNullOrEmpty(dbOptions.ConnectionString))
        {
            if (!EncryptionHelper.IsEncrypted(dbOptions.ConnectionString))
            {
                // Primera ejecución: encriptar y guardar en appsettings.json
                dbOptions.ConnectionString = EncryptionHelper.Encrypt(dbOptions.ConnectionString);

                // Reescribir el appsettings.json con la cadena encriptada
                var json = File.ReadAllText("appsettings.json");
                dynamic config = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                config.DBOptions.ConnectionString = dbOptions.ConnectionString;
                File.WriteAllText("appsettings.json",
                    Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
            }

            // Ya estaba encriptada, desencriptar para usar
            dbOptions.ConnectionString = EncryptionHelper.Decrypt(dbOptions.ConnectionString);

            if (!EncryptionHelper.IsEncrypted(jwtKey.Key))
            {
                jwtKey.Key = EncryptionHelper.Encrypt(jwtKey.Key);
                var json = File.ReadAllText("appsettings.json");
                dynamic config = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                config.Jwt.Key = jwtKey.Key;
                File.WriteAllText("appsettings.json",
                    Newtonsoft.Json.JsonConvert.SerializeObject(config, Newtonsoft.Json.Formatting.Indented));
            }
        }

        // 2. Registrar la instancia ya procesada como singleton
        services.AddSingleton(Options.Create(dbOptions));
        services.Configure<DBOptions>(configuration.GetSection(DBOptions.SectionKey));
        // 3. Registrar otros servicios
        services.AddWpfBlazorWebView();
        services.AddAppServices();

#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
    }
}
