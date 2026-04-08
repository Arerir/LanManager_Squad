using LanManager.Maui.Crew.Views;

namespace LanManager.Maui.Crew;

public partial class CrewAppShell : Shell
{
    public CrewAppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(DoorScanPage), typeof(DoorScanPage));
    }
}
