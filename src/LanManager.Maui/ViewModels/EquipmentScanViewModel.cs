using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanManager.Maui.Shared.Services;
using System.Collections.ObjectModel;
using ZXing.Net.Maui;

namespace LanManager.Maui.ViewModels;

public partial class EquipmentScanViewModel : ObservableObject, IQueryAttributable
{
    private readonly ApiService _apiService;
    private Guid _eventId;
    private DateTime _lastScanTime = DateTime.MinValue;
    private const int ScanCooldownMs = 2000;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("eventId", out var id) && Guid.TryParse(id?.ToString(), out var guid))
            _eventId = guid;
    }

    [ObservableProperty] public partial CameraLocation CameraFacing { get; set; } = CameraLocation.Rear;
    [ObservableProperty] public partial ObservableCollection<EquipmentLoanDto> MyLoans { get; set; } = new();
    [ObservableProperty] public partial string StatusMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial Color StatusColor { get; set; } = Colors.Transparent;
    [ObservableProperty] public partial bool IsBusy { get; set; }

    public EquipmentScanViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task InitAsync()
    {
        var loans = await _apiService.GetMyLoansAsync();
        MyLoans.Clear();
        foreach (var loan in loans.Where(l => l.ReturnedAt == null))
            MyLoans.Add(loan);
    }

    public async Task OnBarcodeDetectedAsync(string barcodeValue)
    {
        if ((DateTime.Now - _lastScanTime).TotalMilliseconds < ScanCooldownMs || IsBusy) return;
        _lastScanTime = DateTime.Now;
        IsBusy = true;

        try
        {
            var loan = await _apiService.BorrowEquipmentAsync(barcodeValue, _eventId);
            ShowSuccess($"✓ Borrowed: {loan?.EquipmentName}");
            await InitAsync();
        }
        catch (HttpRequestException ex)
        {
            if (ex.Message.Contains("409")) ShowError("Already on loan — see staff");
            else if (ex.Message.Contains("400")) ShowError("Must be checked in first");
            else ShowError("Borrow failed");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ToggleCamera()
    {
        CameraFacing = CameraFacing == CameraLocation.Rear ? CameraLocation.Front : CameraLocation.Rear;
    }

    [RelayCommand]
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..");

    private async void ShowSuccess(string msg)
    {
        StatusMessage = msg;
        StatusColor = Colors.Green;
        await Task.Delay(2000);
        StatusMessage = string.Empty;
        StatusColor = Colors.Transparent;
    }

    private async void ShowError(string msg)
    {
        StatusMessage = msg;
        StatusColor = Colors.Red;
        await Task.Delay(3000);
        StatusMessage = string.Empty;
        StatusColor = Colors.Transparent;
    }
}

