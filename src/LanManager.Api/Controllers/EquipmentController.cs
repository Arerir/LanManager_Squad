using LanManager.Data;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LanManager.Api.Controllers;

public record EquipmentDto(Guid Id, string Name, string Type, string QrCode, string? Notes, bool IsAvailable, EquipmentLoanDto? ActiveLoan);
public record EquipmentLoanDto(Guid Id, Guid EquipmentId, string EquipmentName, Guid UserId, string UserName, Guid EventId, DateTime BorrowedAt, DateTime? ReturnedAt);
public record CreateEquipmentRequest(string Name, string Type, string QrCode, string? Notes);
public record UpdateEquipmentRequest(string Name, string Type, string? Notes);

[ApiController]
[Route("api/equipment")]
public class EquipmentController(LanManagerDbContext db) : ControllerBase
{
    [HttpGet("my-loans")]
    [Authorize]
    public async Task<ActionResult<List<EquipmentLoanDto>>> GetMyLoans()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var loans = await db.EquipmentLoans
            .Where(l => l.UserId == userId && l.ReturnedAt == null)
            .Include(l => l.Equipment)
            .Include(l => l.User)
            .Select(l => new EquipmentLoanDto(l.Id, l.EquipmentId, l.Equipment.Name, l.UserId, l.User.UserName ?? string.Empty, l.EventId, l.BorrowedAt, l.ReturnedAt))
            .ToListAsync();

        return Ok(loans);
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Organizer,Operator")]
    public async Task<ActionResult<List<EquipmentDto>>> GetAll()
    {
        var items = await db.Equipment
            .Include(e => e.Loans.Where(l => l.ReturnedAt == null))
            .ThenInclude(l => l.User)
            .ToListAsync();

        var result = items.Select(e =>
        {
            var activeLoan = e.Loans.FirstOrDefault(l => l.ReturnedAt == null);
            var loanDto = activeLoan == null ? null : new EquipmentLoanDto(activeLoan.Id, activeLoan.EquipmentId, e.Name, activeLoan.UserId, activeLoan.User.UserName ?? string.Empty, activeLoan.EventId, activeLoan.BorrowedAt, activeLoan.ReturnedAt);
            return new EquipmentDto(e.Id, e.Name, e.Type.ToString(), e.QrCode, e.Notes, activeLoan == null, loanDto);
        }).ToList();

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<EquipmentDto>> GetById(Guid id)
    {
        var equipment = await db.Equipment
            .Include(e => e.Loans.Where(l => l.ReturnedAt == null))
            .ThenInclude(l => l.User)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (equipment == null) return NotFound();

        var activeLoan = equipment.Loans.FirstOrDefault(l => l.ReturnedAt == null);
        var loanDto = activeLoan == null ? null : new EquipmentLoanDto(activeLoan.Id, activeLoan.EquipmentId, equipment.Name, activeLoan.UserId, activeLoan.User.UserName ?? string.Empty, activeLoan.EventId, activeLoan.BorrowedAt, activeLoan.ReturnedAt);
        return Ok(new EquipmentDto(equipment.Id, equipment.Name, equipment.Type.ToString(), equipment.QrCode, equipment.Notes, activeLoan == null, loanDto));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EquipmentDto>> Create([FromBody] CreateEquipmentRequest request)
    {
        if (!Enum.TryParse<EquipmentType>(request.Type, out var type))
            return BadRequest(new { message = $"Invalid equipment type '{request.Type}'." });

        var equipment = new Equipment
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Type = type,
            QrCode = request.QrCode,
            Notes = request.Notes
        };

        db.Equipment.Add(equipment);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = equipment.Id },
            new EquipmentDto(equipment.Id, equipment.Name, equipment.Type.ToString(), equipment.QrCode, equipment.Notes, true, null));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EquipmentDto>> Update(Guid id, [FromBody] UpdateEquipmentRequest request)
    {
        var equipment = await db.Equipment.FindAsync(id);
        if (equipment == null) return NotFound();

        if (!Enum.TryParse<EquipmentType>(request.Type, out var type))
            return BadRequest(new { message = $"Invalid equipment type '{request.Type}'." });

        equipment.Name = request.Name;
        equipment.Type = type;
        equipment.Notes = request.Notes;
        await db.SaveChangesAsync();

        return Ok(new EquipmentDto(equipment.Id, equipment.Name, equipment.Type.ToString(), equipment.QrCode, equipment.Notes, true, null));
    }

    [HttpPost("borrow/{qrCode}")]
    [Authorize]
    public async Task<ActionResult<EquipmentLoanDto>> Borrow(string qrCode, [FromQuery] Guid eventId)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        var equipment = await db.Equipment.FirstOrDefaultAsync(e => e.QrCode == qrCode);
        if (equipment == null) return NotFound(new { message = "Equipment not found." });

        var activeLoan = await db.EquipmentLoans
            .AnyAsync(l => l.EquipmentId == equipment.Id && l.ReturnedAt == null);
        if (activeLoan)
            return Conflict(new { message = "Equipment is already borrowed." });

        var user = await db.Users.FindAsync(userId);
        if (user == null) return Unauthorized();

        var loan = new EquipmentLoan
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            UserId = userId,
            EventId = eventId,
            BorrowedAt = DateTime.UtcNow
        };

        db.EquipmentLoans.Add(loan);
        await db.SaveChangesAsync();

        var dto = new EquipmentLoanDto(loan.Id, loan.EquipmentId, equipment.Name, loan.UserId, user.UserName ?? string.Empty, loan.EventId, loan.BorrowedAt, loan.ReturnedAt);
        return StatusCode(201, dto);
    }

    [HttpPost("{id:guid}/return")]
    [Authorize(Roles = "Admin,Organizer")]
    public async Task<ActionResult<EquipmentLoanDto>> Return(Guid id)
    {
        var equipment = await db.Equipment.FindAsync(id);
        if (equipment == null) return NotFound();

        var loan = await db.EquipmentLoans
            .Include(l => l.User)
            .Where(l => l.EquipmentId == id && l.ReturnedAt == null)
            .OrderByDescending(l => l.BorrowedAt)
            .FirstOrDefaultAsync();

        if (loan == null) return NotFound(new { message = "No active loan found for this equipment." });

        loan.ReturnedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Ok(new EquipmentLoanDto(loan.Id, loan.EquipmentId, equipment.Name, loan.UserId, loan.User.UserName ?? string.Empty, loan.EventId, loan.BorrowedAt, loan.ReturnedAt));
    }
}
