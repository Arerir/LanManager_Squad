# Project Context

- **Owner:** Daniel Eli
- **Project:** LanManager_Squad — LAN party management platform
- **Stack:** .NET Aspire orchestration, React frontend, .NET 10 Web API backend, .NET MAUI check-in/check-out apps
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

### 2026-04-08: PDF Report Sprint Complete — Orchestration Record
**Orchestration Log:** Recorded in `.squad/orchestration-log/2026-04-08T12-20-01Z-radagast.md`

**Sprint Status:** ✅ Complete
- All 14 tests delivered (7 service, 3 PDF generator, 4 controller)
- 1 test intentionally skipped (auth middleware)
- PR #110 merged successfully as commit cb5704a
- Master merge validated: no test failures, CI green
- Team: Merlin (backend), Morgana (frontend), Circe (MAUI crew), Gandalf (merge orchestration)

**Session Record:** Full PDF report sprint documented in `.squad/log/2026-04-08T12-20-05Z-pdf-sprint-complete.md` (20 todos completed, 6 PRs merged clean, zero conflicts)

### 2026-04-09: TestService Unit Tests (#135)
**What:** Created `src/LanManager.Api.Tests/TestServiceTests.cs` with 7 xUnit tests covering all three methods of `TestService`: `GetCheckInAsync` (2), `GetDoorPassAsync` (2), `GetQrCodeAsync` (3). All 7 pass green, 0 warnings, 0 errors.
**Key types:** `ICustomHttpClient` lives in `LanManager.Api.Clients` alongside `TestEnum`. `GetQrCodeAsync` takes `Guid userGuid` (not `UserDto`) — the service layer bridges the gap by passing `dto.Id`. `QrCodeAtt` is a record defined directly in `TestService.cs`.
**Pattern:** Thin service tests use only `Mock<ICustomHttpClient>` — no DB, no WebApplicationFactory. Verify argument forwarding with `mockClient.Verify(...)` and assert return value mapping separately.
**Branch:** `feat/testservice-tests` → PR #135 targeting `master`.

### 2026-04-09: TestService GetDoorPassAsync IEnumerable update (#135 follow-up)
**What:** Updated `TestServiceTests.cs` to match `GetDoorPassAsync` returning `Task<IEnumerable<DoorPassDto>>`. `ICustomHttpClient` and `TestService` were already updated by Daniel Eli before this task ran.
**Changes:** `GetDoorPassAsync_ReturnsClientResult` now builds a `List<DoorPassDto>` and asserts via `Assert.Single` + `result.First()`. `GetDoorPassAsync_ForwardsCorrectArguments` returns a `List<DoorPassDto>` from the mock. Added `GetDoorPassAsync_ReturnsMultipleItems` to verify the full collection passes through. Total test count: 8 (7 original + 1 new), all pass.
**Gotcha:** The `edit` tool matched the wrong occurrence when the target block appeared after a mid-edit state; always verify the file content with a raw read before assuming the view is current.

### 2026-04-09: GetDoorPassAsync Signature Change — IEnumerable<DoorPassDto> (#135 followup)
**What:** `ICustomHttpClient.GetDoorPassAsync` was updated to return `Task<IEnumerable<DoorPassDto>>`. Updated both `ITestService`/`TestService` in `TestService.cs` and the two `GetDoorPassAsync` tests in `TestServiceTests.cs` to match.
**Pattern:** Happy-path test now asserts `Assert.NotNull`, `Assert.Single`, and `result.First()` equality. Argument forwarding test uses `List<DoorPassDto>` for the mock return value.
**Gotcha:** When a client interface changes return type, both the service wrapper AND the ITestService interface declaration must be updated — they both lived in `TestService.cs`.
