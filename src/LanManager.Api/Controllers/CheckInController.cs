using LanManager.Api.DTOs;
using LanManager.Api.Hubs;
using LanManager.Data;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LanManager.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}")]
public class CheckInController(
    LanManagerDbContext db,
    UserManager<ApplicationUser> userManager,
    IHubContext<AttendanceHub> hubContext) : ControllerBase
{
    [HttpPost("checkin")]
    public async Task<ActionResult<CheckInDto>> CheckIn(Guid eventId, [FromBody] CheckInRequest request)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return NotFound(new { message = "User not found." });

        var isRegistered = await db.Registrations
            .AnyAsync(r => r.EventId == eventId && r.UserId == request.UserId
                      && r.Status == RegistrationStatus.Confirmed);
        if (!isRegistered)
            return BadRequest(new { message = "User is not registered for this event." });

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

        await hubContext.Clients.All.SendAsync("UserCheckedIn",
            new AttendanceBroadcast(eventId, record.UserId, user.UserName ?? "", record.CheckedInAt));

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

        await hubContext.Clients.All.SendAsync("UserCheckedOut",
            new CheckOutBroadcast(eventId, record.UserId, record.User.UserName ?? "", record.CheckedOutAt!.Value));

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
