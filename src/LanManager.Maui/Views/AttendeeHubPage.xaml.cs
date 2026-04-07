using LanManager.Maui.ViewModels;

namespace LanManager.Maui.Views;

public partial class AttendeeHubPage : ContentPage
{
    public AttendeeHubPage(AttendeeHubViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        if (BindingContext is AttendeeHubViewModel vm)
            await vm.CleanupAsync();
    }
}
