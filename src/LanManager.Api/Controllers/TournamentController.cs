using LanManager.Api.DTOs;
using LanManager.Api.Hubs;
using LanManager.Api.Services;
using LanManager.Data;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LanManager.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/tournaments")]
[Authorize]
public class TournamentController : ControllerBase
{
    private readonly LanManagerDbContext _db;
    private readonly BracketService _bracketService;
    private readonly IHubContext<TournamentHub> _hub;

    public TournamentController(LanManagerDbContext db, BracketService bracketService, IHubContext<TournamentHub> hub)
    {
        _db = db;
        _bracketService = bracketService;
        _hub = hub;
    }

    [HttpGet]
    public async Task<ActionResult<List<TournamentDto>>> GetAll(Guid eventId)
    {
        var ts = await _db.Tournaments
            .Where(t => t.EventId == eventId)
            .Select(t => new TournamentDto(t.Id, t.EventId, t.Name, t.Format, t.Status, t.CreatedAt, t.Participants.Count))
            .ToListAsync();
        return Ok(ts);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<ActionResult<TournamentDto>> Create(Guid eventId, [FromBody] CreateTournamentRequest request)
    {
        var ev = await _db.Events.FindAsync(eventId);
        if (ev == null) return NotFound("Event not found");

        var tournament = new Tournament { EventId = eventId, Name = request.Name, Status = "Active" };
        _db.Tournaments.Add(tournament);
        await _db.SaveChangesAsync();

        int seed = 1;
        foreach (var p in request.Participants)
            _db.TournamentParticipants.Add(new TournamentParticipant { TournamentId = tournament.Id, UserId = p.UserId, DisplayName = p.DisplayName, Seed = seed++ });
        await _db.SaveChangesAsync();

        var participants = await _db.TournamentParticipants.Where(p => p.TournamentId == tournament.Id).ToListAsync();
        var matches = _bracketService.GenerateSingleElimination(tournament.Id, participants);
        _db.TournamentMatches.AddRange(matches);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBracket), new { eventId, id = tournament.Id },
            new TournamentDto(tournament.Id, tournament.EventId, tournament.Name, tournament.Format, tournament.Status, tournament.CreatedAt, participants.Count));
    }

    [HttpGet("{id:guid}/bracket")]
    public async Task<ActionResult<BracketDto>> GetBracket(Guid eventId, Guid id)
    {
        var tournament = await _db.Tournaments.Include(t => t.Matches).FirstOrDefaultAsync(t => t.Id == id && t.EventId == eventId);
        if (tournament == null) return NotFound();

        int totalRounds = tournament.Matches.Count > 0 ? tournament.Matches.Max(m => m.Round) : 0;
        var rounds = tournament.Matches
            .GroupBy(m => m.Round).OrderBy(g => g.Key)
            .Select(g => new RoundDto(g.Key, RoundName(g.Key, totalRounds),
                g.OrderBy(m => m.MatchNumber)
                 .Select(m => new MatchDto(m.Id, m.Round, m.MatchNumber, m.Player1Id, m.Player1Name, m.Player2Id, m.Player2Name, m.WinnerId, m.WinnerName, m.Status))
                 .ToList()))
            .ToList();

        return Ok(new BracketDto(tournament.Id, tournament.Name, tournament.Status, rounds));
    }

    [HttpPut("{id:guid}/matches/{matchId:guid}/result")]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<ActionResult<MatchDto>> SubmitResult(Guid eventId, Guid id, Guid matchId, [FromBody] SubmitResultRequest request)
    {
        var match = await _db.TournamentMatches.FirstOrDefaultAsync(m => m.Id == matchId && m.TournamentId == id);
        if (match == null) return NotFound();
        if (match.Status is "Completed" or "Bye") return BadRequest("Match already completed.");
        if (match.Player1Id != request.WinnerId && match.Player2Id != request.WinnerId)
            return BadRequest("Winner must be one of the match participants.");

        match.WinnerId = request.WinnerId;
        match.WinnerName = match.Player1Id == request.WinnerId ? match.Player1Name : match.Player2Name;
        match.Status = "Completed";

        var allMatches = await _db.TournamentMatches.Where(m => m.TournamentId == id).ToListAsync();
        _bracketService.AdvanceWinner(allMatches, match);

        var maxRound = allMatches.Max(m => m.Round);
        var finalMatch = allMatches.FirstOrDefault(m => m.Round == maxRound);
        if (finalMatch?.Status is "Completed" or "Bye")
        {
            var t = await _db.Tournaments.FindAsync(id);
            if (t != null) t.Status = "Completed";
        }

        await _db.SaveChangesAsync();

        var dto = new MatchDto(match.Id, match.Round, match.MatchNumber, match.Player1Id, match.Player1Name, match.Player2Id, match.Player2Name, match.WinnerId, match.WinnerName, match.Status);
        await _hub.Clients.Group(id.ToString()).SendAsync("MatchResultUpdated", dto);
        return Ok(dto);
    }

    private static string RoundName(int round, int totalRounds) => (totalRounds - round) switch
    {
        0 => "Final",
        1 => "Semi-Final",
        2 => "Quarter-Final",
        _ => $"Round {round}"
    };

    [HttpPost("/api/tournaments/{tournamentId:guid}/participants/me")]
    [Authorize]
    public async Task<IActionResult> SelfEnrol(Guid tournamentId)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var tournament = await _db.Tournaments.FindAsync(tournamentId);
        if (tournament == null) return NotFound();

        var isCheckedIn = await _db.CheckInRecords
            .AnyAsync(c => c.EventId == tournament.EventId && c.UserId == userId && c.CheckedOutAt == null);
        if (!isCheckedIn)
            return BadRequest(new { message = "You must be checked in to the event to join a tournament" });

        if (tournament.Status != "Active")
            return BadRequest(new { message = "Tournament is not open for registration" });

        var alreadyEnrolled = await _db.TournamentParticipants
            .AnyAsync(p => p.TournamentId == tournamentId && p.UserId == userId);
        if (alreadyEnrolled)
            return Conflict(new { message = "Already enrolled" });

        var participant = new TournamentParticipant
        {
            TournamentId = tournamentId,
            UserId = userId,
            DisplayName = User.Identity?.Name ?? userId.ToString()
        };
        _db.TournamentParticipants.Add(participant);
        await _db.SaveChangesAsync();

        var enrolledAt = DateTime.UtcNow;
        return CreatedAtAction(nameof(GetBracket),
            new { eventId = tournament.EventId, id = tournamentId },
            new TournamentEnrolmentDto(tournamentId, userId, enrolledAt));
    }
}
