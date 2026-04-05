using System.ComponentModel.DataAnnotations;
using LanManager.Data.Models;

namespace LanManager.Api.DTOs;

public record EventDto(
    Guid Id,
    string Name,
    string? Description,
    string? Location,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity,
    string Status,
    DateTime CreatedAt
);

public record CreateEventRequest(
    [Required] string Name,
    string? Description,
    string? Location,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [Range(1, int.MaxValue)] int Capacity,
    EventStatus Status
);

public record UpdateEventRequest(
    [Required] string Name,
    string? Description,
    string? Location,
    [Required] DateTime StartDate,
    [Required] DateTime EndDate,
    [Range(1, int.MaxValue)] int Capacity,
    EventStatus Status
);
