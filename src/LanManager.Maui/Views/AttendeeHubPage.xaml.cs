using LanManager.Maui.ViewModels;

namespace LanManager.Maui.Views;

public partial class AttendeeHubPage : ContentPage
{
    public AttendeeHubPage(AttendeeHubViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
