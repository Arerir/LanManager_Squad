using LanManager.Maui.ViewModels;
using ZXing.Net.Maui;

namespace LanManager.Maui.Views;

public partial class EquipmentScanPage : ContentPage
{
    private EquipmentScanViewModel ViewModel => (EquipmentScanViewModel)BindingContext;

    public EquipmentScanPage(EquipmentScanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.InitAsync();
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var first = e.Results.FirstOrDefault();
        if (first is null) return;
        MainThread.BeginInvokeOnMainThread(async () =>
            await ViewModel.OnBarcodeDetectedAsync(first.Value));
    }
}
