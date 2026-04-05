using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanManager.Maui.Services;

namespace LanManager.Maui.ViewModels;

public partial class DoorScanViewModel : ObservableObject, IQueryAttributable
{
    private readonly ApiService _apiService;
    private Guid _eventId;
    private DateTime _lastScanTime = DateTime.MinValue;
    private const int ScanCooldownMs = 1000;

    [ObservableProperty] private string _eventName = string.Empty;
    [ObservableProperty] private bool _isExitMode = true; // true=Exit, false=Entry
    [ObservableProperty] private string _directionLabel = "EXIT";
    [ObservableProperty] private Color _directionColor = Colors.Red;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private Color _statusColor = Colors.Transparent;
    [ObservableProperty] private int _outsideCount;
    [ObservableProperty] private bool _isBusy;

    public DoorScanViewModel(ApiService apiService) { _apiService = apiService; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("eventId", out var id) && Guid.TryParse(id?.ToString(), out var guid))
        {
            _eventId = guid;
            _ = RefreshOutsideCountAsync();
        }
        if (query.TryGetValue("eventName", out var name))
            EventName = name?.ToString() ?? string.Empty;
    }

    [RelayCommand]
    private void ToggleDirection()
    {
        IsExitMode = !IsExitMode;
        DirectionLabel = IsExitMode ? "EXIT" : "ENTRY";
        DirectionColor = IsExitMode ? Colors.Red : Colors.Green;
    }

    // Called when ZXing detects a barcode
    public async Task OnBarcodeDetectedAsync(string barcodeValue)
    {
        // Cooldown check
        if ((DateTime.Now - _lastScanTime).TotalMilliseconds < ScanCooldownMs) return;
        _lastScanTime = DateTime.Now;

        if (!Guid.TryParse(barcodeValue, out var userId)) return;
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            var direction = IsExitMode ? "Exit" : "Entry";
            var result = await _apiService.DoorScanAsync(_eventId, userId, direction);
            var emoji = IsExitMode ? "→" : "←";
            ShowSuccess($"{emoji} {(IsExitMode ? "Outside" : "Back")}: {result?.UserName}");
            _ = RefreshOutsideCountAsync();
        }
        catch (HttpRequestException ex)
        {
            if (ex.Message.Contains("400")) ShowError("Not checked in");
            else if (ex.Message.Contains("404")) ShowError("User not found");
            else ShowError("Scan failed");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RefreshOutsideCountAsync()
    {
        OutsideCount = await _apiService.GetOutsideCountAsync(_eventId);
    }

    [RelayCommand]
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

    private async void ShowSuccess(string msg)
    {
        StatusMessage = msg; StatusColor = Colors.Green;
        await Task.Delay(2000);
        StatusMessage = string.Empty; StatusColor = Colors.Transparent;
    }
    private async void ShowError(string msg)
    {
        StatusMessage = msg; StatusColor = Colors.Red;
        await Task.Delay(3000);
        StatusMessage = string.Empty; StatusColor = Colors.Transparent;
    }
}
