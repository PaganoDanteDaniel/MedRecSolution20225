using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MedRec.WPF.UI;
public partial class MainWindow : Window
{
    private IServiceScope? _currentScope;
    private int _scopeSeq = 0; // <-- contador de scopes

    public MainWindow()
    {
        Resources.Add("services", Startup.Services);
        InitializeComponent();
    }

    public void NavigateTo<TComponent>() where TComponent : IComponent
    {
        _currentScope?.Dispose();
        _currentScope = Startup.Services!.CreateScope();

        blazorWebView.RootComponents.Clear();
        blazorWebView.Services = _currentScope.ServiceProvider;
        blazorWebView.RootComponents.Add(new RootComponent
        {
            ComponentType = typeof(TComponent),
            Selector = "#app"
        });
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
    }
}
