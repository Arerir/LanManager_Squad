namespace LanManager.Api.Hubs;

public record AttendanceBroadcast(Guid EventId, Guid UserId, string UserName, DateTime CheckedInAt);
public record CheckOutBroadcast(Guid EventId, Guid UserId, string UserName, DateTime CheckedOutAt);
public record DoorScanBroadcast(Guid EventId, Guid UserId, string UserName, string Direction, DateTime ScannedAt);
