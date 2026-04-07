using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanManager.Maui.Shared.Services;
using System.Collections.ObjectModel;

namespace LanManager.Maui.Crew.ViewModels;

public partial class AttendanceViewModel : ObservableObject, IQueryAttributable
{
    private readonly ApiService _apiService;
    private readonly AppStateService _appState;
    private Guid _eventId;

    [ObservableProperty]
    private ObservableCollection<AttendanceDto> _attendees = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public AttendanceViewModel(ApiService apiService, AppStateService appState)
    {
        _apiService = apiService;
        _appState = appState;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("eventId", out var eventIdObj) && Guid.TryParse(eventIdObj.ToString(), out var eventId))
            _eventId = eventId;
        else if (_appState.HasEvent)
            _eventId = _appState.EventId;
        _ = LoadAttendanceAsync();
    }

    [RelayCommand]
    private async Task LoadAttendanceAsync()
    {
        if (_eventId == Guid.Empty)
        {
            await Shell.Current.GoToAsync("//MainPage");
            return;
        }

        IsLoading = true;
        StatusMessage = "Loading attendance...";
        
        try
        {
            var attendance = await _apiService.GetAttendanceAsync(_eventId);
            Attendees.Clear();
            
            foreach (var attendee in attendance.OrderByDescending(a => a.CheckedInAt))
            {
                Attendees.Add(attendee);
            }

            StatusMessage = Attendees.Count == 0 ? "No attendees checked in" : string.Empty;
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading attendance: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        await LoadAttendanceAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync($"//CheckInPage?eventId={_eventId}");
    }
}


