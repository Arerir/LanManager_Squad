using LanManager.Api.DTOs;
using LanManager.Data;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LanManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController(LanManagerDbContext db) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<EventDto>>> GetAll(
        [FromQuery] EventStatus? status,
        [FromQuery] string? sort)
    {
        var query = db.Events.AsQueryable();

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        query = sort?.ToLowerInvariant() switch
        {
            "name" => query.OrderBy(e => e.Name),
            _ => query.OrderBy(e => e.StartDate)
        };

        var events = await query.Select(e => ToDto(e)).ToListAsync();
        return Ok(events);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<EventDto>> GetById(Guid id)
    {
        var ev = await db.Events.FindAsync(id);
        if (ev is null) return NotFound();
        return Ok(ToDto(ev));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<ActionResult<EventDto>> Create([FromBody] CreateEventRequest request)
    {
        var ev = new Event
        {
            Name = request.Name,
            Description = request.Description,
            Location = request.Location,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Capacity = request.Capacity,
            Status = request.Status
        };

        db.Events.Add(ev);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.Id }, ToDto(ev));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<ActionResult<EventDto>> Update(Guid id, [FromBody] UpdateEventRequest request)
    {
        var ev = await db.Events.FindAsync(id);
        if (ev is null) return NotFound();

        ev.Name = request.Name;
        ev.Description = request.Description;
        ev.Location = request.Location;
        ev.StartDate = request.StartDate;
        ev.EndDate = request.EndDate;
        ev.Capacity = request.Capacity;
        ev.Status = request.Status;

        await db.SaveChangesAsync();
        return Ok(ToDto(ev));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ev = await db.Events.FindAsync(id);
        if (ev is null) return NotFound();

        db.Events.Remove(ev);
        await db.SaveChangesAsync();
        return NoContent();
    }

    private static EventDto ToDto(Event e) => new(
        e.Id, e.Name, e.Description, e.Location,
        e.StartDate, e.EndDate, e.Capacity,
        e.Status.ToString(), e.CreatedAt);
}
