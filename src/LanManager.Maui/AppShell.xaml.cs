using LanManager.Maui.Views;

namespace LanManager.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
		Routing.RegisterRoute(nameof(AttendeeHubPage), typeof(AttendeeHubPage));
		Routing.RegisterRoute(nameof(AttendeeQrPage), typeof(AttendeeQrPage));
		Routing.RegisterRoute(nameof(EquipmentScanPage), typeof(EquipmentScanPage));
		Routing.RegisterRoute(nameof(MyEquipmentPage), typeof(MyEquipmentPage));
	}
}

