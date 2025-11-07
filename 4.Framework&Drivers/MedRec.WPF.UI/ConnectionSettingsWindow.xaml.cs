using MedRec.WPF.UI.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MedRec.WPF.UI;

public partial class ConnectionSettingsWindow : Window
{
    public ConnectionSettingsWindow(string appSettingsPath)
    {
        InitializeComponent();

        // Crear un ServiceProvider mínimo para el componente (si aún no existe Startup.Services)
        var services = new ServiceCollection();
        services.AddWpfBlazorWebView();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
        services.AddSingleton(new MySqlConnectionConfigService(appSettingsPath));

        var sp = services.BuildServiceProvider();
        BlazorHost.Services = sp;

        // Suscribir cierre al evento
        var cfg = sp.GetRequiredService<MySqlConnectionConfigService>();
        cfg.ConnectionSaved += () => Dispatcher.Invoke(Close);
    }
}
