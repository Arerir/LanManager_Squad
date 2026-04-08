# Project Context

- **Owner:** Daniel Eli
- **Project:** LanManager_Squad — LAN party management platform
- **Stack:** .NET Aspire orchestration, React frontend, .NET 9 Web API backend, .NET MAUI check-in/check-out apps
- **Created:** 2026-04-05

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

## Issue #2 — EF Core data models (2026-04-05)
Created Event, User, Registration, CheckInRecord entities. LanManagerDbContext in src/LanManager.Api/Data/. SQLite dev DB. Unique index on Registration(EventId, UserId). InitialCreate migration created. PR opened.

## Sprint 2026-04-08 — Fixes & Architecture
Fixed DoorPassController tests for SignalR broadcast feature (#82). Updated DoorPassControllerTests.cs to mock IHubContext<AttendanceHub>. All 77 unit tests passing. Enables door scan broadcast to real-time clients via AttendanceHub.
## PR #82 — Door Scan Broadcast Test Fixes (2026-04-08)
Fixed failing tests in PR #82 (feat/79-api-doorscan-broadcast). The DoorPassController constructor was updated to include a third parameter `IHubContext<AttendanceHub>` for SignalR broadcasting, but the tests were not updated. Added `MockHubContext()` helper method in DoorPassControllerTests that creates a properly mocked `IHubContext<AttendanceHub>` with chained `Clients.All` setup. Updated all three test methods to pass the mocked hub context to the controller constructor. Tests now build and pass locally (77/77 tests passing).

## Door Scan SignalR Broadcast (2026-04-07)
Added `DoorScanBroadcast` record to `AttendanceBroadcast.cs` and wired `IHubContext<AttendanceHub>` into `DoorPassController` (primary constructor injection, matching CheckInController pattern). After `db.SaveChangesAsync()` in `DoorScan`, broadcasts `"UserDoorScanned"` to all connected SignalR clients. Build verified clean. Decisions inbox written at `.squad/decisions/inbox/merlin-doorscan-signalr.md` for Circe/Switch to implement MAUI listener.

**Broadcast Record:**
```csharp
public record DoorScanBroadcast(
    Guid EventId,
    Guid UserId,
    string UserName,
    string Direction,   // "Entry" or "Exit"
    DateTime ScannedAt
);
```

**Event Name:** `UserDoorScanned` on `/hubs/attendance`

📌 Team update (2026-04-07T15-26-09): Morgana implemented EventContext for nav persistence; Circe wired MAUI listener with JWT auth and auto-clearing notifications — decided by Tank, Morgana, Circe

