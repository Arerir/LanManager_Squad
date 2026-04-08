# Project Context

- **Owner:** Daniel Eli
- **Project:** LanManager_Squad — LAN party management platform
- **Stack:** .NET Aspire orchestration, React frontend, .NET 9 Web API backend, .NET MAUI check-in/check-out apps
- **Created:** 2026-04-05

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->
- Crew app logins must validate Admin/Organizer roles and logout non-staff users.
- Crew app uses shared admin pages (Login/Main/CheckIn/Attendance/DoorScan) wired via CrewAppShell and AppStateService for event context.
- Crew app shell keeps login and event selection outside operator tabs, with check-in and attendance in a tab bar and DoorScan registered as a modal route.
- LanManager.Maui is attendee-only; admin pages (CheckIn/Attendance/DoorScan) live in LanManager.Maui.Crew.
- LanManager.Maui AppShell registers attendee routes (Login, Main, AttendeeHub, AttendeeQr, EquipmentScan) only.

- LanManager.Maui now always routes event selection to AttendeeHubPage; admin/organizer flows live in LanManager.Maui.Crew.

- Created LanManager.Maui.Shared to host AuthService, AuthHandler, ApiService, and Config for reuse across MAUI apps.
- LanManager.Maui now references the shared library; AppStateService remains app-specific.

- Added `Microsoft.AspNetCore.SignalR.Client` (version `10.*`) to `LanManager.Maui.csproj`.
- `SignalRService` lives in `Services/` and is registered as a **singleton** (one connection per app session).
- Hub URL is `{Config.ApiBaseUrl}/hubs/attendance`; JWT passed via `AccessTokenProvider`.
- Events filtered by both `eventId` AND `userId == CurrentUser.Id` to avoid showing other attendees' notifications.
- `AttendeeHubViewModel` now receives `SignalRService` via constructor injection; `ConnectAsync` is called inside `LoadAsync` alongside the seat/tournament tasks.
- `CleanupAsync()` is called from `AttendeeHubPage.OnDisappearing` (code-behind only — no business logic there, just lifecycle wiring).
- `ShowNotification` follows the same fire-and-forget `async void` + `Task.Delay` auto-clear pattern used in `DoorScanViewModel`.
- `UserDoorScanned` is a new hub event being added by Tank/Merlin; the SignalRService handler is ready for it (`"Exit"` → `→ You went outside`, `"Entry"` → `← Welcome back!`).

📌 Team update (2026-04-07T15-26-09): Morgana implemented EventContext for nav persistence; Merlin wired DoorScanBroadcast backend with JWT payload shape (Guid eventId, Guid userId, string userName, string direction, DateTime scannedAt) — decided by Morgana, Tank, Merlin

- **Sprint 2026-04-08:** Completed MAUI Crew split + Playwright E2E tests. CrewAppShell configured with tab navigation (Login/Main outside tabs, CheckIn/Attendance/DoorScan inside tabs, DoorScan modal). CheckIn/DoorScan/Attendance removed from main MAUI app. All 14 todos complete. 11 PRs merged (#94 crew-appshell, #95 maui-remove-admin, #96 maui-appshell, plus 8 prior). 3 PRs rebased and fixed (#80, #81, #82, #93 all merge-ready).
### 2026-04-07: SignalR Attendee Notifications

- Added `Microsoft.AspNetCore.SignalR.Client` (version `10.*`) to `LanManager.Maui.csproj`.
- `SignalRService` lives in `Services/` and is registered as a **singleton** (one connection per app session).
- Hub URL is `{Config.ApiBaseUrl}/hubs/attendance`; JWT passed via `AccessTokenProvider`.
- Events filtered by both `eventId` AND `userId == CurrentUser.Id` to avoid showing other attendees' notifications.
- `AttendeeHubViewModel` now receives `SignalRService` via constructor injection; `ConnectAsync` is called inside `LoadAsync` alongside the seat/tournament tasks.
- `CleanupAsync()` is called from `AttendeeHubPage.OnDisappearing` (code-behind only — no business logic there, just lifecycle wiring).
- `ShowNotification` follows the same fire-and-forget `async void` + `Task.Delay` auto-clear pattern used in `DoorScanViewModel`.
- `UserDoorScanned` is a new hub event being added by Tank/Merlin; the SignalRService handler is ready for it (`"Exit"` → `→ You went outside`, `"Entry"` → `← Welcome back!`).

📌 Team update (2026-04-07T15-26-09): Morgana implemented EventContext for nav persistence; Merlin wired DoorScanBroadcast backend with JWT payload shape (Guid eventId, Guid userId, string userName, string direction, DateTime scannedAt) — decided by Morgana, Tank, Merlin

- LanManager.Maui.Crew lives in src/LanManager.Maui.Crew, mirrors the main MAUI app resources/platforms, and references LanManager.Maui.Shared.
- **Sprint 2026-04-08:** Completed MAUI Crew split + Playwright E2E tests. CrewAppShell configured with tab navigation (Login/Main outside tabs, CheckIn/Attendance/DoorScan inside tabs, DoorScan modal). CheckIn/DoorScan/Attendance removed from main MAUI app. All 14 todos complete. 11 PRs merged (#94 crew-appshell, #95 maui-remove-admin, #96 maui-appshell, plus 8 prior). 3 PRs rebased and fixed (#80, #81, #82, #93 all merge-ready).
- LanManager.Maui.Crew lives in src/LanManager.Maui.Crew, mirrors the main MAUI app resources/platforms, and references LanManager.Maui.Shared.
- **Sprint 2026-04-08:** Completed MAUI Crew split + Playwright E2E tests. CrewAppShell configured with tab navigation (Login/Main outside tabs, CheckIn/Attendance/DoorScan inside tabs, DoorScan modal). CheckIn/DoorScan/Attendance removed from main MAUI app. All 14 todos complete. 11 PRs merged (#94 crew-appshell, #95 maui-remove-admin, #96 maui-appshell, plus 8 prior). 3 PRs rebased and fixed (#80, #81, #82, #93 all merge-ready).
- LanManager.Maui.Crew lives in src/LanManager.Maui.Crew, mirrors the main MAUI app resources/platforms, and references LanManager.Maui.Shared.

### 2026-04-08: Report Download/Share — Crew App (Issue #105)

- Report download lives on `AttendancePage` — natural home since operators review attendance before closing an event.
- `DownloadReportAsync` added to `ApiService` in shared lib; follows same `GetAsync + ReadAsByteArrayAsync` pattern as `GetAttendeeQrCodeAsync`.
- `AttendanceViewModel` now injects `AuthService` — needed to gate the button on Admin/Organizer roles.
- `CanDownloadReport` is computed on page load (event Closed + role check).
- Section toggles all default `true`; if all 4 selected, sends `sections=All`.
- Use `Shell.Current.DisplayAlertAsync` in .NET 10 MAUI — `DisplayAlert` is obsolete.
- `Share.RequestAsync` with `ShareFileRequest` + `ShareFile` works cross-platform with no extra NuGet packages.
- PR #111 opened targeting `master`.

### 2026-04-08: PDF Report Sprint Complete — Orchestration Record
**Orchestration Log:** Recorded in `.squad/orchestration-log/2026-04-08T12-20-03Z-circe.md`

**Sprint Status:** ✅ Complete
- Crew app report download/share delivered with section picker on AttendancePage
- File handling: File.SaveAsync on device storage, Share.RequestAsync for native OS sharing
- PR #111 merged successfully as commit 9cb0a15
- Cross-platform support validated: iOS, Android, Windows
- Team: Merlin (backend service/PDF/endpoint), Morgana (frontend), Radagast (tests), Gandalf (merge orchestration)

**Session Record:** Full PDF report sprint documented in `.squad/log/2026-04-08T12-20-05Z-pdf-sprint-complete.md` (20 todos completed, 6 PRs merged clean, zero conflicts)
