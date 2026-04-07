using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanManager.Maui.Shared.Services;
using System.Collections.ObjectModel;

namespace LanManager.Maui.ViewModels;

public partial class EquipmentScanViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private DateTime _lastScanTime = DateTime.MinValue;
    private const int ScanCooldownMs = 2000;

    [ObservableProperty] private ObservableCollection<EquipmentLoanDto> _myLoans = new();
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private Color _statusColor = Colors.Transparent;
    [ObservableProperty] private bool _isBusy;

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
            var loan = await _apiService.BorrowEquipmentAsync(barcodeValue);
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

