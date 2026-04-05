using LanManager.Maui.ViewModels;

namespace LanManager.Maui.Views;

public partial class CheckInPage : ContentPage
{
    public CheckInPage(CheckInViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
