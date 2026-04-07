using CommunityToolkit.Mvvm.ComponentModel;
using LanManager.Maui.Shared.Services;
using LanManager.Maui.Services;

namespace LanManager.Maui.ViewModels;

public partial class AttendeeQrViewModel : ObservableObject, IQueryAttributable
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly AppStateService _appState;
    private Guid _eventId;

    [ObservableProperty] private ImageSource? _qrImageSource;
    [ObservableProperty] private string _userName = string.Empty;
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _statusMessage = string.Empty;

    public AttendeeQrViewModel(ApiService apiService, AuthService authService, AppStateService appState)
    {
        _apiService = apiService;
        _authService = authService;
        _appState = appState;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("eventId", out var id) && Guid.TryParse(id?.ToString(), out var guid))
            _eventId = guid;
        else if (_appState.HasEvent)
            _eventId = _appState.EventId;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        IsLoading = true;
        StatusMessage = string.Empty;
        UserName = _authService.CurrentUser?.Name ?? string.Empty;
        var userId = _authService.CurrentUser?.Id;

        if (userId == null || !Guid.TryParse(userId, out var userGuid))
        {
            StatusMessage = "Not logged in";
            IsLoading = false;
            return;
        }

        try
        {
            var bytes = await _apiService.GetAttendeeQrCodeAsync(_eventId, userGuid);
            if (bytes != null)
                QrImageSource = ImageSource.FromStream(() => new MemoryStream(bytes));
            else
                StatusMessage = "QR code not available. Check your event registration.";
        }
        catch
        {
            StatusMessage = "Failed to load QR code.";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

