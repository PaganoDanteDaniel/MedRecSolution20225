using System.Windows;
using StartupSvc = MedRec.WPF.UI.Startup;

namespace MedRec.WPF.UI;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var path = StartupSvc.GetAppSettingsPath();
        bool ok = StartupSvc.EnsureConnectionSettings(path);

        if (!ok)
        {
            MessageBox.Show("No se configuró la cadena de conexión. La aplicación se cerrará.", "MedRec",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            Shutdown();
            return;
        }

        StartupSvc.BuildHost(path);

        var main = new MainWindow();
        MainWindow = main;
        main.Show();

        // Ahora sí: cerrar la ventana principal cierra la app.
        Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
    }
}
