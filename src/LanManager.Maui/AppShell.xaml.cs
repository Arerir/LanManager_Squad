using LanManager.Maui.Views;

namespace LanManager.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute("LoginPage", typeof(LoginPage));
		Routing.RegisterRoute("AttendeeHubPage", typeof(AttendeeHubPage));
		Routing.RegisterRoute("AttendeeQrPage", typeof(AttendeeQrPage));
		Routing.RegisterRoute("EquipmentScanPage", typeof(EquipmentScanPage));
	}
}

