using CommunityToolkit.Mvvm.ComponentModel;
using LanManager.Maui.Shared.Services;
using AppStateService = LanManager.Maui.Services.AppStateService;

namespace LanManager.Maui.ViewModels;

public partial class AttendeeQrViewModel : ObservableObject, IQueryAttributable
{
    private readonly AuthService _authService;
    private readonly AppStateService _appState;

    [ObservableProperty] public partial string QrValue { get; set; } = string.Empty;
    [ObservableProperty] public partial string UserName { get; set; } = string.Empty;
    [ObservableProperty] public partial string StatusMessage { get; set; } = string.Empty;

    public AttendeeQrViewModel(AuthService authService, AppStateService appState)
    {
        _authService = authService;
        _appState = appState;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        Load();
    }

    private void Load()
    {
        UserName = _authService.CurrentUser?.Name ?? string.Empty;
        var userId = _authService.CurrentUser?.Id;

        if (string.IsNullOrEmpty(userId))
        {
            StatusMessage = "Not logged in";
            return;
        }

        QrValue = userId;
        StatusMessage = string.Empty;
    }
}
