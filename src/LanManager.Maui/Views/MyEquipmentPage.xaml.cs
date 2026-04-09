using LanManager.Maui.ViewModels;

namespace LanManager.Maui.Views;

public partial class MyEquipmentPage : ContentPage
{
    public MyEquipmentPage(MyEquipmentViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is MyEquipmentViewModel vm)
            await vm.LoadAsync();
    }
}
