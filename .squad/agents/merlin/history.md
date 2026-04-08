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
