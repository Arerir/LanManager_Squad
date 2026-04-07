using LanManager.Maui.Crew.ViewModels;

namespace LanManager.Maui.Crew.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

