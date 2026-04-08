using LanManager.Maui.Crew.ViewModels;
using ZXing.Net.Maui;

namespace LanManager.Maui.Crew.Views;

public partial class DoorScanPage : ContentPage
{
    private DoorScanViewModel _viewModel => (DoorScanViewModel)BindingContext;

    public DoorScanPage(DoorScanViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        var first = e.Results.FirstOrDefault();
        if (first is null) return;
        MainThread.BeginInvokeOnMainThread(async () =>
            await _viewModel.OnBarcodeDetectedAsync(first.Value));
    }
}

