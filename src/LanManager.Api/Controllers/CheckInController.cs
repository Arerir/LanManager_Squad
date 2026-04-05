using LanManager.Api.DTOs;
using LanManager.Data;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LanManager.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}")]
public class CheckInController(LanManagerDbContext db, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost("checkin")]
    public async Task<ActionResult<CheckInDto>> CheckIn(Guid eventId, [FromBody] CheckInRequest request)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return NotFound(new { message = "User not found." });

        var active = await db.CheckInRecords
            .FirstOrDefaultAsync(c => c.EventId == eventId && c.UserId == request.UserId && c.CheckedOutAt == null);
        if (active is not null)
            return Conflict(new { message = "User is already checked in to this event." });

        var record = new CheckInRecord
        {
            EventId = eventId,
            UserId = request.UserId
        };

        db.CheckInRecords.Add(record);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAttendance), new { eventId },
            ToDto(record, user.UserName ?? string.Empty));
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckInDto>> CheckOut(Guid eventId, [FromBody] CheckInRequest request)
    {
        var record = await db.CheckInRecords
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.EventId == eventId && c.UserId == request.UserId && c.CheckedOutAt == null);

        if (record is null)
            return NotFound(new { message = "No active check-in found for this user at this event." });

        record.CheckedOutAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok(ToDto(record, record.User.UserName ?? string.Empty));
    }

    [HttpGet("attendance")]
    public async Task<ActionResult<IEnumerable<AttendanceDto>>> GetAttendance(Guid eventId)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();

        var records = await db.CheckInRecords
            .Where(c => c.EventId == eventId && c.CheckedOutAt == null)
            .Include(c => c.User)
            .Select(c => new AttendanceDto(c.UserId, c.User.UserName ?? string.Empty, c.CheckedInAt))
            .ToListAsync();

        return Ok(records);
    }

    private static CheckInDto ToDto(CheckInRecord r, string userName) =>
        new(r.Id, r.EventId, r.UserId, userName, r.CheckedInAt, r.CheckedOutAt);
}
