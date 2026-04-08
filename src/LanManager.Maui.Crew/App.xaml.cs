namespace LanManager.Maui.Crew;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = new CrewAppShell();
        return new Window(shell);
    }
}
