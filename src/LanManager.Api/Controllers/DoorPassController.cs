using LanManager.Api.DTOs;
using LanManager.Data;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;

namespace LanManager.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}")]
public class DoorPassController(LanManagerDbContext db, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("attendees/{userId:guid}/qrcode")]
    public async Task<IActionResult> GetQrCode(Guid eventId, Guid userId)
    {
        var isRegistered = await db.Registrations
            .AnyAsync(r => r.EventId == eventId && r.UserId == userId);
        if (!isRegistered)
            return NotFound(new { message = "User is not registered for this event." });

        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(userId.ToString(), QRCodeGenerator.ECCLevel.M);
        var qrCode = new PngByteQRCode(qrData);
        var pngBytes = qrCode.GetGraphic(10);
        return File(pngBytes, "image/png");
    }

    [HttpPost("door-scan")]
    public async Task<ActionResult<DoorPassDto>> DoorScan(Guid eventId, [FromBody] DoorScanRequest request)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return NotFound(new { message = "User not found." });

        var isCheckedIn = await db.CheckInRecords
            .AnyAsync(c => c.EventId == eventId && c.UserId == request.UserId && c.CheckedOutAt == null);
        if (!isCheckedIn)
            return BadRequest(new { message = "User is not currently checked in to this event." });

        if (!Enum.TryParse<DoorPassDirection>(request.Direction, out var dir))
            return BadRequest(new { message = $"Invalid direction '{request.Direction}'. Use 'Exit' or 'Entry'." });

        var record = new DoorPassRecord
        {
            EventId = eventId,
            UserId = request.UserId,
            Direction = dir
        };

        db.DoorPasses.Add(record);
        await db.SaveChangesAsync();

        var dto = new DoorPassDto(record.Id, record.EventId, record.UserId,
            user.UserName ?? string.Empty, record.Direction.ToString(), record.ScannedAt);

        return CreatedAtAction(nameof(GetDoorLog), new { eventId }, dto);
    }

    [HttpGet("door-log")]
    public async Task<ActionResult<IEnumerable<DoorPassDto>>> GetDoorLog(Guid eventId)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });

        var records = await db.DoorPasses
            .Where(d => d.EventId == eventId)
            .Include(d => d.User)
            .OrderByDescending(d => d.ScannedAt)
            .Select(d => new DoorPassDto(d.Id, d.EventId, d.UserId,
                d.User.UserName ?? string.Empty, d.Direction.ToString(), d.ScannedAt))
            .ToListAsync();

        return Ok(records);
    }

    [HttpGet("outside")]
    public async Task<ActionResult<IEnumerable<OutsideUserDto>>> GetOutside(Guid eventId)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });

        var outside = await (
            from d in db.DoorPasses
            where d.EventId == eventId
            group d by d.UserId into g
            let latest = g.OrderByDescending(x => x.ScannedAt).First()
            where latest.Direction == DoorPassDirection.Exit
            join u in db.Users on latest.UserId equals u.Id
            select new OutsideUserDto(latest.UserId, u.UserName ?? string.Empty, latest.ScannedAt)
        ).ToListAsync();

        return Ok(outside);
    }
}
