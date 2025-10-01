using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;

using System.Windows;

namespace MedRec.WPF.UI;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private IServiceScope? _currentScope;
    public MainWindow()
    {
        Resources.Add("services", Startup.Services);
        InitializeComponent();
    }

    public void NavigateTo<TComponent>() where TComponent : IComponent
    {
        // 1. Dispose del scope anterior
        _currentScope?.Dispose();

        // 2. Crear un nuevo scope
        _currentScope = Startup.Services!.CreateScope();

        // 3. Resolver el RootComponent dentro del scope actual
        blazorWebView.RootComponents.Clear();
        blazorWebView.RootComponents.Add(new RootComponent
        {
            ComponentType = typeof(TComponent),
            Selector = "#app"
        });

        // 4. Asignar los servicios del scope al BlazorWebView
        blazorWebView.Services = _currentScope.ServiceProvider;
    }
}
