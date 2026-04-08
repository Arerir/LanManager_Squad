# Project Context

- **Owner:** Daniel Eli
- **Project:** LanManager_Squad — LAN party management platform
- **Stack:** .NET Aspire orchestration, React frontend, .NET 9 Web API backend, .NET MAUI check-in/check-out apps
- **Created:** 2026-04-05

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

## Event Update JSON Deserialization Fix — squad/fix-event-update-dto
Root cause: `UpdateEventRequest.Status` (and `CreateEventRequest.Status`) are typed as `EventStatus` (a C# enum). By default, `System.Text.Json` deserializes enums as integers, but the frontend sends string values (`"Active"`, `"Draft"`, etc.). Fix: registered `JsonStringEnumConverter` globally via `AddJsonOptions` in `Program.cs`. All 62 tests pass.

**Pattern to remember:** Any C# enum in a DTO that is received from the React frontend needs `JsonStringEnumConverter` registered (globally or per-property), because the frontend always serialises enums as strings.

## PDF Download Bug (2026-04-08) — PR #112
Root cause: `downloadEventReport()` in `frontend/src/api/events.ts` used a relative URL (`/api/events/{id}/report`) while all other API functions use `${config.apiUrl}/api/...` (absolute). In development, Vite has no `/api` proxy, so the request hit the Vite server and returned the React SPA `index.html` (~630 bytes) with 200 OK. This HTML was saved as a `.pdf`, producing an invalid file. Fix: use absolute URL with `config.apiUrl` and `getToken()` helper. Strengthened test assertion to `bytes.Length > 1024`.

**Pattern to remember:** Any new `fetch()` call in `frontend/src/api/` MUST use `${config.apiUrl}/api/...` (absolute), not a bare `/api/...` relative path. The Vite dev server has no proxy for `/api`.

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

## PR #82 — Door Scan Broadcast Test Fixes (2026-04-08)
Fixed failing tests in PR #82 (feat/79-api-doorscan-broadcast). The DoorPassController constructor was updated to include a third parameter `IHubContext<AttendanceHub>` for SignalR broadcasting, but the tests were not updated. Added `MockHubContext()` helper method in DoorPassControllerTests that creates a properly mocked `IHubContext<AttendanceHub>` with chained `Clients.All` setup. Updated all three test methods to pass the mocked hub context to the controller constructor. Tests now build and pass locally (77/77 tests passing).

## Issue #100 — EventReportService (2026-04-08)
Implemented `ReportSections` [Flags] enum, `EventReportData` DTO (with `RegistrationSummary` and `CheckInSummary` nested types), and `EventReportService` with conditional EF Core `Include()` per section. `CheckInSummary.Duration` is computed in C# from `CheckedInAt`/`CheckedOutAt` diff after the EF query. Registered as scoped in `Program.cs`. **Ambiguity note:** `LanManager.Api.Models` duplicates some entity types from `LanManager.Data.Models` — DTOs and service must fully qualify enum references (e.g., `Data.Models.EventStatus`, `Data.Models.RegistrationStatus`) to avoid CS0104/CS0266 compile errors. PR #106 opened against master.

## Issue #102 — ReportController GET /api/events/{id}/report (2026-04-08)
Created `ReportController` at `src/LanManager.Api/Controllers/ReportController.cs`. Route: `GET /api/events/{eventId:guid}/report`. Auth: `[Authorize(Roles = "Admin,Organizer")]`. Query param: `sections` (comma-separated `ReportSections` flags, default `All`). Returns 400 for unknown section tokens, 404 if event missing, 422 if event not `Closed`, 200+PDF otherwise. Filename: `{EventName-hyphenated}-report.pdf`. Must fully qualify `LanManager.Data.Models.EventStatus` to avoid CS0104 ambiguity with `LanManager.Api.Models`. Build clean. PR #108 opened against squad/101-report-pdf.

## Issue #101 — EventReportPdfGenerator (2026-04-08)
Added QuestPDF 2026.2.4 to LanManager.Api.csproj. Set `QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community` before `builder.Build()` in Program.cs. Created `EventReportPdfGenerator` at `src/LanManager.Api/Services/EventReportPdfGenerator.cs` with `GeneratePdf(EventReportData)` returning `byte[]`. Conditional sections: Registrations table (Name, Status, Registered At), Check-Ins table (Name, Checked In, Checked Out, Duration as h:mm or "Still inside"), Equipment placeholder, Tournaments placeholder. Tables use alternating white/grey row backgrounds with dark header rows. Registered as scoped in Program.cs. PR #107 opened against squad/100-report-service. Build verified clean.

**QuestPDF patterns used:**
- `Document.Create(...).GeneratePdf()` → `byte[]`
- `page.Header().Element(Action<IContainer>)` for header composition
- `Table` + `ColumnsDefinition` + `RelativeColumn` for proportional columns
- `table.Header(header => {...})` for sticky header rows
- `.Background(Colors.Grey.Darken2)` / `.Background(Colors.Grey.Lighten3)` for row coloring
- Duration formatting: `$"{(int)ts.TotalHours}:{ts.Minutes:D2}"` from `TimeSpan`
