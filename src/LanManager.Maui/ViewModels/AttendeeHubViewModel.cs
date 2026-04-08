using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LanManager.Maui.Shared.Services;
using System.Collections.ObjectModel;
using AppStateService = LanManager.Maui.Services.AppStateService;

namespace LanManager.Maui.ViewModels;

public partial class AttendeeHubViewModel : ObservableObject, IQueryAttributable
{
    private readonly ApiService _apiService;
    private readonly AppStateService _appState;
    private Guid _eventId;

    [ObservableProperty] private string _seatLabel = "No seat assigned";
    [ObservableProperty] private string _seatDetail = string.Empty;
    [ObservableProperty] private ObservableCollection<TournamentDto> _tournaments = new();
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _statusMessage = string.Empty;
    [ObservableProperty] private Color _statusColor = Colors.Gray;
    [ObservableProperty] private string _tournamentStatus = string.Empty;

    public AttendeeHubViewModel(ApiService apiService, AppStateService appState)
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
        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (_eventId == Guid.Empty)
        {
            await Shell.Current.GoToAsync("//MainPage");
            return;
        }

        IsLoading = true;
        StatusMessage = string.Empty;
        TournamentStatus = string.Empty;

        try
        {
            var seatTask = _apiService.GetMySeatAsync(_eventId);
            var tournamentsTask = _apiService.GetTournamentsAsync(_eventId);

            await Task.WhenAll(seatTask, tournamentsTask);

            var seat = await seatTask;
            if (seat != null)
            {
                SeatLabel = seat.Label;
                SeatDetail = $"Row {seat.Row + 1}";
            }
            else
            {
                SeatLabel = "No seat assigned";
                SeatDetail = "Check with staff";
            }

            var allTournaments = await tournamentsTask;
            var open = allTournaments.Where(t => t.Status == "Active").ToList();
            Tournaments.Clear();
            foreach (var t in open)
                Tournaments.Add(t);

            if (Tournaments.Count == 0)
                TournamentStatus = "No active tournaments at this time";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading data: {ex.Message}";
            StatusColor = Colors.Red;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task EnrolAsync(string tournamentIdStr)
    {
        if (!Guid.TryParse(tournamentIdStr, out var tournamentId)) return;

        try
        {
            await _apiService.SelfEnrolTournamentAsync(tournamentId);
            StatusMessage = "✓ Enrolled!";
            StatusColor = Colors.Green;
            await LoadAsync();
        }
        catch (HttpRequestException ex)
        {
            if (ex.Message.Contains("409"))
            {
                StatusMessage = "Already enrolled";
                StatusColor = Colors.Orange;
            }
            else if (ex.Message.Contains("400"))
            {
                StatusMessage = ex.Message;
                StatusColor = Colors.Red;
            }
            else
            {
                StatusMessage = "Enrolment failed";
                StatusColor = Colors.Red;
            }
        }
        catch
        {
            StatusMessage = "Enrolment failed";
            StatusColor = Colors.Red;
        }
    }

    [RelayCommand]
    private async Task GoToQrPageAsync()
        => await Shell.Current.GoToAsync($"AttendeeQrPage?eventId={_eventId}");

    [RelayCommand]
    private async Task GoToEquipmentScanAsync()
        => await Shell.Current.GoToAsync("EquipmentScanPage");
}

