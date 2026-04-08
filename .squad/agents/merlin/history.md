# Project Context

- **Owner:** Daniel Eli
- **Project:** LanManager_Squad — LAN party management platform
- **Stack:** .NET Aspire orchestration, React frontend, .NET 10 Web API backend, .NET MAUI check-in/check-out apps
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

## PR #123 — MAUI Equipment Borrow eventId & Attendee QR Fix

**Bug 1 — Equipment borrow sent Guid.Empty as event ID:**
- `AttendeeHubViewModel.GoToEquipmentScanAsync()` now navigates with `?eventId={_eventId}`
- `EquipmentScanViewModel` now implements `IQueryAttributable` to receive `eventId` from navigation query
- `ApiService.BorrowEquipmentAsync(string qrCode, Guid eventId)` signature updated; URL now includes `?eventId={eventId}`

**Bug 2 — Attendee QR code returned 404 for checked-in users:**
- Root cause: `DoorPassController.GetQrCode` only checked `db.Registrations`; door-scan auto-check-in creates `CheckInRecord` without a `Registration`
- Fix: combined check — `db.Registrations.AnyAsync(...) || db.CheckInRecords.AnyAsync(...)` so checked-in attendees can retrieve their QR code

**Files changed:** `ApiService.cs`, `EquipmentScanViewModel.cs`, `AttendeeHubViewModel.cs`, `DoorPassController.cs`  
**Build:** API ✅ MAUI ✅ | **Branch:** fix/maui-borrow-eventid-and-qr | **PR:** https://github.com/Arerir/LanManager_Squad/pull/123

## 2026-04-08 16:05:26 - Attendance Display Name Fix

**Task:** Fix AttendanceDto and SeatsController to use display Name instead of email

**Changes Made:**
- Updated AttendanceDto to include Name field alongside UserName
- Modified CheckInController.GetAttendance to project User.Name in the query
- Enhanced SeatsController.Assign to look up user from database and store display name

**Files Modified:**
- src/LanManager.Api/DTOs/CheckInDtos.cs
- src/LanManager.Api/Controllers/CheckInController.cs  
- src/LanManager.Api/Controllers/SeatsController.cs

**Build & Test Results:**
- Build: ✅ Success
- Tests: ✅ All 62 tests passed

**Branch:** fix/attendance-display-name
**PR:** https://github.com/Arerir/LanManager_Squad/pull/121

## PR #124 — Local QR Generation (QRCoder) and Crew Full Role Access (2026-04-09)

**Fix 1 — Local QR code in Attendee app:**
- AttendeeQrViewModel now uses QRCoder locally via Task.Run — no API call, no main thread blocking.
- QR content: userGuid.ToString() (GUID string, matching what the API was generating).
- Exposes QrImageSource (ImageSource?) + IsLoading + StatusMessage; constructor takes only (AuthService, AppStateService).
- AttendeeQrPage.xaml uses <Image Source=QrImageSource> + <ActivityIndicator> with IsNotNullConverter.
- QRCoder 1.* added to LanManager.Maui.csproj; NoWarn CA1416 suppresses System.Drawing platform warning.

**Fix 2 — Crew app full elevated-role access:**
- CheckInViewModel.CanAccessDoorScan set to true in constructor + LoadDataAsync — all crew users see Door Scanner.
- DoorScanViewModel.InitAsync() role check removed — Operators can scan without Access denied redirect.
- Crew app is elevated-only by design; no intra-app role gating needed.

**Build:** MAUI (0 warnings, 0 errors) MAUI.Crew (0 warnings, 0 errors) | **Branch:** fix/local-qr-and-crew-operator-role | **PR:** https://github.com/Arerir/LanManager_Squad/pull/124

## PR #127 — Camera Flip Button on Scanner Pages (2026-04-09)

### Task: Camera toggle for DoorScan (Crew) and EquipmentScan (Attendee)

**Changes Made:**
- Added `using ZXing.Net.Maui;` to both ViewModels
- Added `[ObservableProperty] CameraLocation CameraFacing` defaulting to `CameraLocation.Rear` on both ViewModels
- Added `[RelayCommand] ToggleCamera()` toggling Rear ↔ Front on both ViewModels
- Wrapped `CameraBarcodeReaderView` in a `<Grid>` in both XAML pages with an overlay `Button` (semi-transparent dark pill, top-right corner) bound to `ToggleCameraCommand`
- `CameraLocation` bound on the camera view via `{Binding CameraFacing}`

**Files Modified:**
- `src/LanManager.Maui.Crew/ViewModels/DoorScanViewModel.cs`
- `src/LanManager.Maui/ViewModels/EquipmentScanViewModel.cs`
- `src/LanManager.Maui.Crew/Views/DoorScanPage.xaml`
- `src/LanManager.Maui/Views/EquipmentScanPage.xaml`

**Build Results:** MAUI ✅ (0 warnings, 0 errors) | MAUI.Crew ✅ (0 warnings, 0 errors)

**Branch:** feat/camera-toggle-scanners  
**PR:** https://github.com/Arerir/LanManager_Squad/pull/127



### Task 1 — Backend: Auto-flip door direction after Exit
- In `DoorPassController.DoorScan`, after parsing direction and before `isCheckedIn` check, queries the user's latest door pass. If last pass was Exit, forces `dir = Entry`. Ensures after any exit the next scan is always Entry regardless of what crew app sends.

### Task 2 — Backend: Attendee door status endpoint
- Added `GET /api/events/{eventId}/attendees/{userId}/door-status` returning `{ status: "Entry" | "Exit" | "Unregistered" }`.
- Authorized: staff can query any user; attendees can only query themselves.

### Task 3 — Attendee MAUI: QR page status colors + message
- `ApiService.GetAttendeeDoorStatusAsync()` added to `LanManager.Maui.Shared`.
- `AttendeeQrViewModel` now takes `(ApiService, AuthService, AppStateService)` — fetches door status after QR generation.
- `PageBackground` property drives page color: green (Entry), red (Exit), purple (Unregistered/null).
- `AttendeeQrPage.xaml` binds `BackgroundColor` to `PageBackground`; StatusMessage `TextColor` changed to `White`.

### Task 4 — Crew MAUI: Restrict login to crew roles
- `LoginViewModel.LoginAsync` now checks `CurrentUser.Roles` post-login. If no Admin/Organizer/Operator role found, calls `LogoutAsync()` and shows "Access denied" error message.

**Build:** API ✅ MAUI ✅ MAUI.Crew ✅ (0 warnings, 0 errors each) | **Branch:** feat/door-scan-status-sprint | **PR:** https://github.com/Arerir/LanManager_Squad/pull/126  
**Merged by:** Gandalf (squash, `--admin`) | **CI:** ✅ 4/4 | **Master HEAD:** 344570f  
**Orchestration log:** `.squad/orchestration-log/2026-04-09T10-00-01Z-merlin-sprint-backend-maui.md`  
**Session log:** `.squad/log/2026-04-09T10-00-03Z-sprint-door-scan.md`

## PR #127— Camera Flip Button on DoorScan & EquipmentScan (2026-04-09)

### Task: Add camera orientation toggle (Rear ↔ Front) to scanner pages

**Changes Made:**
- Added `[ObservableProperty] CameraLocation CameraFacing` (default: `Rear`) to both ViewModels
- Added `[RelayCommand] ToggleCamera()` to both ViewModels
- Wrapped `CameraBarcodeReaderView` in `<Grid>` with overlay button on both Pages
- Button styling: Semi-transparent dark background (`#AA000000`), top-right placement, rounded corners, white text

**Files Modified:**
- `src/LanManager.Maui.Crew/ViewModels/DoorScanViewModel.cs` — CameraFacing property, ToggleCamera command
- `src/LanManager.Maui.Crew/Views/DoorScanPage.xaml` — Grid + button overlay
- `src/LanManager.Maui/ViewModels/EquipmentScanViewModel.cs` — CameraFacing property, ToggleCamera command
- `src/LanManager.Maui/Views/EquipmentScanPage.xaml` — Grid + button overlay

**Build Results:**
- MAUI ✅ (0 warnings, 0 errors)
- MAUI.Crew ✅ (0 warnings, 0 errors)
- CI: 4/4 checks passing (API Tests, Build API, Build Frontend, GitGuardian)

**Branch:** feat/camera-toggle-scanners  
**PR:** https://github.com/Arerir/LanManager_Squad/pull/127  
**Merged:** Yes (squash merge via `--admin`)

**Pattern Established:** ZXing.Net.MAUI's `CameraLocation` is a bindable property — can toggle camera orientation purely via MVVM binding without code-behind. Grid overlay is idiomatic MAUI approach for floating UI over camera view.

