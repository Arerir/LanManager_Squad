using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanManager.Maui.Shared.Services;

namespace LanManager.Maui.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;

    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private bool _isBusy;

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Email and password are required.";
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var success = await _authService.LoginAsync(Email, Password);
            if (success)
            {
                var shell = new AppShell();
                shell.UpdateMenuForUser(_authService.CurrentUser);
                if (Application.Current?.Windows.Count > 0)
                    Application.Current.Windows[0].Page = shell;
            }
            else
            {
                ErrorMessage = "Invalid email or password.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Login failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}

