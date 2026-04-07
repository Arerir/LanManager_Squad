using Microsoft.Extensions.Logging;
using LanManager.Maui.Services;
using LanManager.Maui.ViewModels;
using LanManager.Maui.Views;
using ZXing.Net.Maui.Controls;

namespace LanManager.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseBarcodeReader()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Auth
		builder.Services.AddSingleton<AppStateService>();
		builder.Services.AddSingleton<AuthService>();
		builder.Services.AddTransient<AuthHandler>();

		// Register HttpClient (with AuthHandler) and ApiService
		builder.Services.AddSingleton<ApiService>(sp =>
		{
			var authHandler = sp.GetRequiredService<AuthHandler>();
			authHandler.InnerHandler = new HttpClientHandler();
			var httpClient = new HttpClient(authHandler);
			return new ApiService(httpClient);
		});

		// Register ViewModels
		builder.Services.AddTransient<LoginViewModel>();
		builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<CheckInViewModel>();
		builder.Services.AddTransient<AttendanceViewModel>();
		builder.Services.AddTransient<DoorScanViewModel>();
		builder.Services.AddTransient<AttendeeHubViewModel>();
		builder.Services.AddTransient<AttendeeQrViewModel>();
		builder.Services.AddTransient<EquipmentScanViewModel>();

		// Register Views
		builder.Services.AddTransient<LoginPage>();
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<CheckInPage>();
		builder.Services.AddTransient<AttendancePage>();
		builder.Services.AddTransient<DoorScanPage>();
		builder.Services.AddTransient<AttendeeHubPage>();
		builder.Services.AddTransient<AttendeeQrPage>();
		builder.Services.AddTransient<EquipmentScanPage>();

		return builder.Build();
	}
}
