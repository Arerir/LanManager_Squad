namespace LanManager.Api.DTOs;

public record SeatDto(Guid Id, Guid EventId, int Row, int Column, string Label, Guid? AssignedUserId, string? AssignedUserName, DateTime? AssignedAt);
public record CreateSeatsGridRequest(int Rows, int Columns);
public record AssignSeatRequest(Guid UserId, string UserName);
