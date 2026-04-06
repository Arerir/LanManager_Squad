using LanManager.Maui.Services;
using LanManager.Maui.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LanManager.Maui;

public partial class App : Application
{
    private readonly AuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    public App(AuthService authService, IServiceProvider serviceProvider)
    {
        InitializeComponent();  // loads App.xaml resources FIRST
        _authService = authService;
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var isLoggedIn = _authService.IsLoggedInAsync().GetAwaiter().GetResult();
        // Resolve LoginPage after InitializeComponent so App.xaml StaticResources are available
        Page startPage = isLoggedIn
            ? new AppShell()
            : _serviceProvider.GetRequiredService<LoginPage>();

        return new Window(startPage);
    }
}