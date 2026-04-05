using Microsoft.Extensions.Logging;
using LanManager.Maui.Services;
using LanManager.Maui.ViewModels;
using LanManager.Maui.Views;

namespace LanManager.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		// Register HttpClient and ApiService
		builder.Services.AddSingleton<HttpClient>();
		builder.Services.AddSingleton<ApiService>();

		// Register ViewModels
		builder.Services.AddTransient<MainViewModel>();
		builder.Services.AddTransient<CheckInViewModel>();
		builder.Services.AddTransient<AttendanceViewModel>();

		// Register Views
		builder.Services.AddTransient<MainPage>();
		builder.Services.AddTransient<CheckInPage>();
		builder.Services.AddTransient<AttendancePage>();

		return builder.Build();
	}
}
