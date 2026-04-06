using LanManager.Api.DTOs;
using LanManager.Data;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LanManager.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:guid}/seats")]
[Authorize]
public class SeatsController : ControllerBase
{
    private readonly LanManagerDbContext _db;
    public SeatsController(LanManagerDbContext db) { _db = db; }

    [HttpGet("my-seat")]
    public async Task<ActionResult<SeatDto>> GetMySeat(Guid eventId)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var seat = await _db.Seats.FirstOrDefaultAsync(s => s.EventId == eventId && s.AssignedUserId == userId);
        if (seat == null) return NotFound();

        return Ok(new SeatDto(seat.Id, seat.EventId, seat.Row, seat.Column, seat.Label, seat.AssignedUserId, seat.AssignedUserName, seat.AssignedAt));
    }

    [HttpGet]
    public async Task<ActionResult<List<SeatDto>>> GetAll(Guid eventId)
    {
        var seats = await _db.Seats
            .Where(s => s.EventId == eventId)
            .OrderBy(s => s.Row).ThenBy(s => s.Column)
            .Select(s => new SeatDto(s.Id, s.EventId, s.Row, s.Column, s.Label, s.AssignedUserId, s.AssignedUserName, s.AssignedAt))
            .ToListAsync();
        return Ok(seats);
    }

    [HttpPost("grid")]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<ActionResult<List<SeatDto>>> CreateGrid(Guid eventId, [FromBody] CreateSeatsGridRequest request)
    {
        if (request.Rows < 1 || request.Rows > 26 || request.Columns < 1 || request.Columns > 50)
            return BadRequest("Rows must be 1-26, columns 1-50.");

        var existing = await _db.Seats.Where(s => s.EventId == eventId).ToListAsync();
        _db.Seats.RemoveRange(existing);

        var seats = new List<Seat>();
        for (int row = 0; row < request.Rows; row++)
        {
            char rowLetter = (char)('A' + row);
            for (int col = 0; col < request.Columns; col++)
                seats.Add(new Seat { EventId = eventId, Row = row, Column = col, Label = $"{rowLetter}{col + 1}" });
        }

        _db.Seats.AddRange(seats);
        await _db.SaveChangesAsync();
        return Ok(seats.Select(s => new SeatDto(s.Id, s.EventId, s.Row, s.Column, s.Label, null, null, null)).ToList());
    }

    [HttpPut("{seatId:guid}/assign")]
    [Authorize(Roles = "Admin,Organizer,Operator")]
    public async Task<ActionResult<SeatDto>> Assign(Guid eventId, Guid seatId, [FromBody] AssignSeatRequest request)
    {
        var seat = await _db.Seats.FirstOrDefaultAsync(s => s.Id == seatId && s.EventId == eventId);
        if (seat == null) return NotFound();
        if (seat.AssignedUserId != null && seat.AssignedUserId != request.UserId)
            return Conflict(new { message = $"Seat {seat.Label} is already assigned to {seat.AssignedUserName}." });

        var prev = await _db.Seats.FirstOrDefaultAsync(s => s.EventId == eventId && s.AssignedUserId == request.UserId);
        if (prev != null) { prev.AssignedUserId = null; prev.AssignedUserName = null; prev.AssignedAt = null; }

        seat.AssignedUserId = request.UserId;
        seat.AssignedUserName = request.UserName;
        seat.AssignedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new SeatDto(seat.Id, seat.EventId, seat.Row, seat.Column, seat.Label, seat.AssignedUserId, seat.AssignedUserName, seat.AssignedAt));
    }

    [HttpDelete("{seatId:guid}/assign")]
    [Authorize(Roles = "Admin,Organizer,Operator")]
    public async Task<IActionResult> Unassign(Guid eventId, Guid seatId)
    {
        var seat = await _db.Seats.FirstOrDefaultAsync(s => s.Id == seatId && s.EventId == eventId);
        if (seat == null) return NotFound();
        seat.AssignedUserId = null; seat.AssignedUserName = null; seat.AssignedAt = null;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
