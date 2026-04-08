using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanManager.Maui.Shared.Services;
using System.Collections.ObjectModel;
using AppStateService = LanManager.Maui.Services.AppStateService;

namespace LanManager.Maui.ViewModels;

public partial class MyEquipmentViewModel : ObservableObject, IQueryAttributable
{
    private readonly ApiService _apiService;
    private readonly AppStateService _appState;
    private Guid _eventId;

    [ObservableProperty] public partial ObservableCollection<EquipmentLoanDto> ActiveLoans { get; set; } = new();
    [ObservableProperty] public partial ObservableCollection<EquipmentLoanDto> ReturnedLoans { get; set; } = new();
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string StatusMessage { get; set; } = string.Empty;
    [ObservableProperty] public partial bool HasActiveLoans { get; set; }
    [ObservableProperty] public partial bool HasReturnedLoans { get; set; }

    public MyEquipmentViewModel(ApiService apiService, AppStateService appState)
    {
        _apiService = apiService;
        _appState = appState;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("eventId", out var id) && Guid.TryParse(id?.ToString(), out var guid))
            _eventId = guid;
        else if (_appState.HasEvent)
            _eventId = _appState.EventId;
    }

    public async Task LoadAsync()
    {
        if (_eventId == Guid.Empty)
            return;

        IsLoading = true;
        StatusMessage = string.Empty;

        try
        {
            var loans = await _apiService.GetMyLoansAsync();
            var eventLoans = loans.Where(l => l.EventId == _eventId).ToList();

            var active = eventLoans.Where(l => l.ReturnedAt == null).ToList();
            var returned = eventLoans.Where(l => l.ReturnedAt != null)
                                     .OrderByDescending(l => l.ReturnedAt)
                                     .ToList();

            ActiveLoans.Clear();
            foreach (var loan in active)
                ActiveLoans.Add(loan);

            ReturnedLoans.Clear();
            foreach (var loan in returned)
                ReturnedLoans.Add(loan);

            HasActiveLoans = ActiveLoans.Count > 0;
            HasReturnedLoans = ReturnedLoans.Count > 0;

            if (eventLoans.Count == 0)
                StatusMessage = "No equipment registered for this event";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MyEquipmentViewModel.LoadAsync error: {ex.Message}");
            StatusMessage = "Failed to load equipment";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync() => await LoadAsync();

    [RelayCommand]
    private async Task GoBackAsync() => await Shell.Current.GoToAsync("..");
}
