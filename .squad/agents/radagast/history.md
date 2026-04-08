# Project Context

- **Owner:** Daniel Eli
- **Project:** LanManager_Squad — LAN party management platform
- **Stack:** .NET Aspire orchestration, React frontend, .NET 9 Web API backend, .NET MAUI check-in/check-out apps
- **Created:** 2026-04-05

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-08: Playwright E2E Infrastructure Setup (#71)
**What:** Added Playwright config and e2e scaffolding in `frontend/`, including auth fixture (email/password selectors) and smoke tests. Tests run via `npm run test:e2e` and expect login to redirect to `/events`.

### 2026-04-08: EventReportService Test Scaffold (#103)
**What:** Created `src/LanManager.Api.Tests/Reports/EventReportServiceTests.cs` with 7 xUnit tests covering all spec cases for `EventReportService.GetReportDataAsync`. Merlin had already landed the service on `squad/100-report-service`, so tests were aligned to actual types immediately — all 7 pass green.
**Key types:** `EventReportData` uses `Name` (not `EventName`), `Registrations` and `CheckIns` are `T[]` arrays (use `.Length`, not `.Count`). `ReportSections` is a `[Flags]` enum in `LanManager.Api.Models`. Service takes `LanManagerDbContext` as primary constructor param.
**Gotcha:** `LanManager.Api.Models` has its own `ApplicationUser`, `Event`, `Registration`, `CheckInRecord` mirroring `LanManager.Data.Models`. Use `using ApiModels = LanManager.Api.Models;` alias to resolve ambiguity in test files that need both namespaces.
**Branch:** `squad/103-report-tests-scaffold` — pushed, no PR opened (waiting for implementation PR to merge first).

### 2026-04-08: Complete Report Pipeline Tests (#103)
**What:** Added `EventReportPdfGeneratorTests` (3 cases) and `ReportControllerTests` (4 active + 1 skipped) on top of the 7 existing service tests. PR #110 targets `squad/102-report-endpoint`.
**PDF generator tests:** Set `QuestPDF.Settings.License = LicenseType.Community` in the test constructor before calling `GeneratePdf`. Tests cover full data, null optional sections, and null `CheckedOutAt` values.
**Controller tests:** Use direct controller instantiation (no WebApplicationFactory), same pattern as `EventsControllerTests`. `[Authorize]` attributes are NOT enforced — the 403 test is `[Skip]` with explanation. Build `ReportController` with `new EventReportService(db)` + `new EventReportPdfGenerator()` directly.
**Gotcha — shared working directory:** The repo has no worktree isolation. Another agent can checkout a different branch mid-task, causing git checkout conflicts. Always check `git branch` before committing. Use `git stash` if needed to safely switch branches.
**Namespace ambiguity fix:** Both `LanManager.Api.Models` and `LanManager.Data.Models` define `EventStatus`, `RegistrationStatus`, `Event`, `ApplicationUser`. In test files for the `Api.Models` layer, drop the `using LanManager.Data.Models;` global import and use `using DataModels = LanManager.Data.Models;` alias instead, then qualify as `DataModels.EventStatus.Closed` etc.
**Branch:** `squad/103-report-tests-final` → PR #110.
