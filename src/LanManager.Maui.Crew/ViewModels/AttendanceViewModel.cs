using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanManager.Maui.Shared.Services;
using System.Collections.ObjectModel;

namespace LanManager.Maui.Crew.ViewModels;

public partial class AttendanceViewModel : ObservableObject, IQueryAttributable
{
    private readonly ApiService _apiService;
    private readonly AuthService _authService;
    private readonly AppStateService _appState;
    private Guid _eventId;
    private string _eventStatus = string.Empty;

    [ObservableProperty]
    private ObservableCollection<AttendanceDto> _attendees = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isEventClosed;

    [ObservableProperty]
    private bool _canDownloadReport;

    [ObservableProperty]
    private bool _showReportPanel;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private bool _includeRegistrations = true;

    [ObservableProperty]
    private bool _includeCheckIns = true;

    [ObservableProperty]
    private bool _includeEquipment = true;

    [ObservableProperty]
    private bool _includeTournaments = true;

    public AttendanceViewModel(ApiService apiService, AuthService authService, AppStateService appState)
    {
        _apiService = apiService;
        _authService = authService;
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
            var attendanceTask = _apiService.GetAttendanceAsync(_eventId);
            var eventsTask = _apiService.GetEventsAsync();
            await Task.WhenAll(attendanceTask, eventsTask);

            var attendance = await attendanceTask;
            Attendees.Clear();
            foreach (var attendee in attendance.OrderByDescending(a => a.CheckedInAt))
                Attendees.Add(attendee);

            var evt = (await eventsTask).FirstOrDefault(e => e.Id == _eventId);
            _eventStatus = evt?.Status ?? string.Empty;
            IsEventClosed = string.Equals(_eventStatus, "Closed", StringComparison.OrdinalIgnoreCase);
            CanDownloadReport = IsEventClosed &&
                (_authService.CurrentUser?.Roles.Any(r => r == "Admin" || r == "Organizer") ?? false);

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

    [RelayCommand]
    private void ToggleReportPanel()
    {
        ShowReportPanel = !ShowReportPanel;
    }

    [RelayCommand]
    private async Task DownloadReportAsync()
    {
        var selected = new List<string>();
        if (IncludeRegistrations) selected.Add("Registrations");
        if (IncludeCheckIns) selected.Add("CheckIns");
        if (IncludeEquipment) selected.Add("Equipment");
        if (IncludeTournaments) selected.Add("Tournaments");

        var sections = selected.Count == 4 ? "All" : selected.Count > 0 ? string.Join(",", selected) : "Summary";

        IsDownloading = true;
        try
        {
            var pdfBytes = await _apiService.DownloadReportAsync(_eventId, sections);
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                await Shell.Current.DisplayAlertAsync("Download Failed", "Could not download the report. The event must be closed and you must have Admin or Organizer role.", "OK");
                return;
            }

            var filePath = Path.Combine(FileSystem.CacheDirectory, $"event-report-{_eventId}.pdf");
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Event Report",
                File = new ShareFile(filePath)
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Report download failed: {ex.Message}", "OK");
        }
        finally
        {
            IsDownloading = false;
        }
    }
}
