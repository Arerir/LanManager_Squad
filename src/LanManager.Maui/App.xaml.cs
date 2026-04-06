using LanManager.Maui.Services;
using LanManager.Maui.ViewModels;
using LanManager.Maui.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LanManager.Maui;

public partial class App : Application
{
    private readonly AuthService _authService;
    private readonly LoginPage _loginPage;
    private readonly IServiceProvider _serviceProvider;

    public App(AuthService authService, LoginPage loginPage, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _authService = authService;
        _loginPage = loginPage;
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var isLoggedIn = _authService.IsLoggedInAsync().GetAwaiter().GetResult();
        Page startPage = isLoggedIn
            ? new AppShell()
            : _loginPage;

        return new Window(startPage);
    }
}