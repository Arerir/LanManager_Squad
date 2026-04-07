using LanManager.Maui.Crew.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LanManager.Maui.Crew;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loginPage = IPlatformApplication.Current!.Services.GetRequiredService<LoginPage>();
        return new Window(loginPage);
    }
}
