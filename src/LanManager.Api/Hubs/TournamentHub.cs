using Microsoft.AspNetCore.SignalR;

namespace LanManager.Api.Hubs;

public class TournamentHub : Hub
{
    public async Task JoinTournament(string tournamentId)
        => await Groups.AddToGroupAsync(Context.ConnectionId, tournamentId);

    public async Task LeaveTournament(string tournamentId)
        => await Groups.RemoveFromGroupAsync(Context.ConnectionId, tournamentId);
}
