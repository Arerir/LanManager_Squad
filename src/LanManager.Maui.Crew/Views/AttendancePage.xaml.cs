using LanManager.Maui.Crew.ViewModels;

namespace LanManager.Maui.Crew.Views;

public partial class AttendancePage : ContentPage
{
    public AttendancePage(AttendanceViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}

