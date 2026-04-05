using LanManager.Maui.Views;

namespace LanManager.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("DoorScanPage", typeof(DoorScanPage));
	}
}
