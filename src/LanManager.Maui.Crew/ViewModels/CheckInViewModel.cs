using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanManager.Maui.Shared.Services;
using System.Collections.ObjectModel;

namespace LanManager.Maui.Crew.ViewModels;

public partial class CheckInViewModel : ObservableObject, IQueryAttributable
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly AppStateService _appState;
    private List<UserDto> _allUsers = new();
    private Guid _eventId;

    [ObservableProperty]
    public partial string EventName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int CheckedInCount { get; set; }

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial ObservableCollection<UserDto> FilteredUsers { get; set; } = new();

    [ObservableProperty]
    public partial bool IsCheckOutMode { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial bool CanAccessDoorScan { get; set; }

    [ObservableProperty]
    public partial string StatusMessage { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Color StatusColor { get; set; } = Colors.Transparent;

    public CheckInViewModel(ApiService apiService, AuthService authService, AppStateService appState)
    {
        _apiService = apiService;
        _authService = authService;
        _appState = appState;
        CanAccessDoorScan = true;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("eventId", out var eventIdObj) && Guid.TryParse(eventIdObj.ToString(), out var eventId))
        {
            _eventId = eventId;
            _appState.SetEvent(_eventId, string.Empty);
        }
        else if (_appState.HasEvent)
        {
            _eventId = _appState.EventId;
        }
        _ = LoadDataAsync();
    }

    partial void OnSearchTextChanged(string value)
    {
        FilterUsers();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (_eventId == Guid.Empty && _appState.HasEvent)
            _eventId = _appState.EventId;

        if (_eventId == Guid.Empty)
        {
            await Shell.Current.GoToAsync("//MainPage");
            return;
        }

        CanAccessDoorScan = true;

        IsLoading = true;
        
        try
        {
            var eventsTask = _apiService.GetEventsAsync();
            var usersTask = _apiService.GetUsersAsync(1, 100);
            var attendanceTask = _apiService.GetAttendanceAsync(_eventId);

            await Task.WhenAll(eventsTask, usersTask, attendanceTask);

            var events = await eventsTask;
            var evt = events.FirstOrDefault(e => e.Id == _eventId);
            EventName = evt?.Name ?? "Unknown Event";

            _allUsers = await usersTask;
            
            var attendance = await attendanceTask;
            CheckedInCount = attendance.Count;

            FilterUsers();
        }
        catch (Exception ex)
        {
            ShowError($"Error loading data: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CheckInUserAsync(UserDto user)
    {
        IsLoading = true;
        
        try
        {
            if (IsCheckOutMode)
            {
                await _apiService.CheckOutAsync(_eventId, user.Id);
                ShowSuccess($"✓ Checked out: {user.Name}");
                CheckedInCount--;
            }
            else
            {
                await _apiService.CheckInAsync(_eventId, user.Id);
                ShowSuccess($"✓ Checked in: {user.Name}");
                CheckedInCount++;
            }
        }
        catch (HttpRequestException ex)
        {
            if (ex.Message.Contains("409"))
            {
                ShowError($"Already checked in: {user.Name}");
            }
            else if (ex.Message.Contains("400"))
            {
                ShowError($"Not registered: {user.Name}");
            }
            else if (ex.Message.Contains("404"))
            {
                ShowError($"No active check-in for: {user.Name}");
            }
            else
            {
                ShowError($"Error: {ex.Message}");
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GoToAttendanceAsync()
    {
        await Shell.Current.GoToAsync($"//AttendancePage?eventId={_eventId}");
    }

    [RelayCommand]
    private async Task GoToDoorScannerAsync()
    {
        var name = Uri.EscapeDataString(!string.IsNullOrEmpty(EventName) ? EventName : _appState.EventName);
        await Shell.Current.GoToAsync($"DoorScanPage?eventId={_eventId}&eventName={name}");
    }

    [RelayCommand]
    private void ToggleCheckOutMode()
    {
        IsCheckOutMode = !IsCheckOutMode;
        StatusMessage = IsCheckOutMode ? "CHECK-OUT MODE" : "CHECK-IN MODE";
        StatusColor = IsCheckOutMode ? Colors.Orange : Colors.Green;
    }

    private void FilterUsers()
    {
        FilteredUsers.Clear();

        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allUsers
            : _allUsers.Where(u =>
                u.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                u.UserName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)
            );

        foreach (var user in filtered.Take(50))
        {
            FilteredUsers.Add(user);
        }
    }

    private async void ShowSuccess(string message)
    {
        StatusMessage = message;
        StatusColor = Colors.Green;
        await Task.Delay(2000);
        StatusMessage = string.Empty;
        StatusColor = Colors.Transparent;
    }

    private async void ShowError(string message)
    {
        StatusMessage = message;
        StatusColor = Colors.Red;
        await Task.Delay(3000);
        StatusMessage = string.Empty;
        StatusColor = Colors.Transparent;
    }
}


