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
        InitializeComponent();
        _authService = authService;
        _serviceProvider = serviceProvider;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new AppShell();
        return new Window(shell);
    }

    protected override async void OnStart()
    {
        base.OnStart();
        try
        {
            var isLoggedIn = await _authService.IsLoggedInAsync();
            if (!isLoggedIn)
            {
                ShowLoginPage();
            }
            else if (Windows.Count > 0 && Windows[0].Page is AppShell shell)
            {
                shell.UpdateMenuForUser(_authService.CurrentUser);
            }
        }
        catch
        {
            ShowLoginPage();
        }
    }

    internal void ShowLoginPage()
    {
        var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
        if (Windows.Count > 0)
            Windows[0].Page = loginPage;
    }
}