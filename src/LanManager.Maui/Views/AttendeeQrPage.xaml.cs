using LanManager.Maui.ViewModels;

namespace LanManager.Maui.Views;

public partial class AttendeeQrPage : ContentPage
{
    public AttendeeQrPage(AttendeeQrViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
