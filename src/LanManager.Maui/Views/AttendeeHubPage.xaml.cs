using LanManager.Maui.ViewModels;

namespace LanManager.Maui.Views;

public partial class AttendeeHubPage : ContentPage
{
    private AttendeeHubViewModel ViewModel => (AttendeeHubViewModel)BindingContext;

    public AttendeeHubPage(AttendeeHubViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
