using LanManager.Maui.ViewModels;

namespace LanManager.Maui.Views;

public partial class AttendancePage : ContentPage
{
    public AttendancePage(AttendanceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
