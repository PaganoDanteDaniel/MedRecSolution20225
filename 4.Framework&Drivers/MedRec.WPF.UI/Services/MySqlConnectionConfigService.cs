using MedRec.Shared.Security;
using Newtonsoft.Json;
using System.IO;

namespace MedRec.WPF.UI.Services;

public class MySqlConnectionConfigService
{
    private readonly string _appSettingsPath;

    public MySqlConnectionConfigService(string appSettingsPath)
    {
        _appSettingsPath = appSettingsPath;
    }

    public event Action? ConnectionSaved; // Evento de notificación

    public string BuildPlainConnectionString(string server, int port, string database, string user, string password,
        bool allowPublicKeyRetrieval, string sslMode)
    {
        return $"Server={server};Port={port};Database={database};User={user};Password={password};" +
               $"AllowPublicKeyRetrieval={(allowPublicKeyRetrieval ? "True" : "False")};SslMode={sslMode}";
    }

    public void PersistEncrypted(string plainConnectionString)
    {
        var encrypted = EncryptionHelper.Encrypt(plainConnectionString);

        dynamic? json = JsonConvert.DeserializeObject(File.ReadAllText(_appSettingsPath))
                        ?? new Newtonsoft.Json.Linq.JObject();

        if (json.DBOptionsMySql == null)
            json.DBOptionsMySql = new Newtonsoft.Json.Linq.JObject();

        json.DBOptionsMySql.ConnectionString = encrypted;

        File.WriteAllText(_appSettingsPath,
            JsonConvert.SerializeObject(json, Formatting.Indented));

        ConnectionSaved?.Invoke(); // dispara evento
    }
}
