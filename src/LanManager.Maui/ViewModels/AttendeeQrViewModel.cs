using CommunityToolkit.Mvvm.ComponentModel;
using LanManager.Maui.Shared.Services;
using QRCoder;
using AppStateService = LanManager.Maui.Services.AppStateService;

namespace LanManager.Maui.ViewModels;

public partial class AttendeeQrViewModel : ObservableObject, IQueryAttributable
{
    private readonly AuthService _authService;
    private readonly AppStateService _appState;
    private Guid _eventId;

    [ObservableProperty] public partial ImageSource? QrImageSource { get; set; }
    [ObservableProperty] public partial string UserName { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string StatusMessage { get; set; } = string.Empty;

    public AttendeeQrViewModel(AuthService authService, AppStateService appState)
    {
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
            var pngBytes = await Task.Run(() =>
            {
                var qrGenerator = new QRCodeGenerator();
                var qrData = qrGenerator.CreateQrCode(userGuid.ToString(), QRCodeGenerator.ECCLevel.M);
                var qrCode = new PngByteQRCode(qrData);
                return qrCode.GetGraphic(10);
            });
            QrImageSource = ImageSource.FromStream(() => new MemoryStream(pngBytes));
        }
        catch (Exception ex)
        {
            StatusMessage = $"Failed to generate QR code: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}