using LanManager.Maui.Services;
using LanManager.Maui.Views;

namespace LanManager.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		// DoorScanPage is a ShellContent (sidebar) — no route registration needed
		Routing.RegisterRoute("LoginPage", typeof(LoginPage));
		Routing.RegisterRoute("AttendeeHubPage", typeof(AttendeeHubPage));
		Routing.RegisterRoute("AttendeeQrPage", typeof(AttendeeQrPage));
		Routing.RegisterRoute("EquipmentScanPage", typeof(EquipmentScanPage));
	}

	public void UpdateMenuForUser(AuthUser? user)
	{
		var roles = user?.Roles ?? new List<string>();
		var canScan = roles.Any(r => r == "Admin" || r == "Organizer" || r == "Operator");
		ScannerContent.IsVisible = canScan;
	}
}
