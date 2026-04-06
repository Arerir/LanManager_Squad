using LanManager.Api.DTOs;
using LanManager.Data;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using System.Security.Claims;

namespace LanManager.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}")]
[Authorize(Roles = "Admin,Organizer,Operator")]
public class DoorPassController(LanManagerDbContext db, UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("attendees/{userId:guid}/qrcode")]
    [Authorize]
    public async Task<IActionResult> GetQrCode(Guid eventId, Guid userId)
    {
        var callerIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var callerIsStaff = User.IsInRole("Admin") || User.IsInRole("Organizer") || User.IsInRole("Operator");

        if (!callerIsStaff)
        {
            if (!Guid.TryParse(callerIdStr, out var callerId) || callerId != userId)
                return Forbid();
        }

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
    public async Task<ActionResult<DoorScanResultDto>> DoorScan(Guid eventId, [FromBody] DoorScanRequest request)
    {
        var ev = await db.Events.FindAsync(eventId);
        if (ev is null) return NotFound(new { message = "Event not found." });

        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null) return NotFound(new { message = "User not found." });

        if (!Enum.TryParse<DoorPassDirection>(request.Direction, out var dir))
            return BadRequest(new { message = $"Invalid direction '{request.Direction}'. Use 'Exit' or 'Entry'." });

        var isCheckedIn = await db.CheckInRecords
            .AnyAsync(c => c.EventId == eventId && c.UserId == request.UserId && c.CheckedOutAt == null);

        bool wasAutoCheckedIn = false;
        if (!isCheckedIn)
        {
            // Auto check-in: create a CheckInRecord and force Entry direction
            db.CheckInRecords.Add(new CheckInRecord
            {
                EventId = eventId,
                UserId = request.UserId
            });
            dir = DoorPassDirection.Entry;
            wasAutoCheckedIn = true;
        }

        var record = new DoorPassRecord
        {
            EventId = eventId,
            UserId = request.UserId,
            Direction = dir
        };

        db.DoorPasses.Add(record);
        await db.SaveChangesAsync();

        var passDto = new DoorPassDto(record.Id, record.EventId, record.UserId,
            user.UserName ?? string.Empty, record.Direction.ToString(), record.ScannedAt);

        var result = new DoorScanResultDto(passDto, wasAutoCheckedIn, user.UserName ?? string.Empty);
        return CreatedAtAction(nameof(GetDoorLog), new { eventId }, result);
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

        var passes = await db.DoorPasses
            .Where(d => d.EventId == eventId)
            .Include(d => d.User)
            .OrderByDescending(d => d.ScannedAt)
            .ToListAsync();

        var outside = passes
            .GroupBy(d => d.UserId)
            .Select(g => g.First())                          // latest pass per user (already desc ordered)
            .Where(d => d.Direction == DoorPassDirection.Exit)
            .Select(d => new OutsideUserDto(d.UserId, d.User.UserName ?? string.Empty, d.ScannedAt))
            .ToList();

        return Ok(outside);
    }
}