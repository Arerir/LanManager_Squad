using LanManager.Api.DTOs;
using LanManager.Data;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LanManager.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}")]
public class RegistrationsController(LanManagerDbContext db, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost("register")]
    [Authorize(Roles = "Attendee,Admin")]
    public async Task<ActionResult<RegistrationDto>> Register(Guid eventId, [FromBody] RegisterForEventRequest request)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return NotFound(new { message = "User not found." });

        var existing = await db.Registrations
            .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == request.UserId);
        if (existing is not null)
            return Conflict(new { message = "User is already registered for this event." });

        var registeredCount = await db.Registrations
            .CountAsync(r => r.EventId == eventId && r.Status == RegistrationStatus.Confirmed);
        if (registeredCount >= ev.Capacity)
            return Conflict(new { message = "Event is at full capacity." });

        var registration = new Registration
        {
            EventId = eventId,
            UserId = request.UserId
        };

        db.Registrations.Add(registration);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAttendees), new { eventId },
            ToDto(registration, user.UserName ?? string.Empty));
    }

    [HttpGet("attendees")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<RegistrationDto>>> GetAttendees(Guid eventId)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();

        var registrations = await db.Registrations
            .Where(r => r.EventId == eventId)
            .Include(r => r.User)
            .Select(r => ToDto(r, r.User.UserName ?? string.Empty))
            .ToListAsync();

        return Ok(registrations);
    }

    private static RegistrationDto ToDto(Registration r, string userName) =>
        new(r.Id, r.EventId, r.UserId, userName, r.Status.ToString(), r.RegisteredAt);
}