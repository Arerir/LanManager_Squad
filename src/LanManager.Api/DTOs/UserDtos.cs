namespace LanManager.Api.DTOs;

public record UserDto(
    Guid Id,
    string Name,
    string UserName,
    string Email
);

public record CreateUserRequest(
    string Name,
    string UserName,
    string Email,
    string Password
);

public record PagedResult<T>(
    IEnumerable<T> Items,
    int Page,
    int PageSize,
    int TotalCount
);

public record RegisterForEventRequest(Guid UserId);

public record RegistrationDto(
    Guid Id,
    Guid EventId,
    Guid UserId,
    string UserName,
    string Status,
    DateTime RegisteredAt
);
