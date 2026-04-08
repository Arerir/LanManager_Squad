using CommunityToolkit.Mvvm.ComponentModel;
using LanManager.Maui.Shared.Services;
using QRCoder;
using AppStateService = LanManager.Maui.Services.AppStateService;

namespace LanManager.Maui.ViewModels;

public partial class AttendeeQrViewModel : ObservableObject, IQueryAttributable
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly AppStateService _appState;
    private Guid _eventId;

    [ObservableProperty] public partial ImageSource? QrImageSource { get; set; }
    [ObservableProperty] public partial string UserName { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string StatusMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial Color PageBackground { get; set; } = Color.FromArgb("#0d0d1a");

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
            // Generate QR on background thread
            var pngBytes = await Task.Run(() =>
            {
                var qrGenerator = new QRCodeGenerator();
                var qrData = qrGenerator.CreateQrCode(userGuid.ToString(), QRCodeGenerator.ECCLevel.M);
                var qrCode = new PngByteQRCode(qrData);
                return qrCode.GetGraphic(10);
            });
            QrImageSource = ImageSource.FromStream(() => new MemoryStream(pngBytes));

            // Fetch door status
            if (_eventId != Guid.Empty)
            {
                var status = await _apiService.GetAttendeeDoorStatusAsync(_eventId, userGuid);
                ApplyStatus(status);
            }
            else
            {
                StatusMessage = "Show this QR to check in";
                PageBackground = Color.FromArgb("#2d0050"); // Purple — unregistered
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to load: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyStatus(string? status)
    {
        switch (status)
        {
            case "Entry":
                StatusMessage = "✅ You are checked in";
                PageBackground = Color.FromArgb("#0a3a1a"); // Dark green
                break;
            case "Exit":
                StatusMessage = "🚪 You are currently outside";
                PageBackground = Color.FromArgb("#3a0a0a"); // Dark red
                break;
            default: // Unregistered or null
                StatusMessage = "Show this QR to check in";
                PageBackground = Color.FromArgb("#2d0050"); // Dark purple
                break;
        }
    }
}
