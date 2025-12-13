using MedRec.DataContext.MySql.DataContext;
using MedRec.DataContext.MySql.Options;
using MedRec.Shared.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.IO;

namespace MedRec.WPF.UI;

public static class Startup
{
    public static IServiceProvider? Services { get; private set; }

    public static string GetAppSettingsPath()
    {
        string appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MedRec");
        Directory.CreateDirectory(appDataPath);
        string appSettingsPath = Path.Combine(appDataPath, "appsettings.json");
        string sourcePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(appSettingsPath))
            File.Copy(sourcePath, appSettingsPath, overwrite: false);
        return appSettingsPath;
    }

    // Devuelve true si tras el proceso existe cadena válida.
    public static bool EnsureConnectionSettings(string appSettingsPath)
    {
        bool TieneConexion(string path)
        {
            try
            {
                dynamic? cfg = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText(path));
                return cfg != null &&
                       cfg.DBOptionsMySql != null &&
                       cfg.DBOptionsMySql.ConnectionString != null &&
                       !string.IsNullOrWhiteSpace((string)cfg.DBOptionsMySql.ConnectionString);
            }
            catch
            {
                return false;
            }
        }

        if (TieneConexion(appSettingsPath))
            return true;

        // Mostrar formulario para capturar datos
        var window = new ConnectionSettingsWindow(appSettingsPath);
        window.ShowDialog();

        // Revalidar luego de cerrar
        return TieneConexion(appSettingsPath);
    }

    public static void BuildHost(string appSettingsPath)
    {
        var culture = new CultureInfo("es-ES");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        var host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.Sources.Clear();
                config.AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {
                WireupServices(services, context.Configuration, appSettingsPath);
            })
            .Build();

        // PRECALENTAMIENTO DE ENTITY FRAMEWORK
        try
        {
            using var scope = host.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MedRecContext>();
            _ = dbContext.Model;
        }
        catch (Exception ex)
        {
            // Opcional: loguear el error (por si la conexión falla al inicio)
            // Podés usar System.Diagnostics.Debug.WriteLine o un logger si lo tenés
            System.Diagnostics.Debug.WriteLine($"Error al precargar EF Core: {ex}");
            // Nota: si la conexión no está lista aún (ej. usuario no ingresó credenciales),
            // este paso fallará, pero no es crítico: la primera carga en UI será lenta, pero funcional.
        }

        Services = host.Services;
    }

    private static void WireupServices(IServiceCollection services, IConfiguration configuration, string appSettingsPath)
    {
        var dbOptionsMySql = new DBOptionsMySql();
        var jwtKey = new Jwt();
        configuration.GetSection(DBOptionsMySql.SectionKey).Bind(dbOptionsMySql);
        configuration.GetSection(Jwt.SectionKey).Bind(jwtKey);

        if (!string.IsNullOrEmpty(dbOptionsMySql.ConnectionString))
        {
            if (!EncryptionHelper.IsEncrypted(dbOptionsMySql.ConnectionString))
            {
                dbOptionsMySql.ConnectionString = EncryptionHelper.Encrypt(dbOptionsMySql.ConnectionString);
                var json = File.ReadAllText(appSettingsPath);
                dynamic configFile = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                if (configFile.DBOptionsMySql == null)
                    configFile.DBOptionsMySql = new Newtonsoft.Json.Linq.JObject();
                configFile.DBOptionsMySql.ConnectionString = dbOptionsMySql.ConnectionString;
                File.WriteAllText(appSettingsPath,
                    Newtonsoft.Json.JsonConvert.SerializeObject(configFile, Newtonsoft.Json.Formatting.Indented));
            }
            dbOptionsMySql.ConnectionString = EncryptionHelper.Decrypt(dbOptionsMySql.ConnectionString);
        }

        if (!string.IsNullOrEmpty(jwtKey.Key) && !EncryptionHelper.IsEncrypted(jwtKey.Key))
        {
            jwtKey.Key = EncryptionHelper.Encrypt(jwtKey.Key);
            var json = File.ReadAllText(appSettingsPath);
            dynamic configFile = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            if (configFile.Jwt == null)
                configFile.Jwt = new Newtonsoft.Json.Linq.JObject();
            configFile.Jwt.Key = jwtKey.Key;
            File.WriteAllText(appSettingsPath,
                Newtonsoft.Json.JsonConvert.SerializeObject(configFile, Newtonsoft.Json.Formatting.Indented));
        }

        services.AddSingleton(Options.Create(dbOptionsMySql));
        services.Configure<DBOptionsMySql>(configuration.GetSection(DBOptionsMySql.SectionKey));
        services.AddWpfBlazorWebView();
        services.AddAppServices();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
    }
}
