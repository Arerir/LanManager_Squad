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
    string Name,
    string? Description,
    string? Location,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity,
    EventStatus Status
);

public record UpdateEventRequest(
    string Name,
    string? Description,
    string? Location,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity,
    EventStatus Status
);
