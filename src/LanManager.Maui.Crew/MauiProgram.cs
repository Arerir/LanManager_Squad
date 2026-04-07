using LanManager.Maui.Crew.ViewModels;
using LanManager.Maui.Crew.Views;
using LanManager.Maui.Shared.Services;
using Microsoft.Extensions.Logging;
using ZXing.Net.Maui.Controls;

namespace LanManager.Maui.Crew;

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

        // HttpClient with AuthHandler
        builder.Services.AddSingleton<ApiService>(sp =>
        {
            var authHandler = sp.GetRequiredService<AuthHandler>();
            authHandler.InnerHandler = new HttpClientHandler();
            var httpClient = new HttpClient(authHandler);
            return new ApiService(httpClient);
        });

        // ViewModels
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<CheckInViewModel>();
        builder.Services.AddTransient<AttendanceViewModel>();
        builder.Services.AddTransient<DoorScanViewModel>();

        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<CheckInPage>();
        builder.Services.AddTransient<AttendancePage>();
        builder.Services.AddTransient<DoorScanPage>();

        return builder.Build();
    }
}
