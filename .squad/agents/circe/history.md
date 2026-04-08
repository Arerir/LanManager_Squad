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

- LanManager.Maui now always routes event selection to AttendeeHubPage; admin/organizer flows live in LanManager.Maui.Crew.

- Created LanManager.Maui.Shared to host AuthService, AuthHandler, ApiService, and Config for reuse across MAUI apps.
- LanManager.Maui now references the shared library; AppStateService remains app-specific.

- LanManager.Maui.Crew lives in src/LanManager.Maui.Crew, mirrors the main MAUI app resources/platforms, and references LanManager.Maui.Shared.
- **Sprint 2026-04-08:** Completed MAUI Crew split + Playwright E2E tests. CrewAppShell configured with tab navigation (Login/Main outside tabs, CheckIn/Attendance/DoorScan inside tabs, DoorScan modal). CheckIn/DoorScan/Attendance removed from main MAUI app. All 14 todos complete. 11 PRs merged (#94 crew-appshell, #95 maui-remove-admin, #96 maui-appshell, plus 8 prior). 3 PRs rebased and fixed (#80, #81, #82, #93 all merge-ready).
- LanManager.Maui.Crew lives in src/LanManager.Maui.Crew, mirrors the main MAUI app resources/platforms, and references LanManager.Maui.Shared.
- **Sprint 2026-04-08:** Completed MAUI Crew split + Playwright E2E tests. CrewAppShell configured with tab navigation (Login/Main outside tabs, CheckIn/Attendance/DoorScan inside tabs, DoorScan modal). CheckIn/DoorScan/Attendance removed from main MAUI app. All 14 todos complete. 11 PRs merged (#94 crew-appshell, #95 maui-remove-admin, #96 maui-appshell, plus 8 prior). 3 PRs rebased and fixed (#80, #81, #82, #93 all merge-ready).
- LanManager.Maui.Crew lives in src/LanManager.Maui.Crew, mirrors the main MAUI app resources/platforms, and references LanManager.Maui.Shared.
