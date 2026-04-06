namespace LanManager.Api.DTOs;

public record DoorScanRequest(Guid UserId, string Direction); // "Exit" or "Entry"

public record DoorPassDto(
    Guid Id,
    Guid EventId,
    Guid UserId,
    string UserName,
    string Direction,
    DateTime ScannedAt
);

public record DoorScanResultDto(
    DoorPassDto DoorPass,
    bool WasAutoCheckedIn,
    string UserName
);

public record OutsideUserDto(
    Guid UserId,
    string UserName,
    DateTime ExitedAt
);
