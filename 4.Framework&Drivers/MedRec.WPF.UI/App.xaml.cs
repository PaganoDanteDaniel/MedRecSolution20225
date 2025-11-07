using System.Windows;
namespace MedRec.WPF.UI;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Evita que cerrar la ventana de configuración cierre toda la app.
        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var path = MedRec.WPF.UI.Startup.GetAppSettingsPath();
        bool ok = MedRec.WPF.UI.Startup.EnsureConnectionSettings(path);

        if (!ok)
        {
            MessageBox.Show("No se configuró la cadena de conexión. La aplicación se cerrará.", "MedRec",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            Shutdown();
            return;
        }

        MedRec.WPF.UI.Startup.BuildHost(path);

        var main = new MainWindow();
        MainWindow = main;
        main.Show();

        // Ahora sí: cerrar la ventana principal cierra la app.
        Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
    }
}
