namespace MedRec.WPF.UI.Components.Connection;
public partial class MySqlConnectionSetup
{
    private bool _saved;
    private MySqlConnModel _model = new()
    {
        Port = 3306,
        Database = "medrecdb",
        AllowPublicKeyRetrieval = true,
        SslMode = "none"
    };

    private void OnValidSubmit()
    {
        var plain = ConfigService.BuildPlainConnectionString(
            _model.Server ?? "",
            _model.Port,
            _model.Database ?? "",
            _model.User ?? "",
            _model.Password ?? "",
            _model.AllowPublicKeyRetrieval,
            _model.SslMode ?? "none");

        ConfigService.PersistEncrypted(plain);
        _saved = true;
    }

    private class MySqlConnModel
    {
        public string? Server { get; set; }
        public int Port { get; set; }
        public string? Database { get; set; }
        public string? User { get; set; }
        public string? Password { get; set; }
        public bool AllowPublicKeyRetrieval { get; set; }
        public string? SslMode { get; set; }
    }
}