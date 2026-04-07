using LanManager.Maui.Crew.ViewModels;

namespace LanManager.Maui.Crew.Views;

public partial class CheckInPage : ContentPage
{
    public CheckInPage(CheckInViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

