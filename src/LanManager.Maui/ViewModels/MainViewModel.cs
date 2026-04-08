using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanManager.Maui.Shared.Services;
using System.Collections.ObjectModel;
using AppStateService = LanManager.Maui.Services.AppStateService;

namespace LanManager.Maui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly AppStateService _appState;

    [ObservableProperty]
    public partial ObservableCollection<EventDto> Events { get; set; } = new();

    [ObservableProperty]
    public partial EventDto? SelectedEvent { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    public MainViewModel(ApiService apiService, AuthService authService, AppStateService appState)
    {
        _apiService = apiService;
        _authService = authService;
        _appState = appState;
    }

    [RelayCommand]
    private async Task LoadEventsAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading active events...";
        
        try
        {
            var events = await _apiService.GetEventsAsync("Active");
            Events.Clear();
            
            foreach (var evt in events)
            {
                Events.Add(evt);
            }

            if (Events.Count == 0)
            {
                StatusMessage = "No active events found";
            }
            else
            {
                StatusMessage = string.Empty;
                SelectedEvent = Events.FirstOrDefault();
            }

        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading events: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SelectEventAsync()
    {
        if (SelectedEvent == null)
        {
            StatusMessage = "Please select an event";
            return;
        }

        _appState.SetEvent(SelectedEvent.Id, SelectedEvent.Name);
        await Shell.Current.GoToAsync($"AttendeeHubPage?eventId={SelectedEvent.Id}");
    }
}

