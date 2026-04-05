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
}
