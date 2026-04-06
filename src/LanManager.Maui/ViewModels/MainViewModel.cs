using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanManager.Maui.Services;
using System.Collections.ObjectModel;

namespace LanManager.Maui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;

    [ObservableProperty]
    private ObservableCollection<EventDto> _events = new();

    [ObservableProperty]
    private EventDto? _selectedEvent;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public MainViewModel(ApiService apiService, AuthService authService)
    {
        _apiService = apiService;
        _authService = authService;
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

        var isAttendeeOnly = _authService.CurrentUser?.Roles
            .All(r => r == "Attendee") ?? true;

        if (isAttendeeOnly)
            await Shell.Current.GoToAsync($"AttendeeHubPage?eventId={SelectedEvent.Id}");
        else
            await Shell.Current.GoToAsync($"//CheckInPage?eventId={SelectedEvent.Id}");
    }
}
