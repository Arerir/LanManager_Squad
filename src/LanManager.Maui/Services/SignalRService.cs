using LanManager.Maui.Shared;
using LanManager.Maui.Shared.Services;
using Microsoft.AspNetCore.SignalR.Client;

namespace LanManager.Maui.Services;

public class SignalRService
{
    private readonly AuthService _authService;
    private HubConnection? _connection;
    private Guid _eventId;

    public Action<string>? OnCheckedIn { get; set; }
    public Action<string>? OnDoorScanned { get; set; }

    public SignalRService(AuthService authService)
    {
        _authService = authService;
    }

    public async Task ConnectAsync(Guid eventId)
    {
        _eventId = eventId;

        if (_connection is not null)
            await DisconnectAsync();

        _connection = new HubConnectionBuilder()
            .WithUrl($"{Config.ApiBaseUrl}/hubs/attendance", options =>
            {
                options.AccessTokenProvider = async () => await _authService.GetTokenAsync();
            })
            .WithAutomaticReconnect()
            .Build();

        _connection.On<Guid, string, string, DateTime>("UserCheckedIn",
            (evId, userId, userName, checkedInAt) =>
            {
                if (evId != _eventId) return;
                if (userId != _authService.CurrentUser?.Id) return;
                OnCheckedIn?.Invoke("✓ You're checked in!");
            });

        _connection.On<Guid, string, string, string, DateTime>("UserDoorScanned",
            (evId, userId, userName, direction, scannedAt) =>
            {
                if (evId != _eventId) return;
                if (userId != _authService.CurrentUser?.Id) return;
                var msg = direction == "Exit" ? "→ You went outside" : "← Welcome back!";
                OnDoorScanned?.Invoke(msg);
            });

        try
        {
            await _connection.StartAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SignalR connect error: {ex.Message}");
        }
    }

    public async Task DisconnectAsync()
    {
        if (_connection is null) return;
        try
        {
            await _connection.StopAsync();
            await _connection.DisposeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SignalR disconnect error: {ex.Message}");
        }
        finally
        {
            _connection = null;
        }
    }
}
