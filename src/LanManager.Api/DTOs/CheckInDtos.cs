namespace LanManager.Api.DTOs;

public record CheckInRequest(Guid UserId);

public record CheckInDto(
    Guid Id,
    Guid EventId,
    Guid UserId,
    string UserName,
    DateTime CheckedInAt,
    DateTime? CheckedOutAt
);

public record AttendanceDto(
    Guid UserId,
    string UserName,
    string Name,
    DateTime CheckedInAt
);
