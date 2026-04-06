using LanManager.Data;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LanManager.Api.Services;

public class DataSeeder
{
    private readonly LanManagerDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public DataSeeder(LanManagerDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        // --- Users ---
        await EnsureUser("admin@lanmanager.dev", "Admin@1234", "Admin User", "Admin");

        await EnsureUser("organizer1@lanmanager.dev", "Organizer@1234", "Alice Organizer", "Organizer");
        await EnsureUser("organizer2@lanmanager.dev", "Organizer@1234", "Bob Organizer", "Organizer");

        var attendeeNames = new[] {
            ("charlie@lanmanager.dev", "Charlie Andersen"),
            ("diana@lanmanager.dev", "Diana Berg"),
            ("erik@lanmanager.dev", "Erik Dahl"),
            ("fiona@lanmanager.dev", "Fiona Eriksen"),
            ("gunnar@lanmanager.dev", "Gunnar Fjord"),
            ("hilde@lanmanager.dev", "Hilde Grøn"),
            ("ivan@lanmanager.dev", "Ivan Hansen"),
            ("julia@lanmanager.dev", "Julia Iversen"),
            ("kai@lanmanager.dev", "Kai Johansen"),
            ("lena@lanmanager.dev", "Lena Karlsen")
        };
        var attendees = new List<ApplicationUser>();
        foreach (var (email, name) in attendeeNames)
            attendees.Add(await EnsureUser(email, "Attendee@1234", name, "Attendee"));

        await EnsureUser("operator1@lanmanager.dev", "Operator@1234", "Mike Operator", "Operator");
        await EnsureUser("operator2@lanmanager.dev", "Operator@1234", "Nina Operator", "Operator");

        // --- Events ---
        var now = DateTime.UtcNow;

        var draftEvent = await EnsureEvent(
            new Guid("a0000000-0000-0000-0000-000000000001"),
            "LanManager Draft Event", "A draft event not yet published", "Venue A",
            now.AddDays(30), now.AddDays(31), 50, EventStatus.Draft);

        var publishedEvent = await EnsureEvent(
            new Guid("a0000000-0000-0000-0000-000000000002"),
            "Summer LAN 2026", "Open for registration!", "Venue B",
            now.AddDays(14), now.AddDays(15), 30, EventStatus.Published);

        var activeEvent = await EnsureEvent(
            new Guid("a0000000-0000-0000-0000-000000000003"),
            "LanManager Live Event", "Currently running", "Venue C",
            now.AddHours(-2), now.AddHours(22), 20, EventStatus.Active);

        var closedEvent = await EnsureEvent(
            new Guid("a0000000-0000-0000-0000-000000000004"),
            "Winter LAN 2025", "Past event", "Venue D",
            now.AddDays(-60), now.AddDays(-59), 40, EventStatus.Closed);

        // --- Registrations ---
        // Active event: 6 attendees registered
        for (int i = 0; i < 6; i++)
            await EnsureRegistration(activeEvent.Id, attendees[i].Id);

        // Published event: 3 attendees registered
        for (int i = 0; i < 3; i++)
            await EnsureRegistration(publishedEvent.Id, attendees[i].Id);

        // --- Check-ins ---
        // Active event: 4 of the 6 checked in
        for (int i = 0; i < 4; i++)
            await EnsureCheckIn(activeEvent.Id, attendees[i].Id);

        await _db.SaveChangesAsync();

        // --- Door Passes (active event) ---
        await EnsureDoorPasses(activeEvent.Id, attendees, now);

        // --- Seats (active event: 4×5 grid) ---
        await EnsureSeats(activeEvent.Id, attendees, now);

        // --- Tournament (active event: 4-player single elimination) ---
        await EnsureTournament(activeEvent.Id, attendees.Take(4).ToList(), now);

        await _db.SaveChangesAsync();
    }

    private async Task<ApplicationUser> EnsureUser(string email, string password, string name, string role)
    {
        var existing = await _userManager.FindByEmailAsync(email);
        if (existing != null) return existing;

        var user = new ApplicationUser { UserName = email, Email = email, Name = name, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new Exception($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        await _userManager.AddToRoleAsync(user, role);
        return user;
    }

    private async Task<Event> EnsureEvent(Guid id, string name, string description, string location, DateTime start, DateTime end, int capacity, EventStatus status)
    {
        var existing = await _db.Events.FindAsync(id);
        if (existing != null) return existing;

        var ev = new Event
        {
            Id = id, Name = name, Description = description, Location = location,
            StartDate = start, EndDate = end, Capacity = capacity,
            Status = status, CreatedAt = DateTime.UtcNow
        };
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        return ev;
    }

    private async Task EnsureRegistration(Guid eventId, Guid userId)
    {
        var exists = await _db.Registrations.AnyAsync(r => r.EventId == eventId && r.UserId == userId);
        if (exists) return;
        _db.Registrations.Add(new Registration { EventId = eventId, UserId = userId, RegisteredAt = DateTime.UtcNow });
    }

    private async Task EnsureCheckIn(Guid eventId, Guid userId)
    {
        var exists = await _db.CheckInRecords.AnyAsync(c => c.EventId == eventId && c.UserId == userId);
        if (exists) return;
        _db.CheckInRecords.Add(new CheckInRecord { EventId = eventId, UserId = userId, CheckedInAt = DateTime.UtcNow });
    }

    private async Task EnsureDoorPasses(Guid eventId, List<ApplicationUser> attendees, DateTime now)
    {
        if (await _db.DoorPasses.AnyAsync(d => d.EventId == eventId)) return;

        // attendees[0]: exited and returned
        // attendees[1]: currently outside
        // attendees[2]: exited and returned earlier
        // attendees[3]: just stepped out
        _db.DoorPasses.AddRange(
            new DoorPassRecord { EventId = eventId, UserId = attendees[0].Id, Direction = DoorPassDirection.Exit,  ScannedAt = now.AddMinutes(-90) },
            new DoorPassRecord { EventId = eventId, UserId = attendees[0].Id, Direction = DoorPassDirection.Entry, ScannedAt = now.AddMinutes(-60) },
            new DoorPassRecord { EventId = eventId, UserId = attendees[1].Id, Direction = DoorPassDirection.Exit,  ScannedAt = now.AddMinutes(-30) },
            new DoorPassRecord { EventId = eventId, UserId = attendees[2].Id, Direction = DoorPassDirection.Exit,  ScannedAt = now.AddMinutes(-120) },
            new DoorPassRecord { EventId = eventId, UserId = attendees[2].Id, Direction = DoorPassDirection.Entry, ScannedAt = now.AddMinutes(-108) },
            new DoorPassRecord { EventId = eventId, UserId = attendees[3].Id, Direction = DoorPassDirection.Exit,  ScannedAt = now.AddMinutes(-15) }
        );
    }

    private async Task EnsureSeats(Guid eventId, List<ApplicationUser> attendees, DateTime now)
    {
        if (await _db.Seats.AnyAsync(s => s.EventId == eventId)) return;

        var seats = new List<Seat>();
        for (int row = 1; row <= 4; row++)
            for (int col = 1; col <= 5; col++)
                seats.Add(new Seat
                {
                    EventId = eventId,
                    Row = row,
                    Column = col,
                    Label = $"{(char)('A' + row - 1)}{col}"
                });

        // Assign first 4 seats to the checked-in attendees
        for (int i = 0; i < 4; i++)
        {
            seats[i].AssignedUserId   = attendees[i].Id;
            seats[i].AssignedUserName = attendees[i].Name;
            seats[i].AssignedAt       = now.AddHours(-1);
        }

        _db.Seats.AddRange(seats);
    }

    private async Task EnsureTournament(Guid eventId, List<ApplicationUser> participants, DateTime now)
    {
        if (await _db.Tournaments.AnyAsync(t => t.EventId == eventId)) return;

        var tournamentId = new Guid("b0000000-0000-0000-0000-000000000001");

        var tp = participants.Select((u, i) => new TournamentParticipant
        {
            Id           = new Guid($"c000000{i + 1}-0000-0000-0000-000000000001"),
            TournamentId = tournamentId,
            UserId       = u.Id,
            DisplayName  = u.Name,
            Seed         = i + 1
        }).ToList();

        // Round 1, Match 1: [0] vs [1] → [0] wins (Completed)
        // Round 1, Match 2: [2] vs [3] → [2] wins (Completed)
        // Round 2, Match 1 (Final): [0] vs [2] → in progress
        var matches = new List<TournamentMatch>
        {
            new()
            {
                Id           = new Guid("d0000001-0000-0000-0000-000000000001"),
                TournamentId = tournamentId,
                Round        = 1, MatchNumber = 1,
                Player1Id    = participants[0].Id, Player1Name = participants[0].Name,
                Player2Id    = participants[1].Id, Player2Name = participants[1].Name,
                WinnerId     = participants[0].Id, WinnerName  = participants[0].Name,
                Status       = "Completed"
            },
            new()
            {
                Id           = new Guid("d0000002-0000-0000-0000-000000000001"),
                TournamentId = tournamentId,
                Round        = 1, MatchNumber = 2,
                Player1Id    = participants[2].Id, Player1Name = participants[2].Name,
                Player2Id    = participants[3].Id, Player2Name = participants[3].Name,
                WinnerId     = participants[2].Id, WinnerName  = participants[2].Name,
                Status       = "Completed"
            },
            new()
            {
                Id           = new Guid("d0000003-0000-0000-0000-000000000001"),
                TournamentId = tournamentId,
                Round        = 2, MatchNumber = 1,
                Player1Id    = participants[0].Id, Player1Name = participants[0].Name,
                Player2Id    = participants[2].Id, Player2Name = participants[2].Name,
                Status       = "InProgress"
            }
        };

        _db.Tournaments.Add(new Tournament
        {
            Id           = tournamentId,
            EventId      = eventId,
            Name         = "Summer LAN Cup",
            Format       = "SingleElimination",
            Status       = "Active",
            CreatedAt    = now.AddHours(-1),
            Participants = tp,
            Matches      = matches
        });
    }
}
