using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanManager.Maui.Services;
using System.Collections.ObjectModel;

namespace LanManager.Maui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<EventDto> _events = new();

    [ObservableProperty]
    private EventDto? _selectedEvent;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public MainViewModel(ApiService apiService)
    {
        _apiService = apiService;
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

        await Shell.Current.GoToAsync($"//CheckInPage?eventId={SelectedEvent.Id}");
    }
}
