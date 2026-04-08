# Squad Decisions

## Active Decisions

### 2026-04-05: Issue source connected
**By:** Morpheus  
**What:** GitHub repo Arerir/LanManager_Squad connected. 12 starter issues created covering MVP foundation + 2 enhancements.  
**Why:** User requested — issues created from brainstorm session.

#### Issues Created

##### Foundation (MVP-critical)
1. **#1** — Scaffold Aspire solution with AppHost and service references
2. **#2** — Implement EF Core data models with migrations
3. **#3** — Implement Events API with CRUD endpoints
4. **#4** — Implement Users and Registration API endpoints
5. **#5** — Implement Check-in/Check-out API endpoints
6. **#6** — Scaffold React app with Vite, TypeScript, and routing
7. **#7** — Build event management views in React (list, detail, create/edit)
8. **#8** — Build user registration and profile forms in React
9. **#9** — Build MAUI check-in app with operator screen
10. **#10** — Implement real-time attendance board with SignalR

##### Enhancement (post-MVP)
11. **#11** — Build tournament bracket service with live standings
12. **#12** — Implement interactive seating and floor map

#### Labels Created
- `squad` — Squad work item (all issues)
- `backend` — Backend .NET work
- `frontend` — Frontend React work
- `maui` — MAUI mobile/desktop app
- `infrastructure` — Aspire orchestration & infra
- `enhancement` — Post-MVP enhancement

All issues properly labeled for triage workflow.

### 2026-04-05: Aspire solution structure
**By:** Morpheus  
**What:** Solution at repo root. Projects under src/. Frontend React app at frontend/. Solution: LanManager.sln. AppHost orchestrates API; frontend commented out until Trinity scaffolds it. MAUI integrated separately by Switch.  
**Why:** Standard Aspire layout. Separation of src/ and frontend/ keeps dotnet and npm tooling isolated.

### 2026-04-05: Data model decisions
**By:** Tank  
**What:** SQLite for dev DB (easy swap to SQL Server for prod via connection string). Enums stored as strings in DB for readability. Unique constraint on Registration(EventId, UserId) prevents double-registration. CheckInRecord tracks check-out via nullable CheckedOutAt (single record per visit).  
**Why:** Pragmatic dev setup. Relationships are clean and direct — no over-engineering.

### 2026-04-05: Frontend scaffold decisions
**By:** Trinity  
**What:** Vite + React 18 + TypeScript in frontend/. React Router v6 for client-side routing. VITE_API_URL env var for backend URL (defaults to localhost:5000). No CSS framework yet — base styles inline until we settle on a design system.  
**Why:** Vite is fast for dev. TypeScript everywhere from day one. No design system decision yet — keep it flexible.

### 2026-04-05: API contracts (Tank)
**By:** Tank  
**What:** All REST API routes prefixed `/api`. GUIDs as IDs throughout. UTC ISO8601 dates. Status fields serialized as strings (not integers). `[ApiController]` returns `ProblemDetails` for errors; `ValidationProblemDetails` for 400s. `POST /register` and `POST /events` return `201 Created` with `Location` header.  
**Key endpoints:** `GET/POST /api/events`, `GET/PUT/DELETE /api/events/{id}`, `POST /api/users/register`, `GET /api/users/{id}`, `GET /api/users`, `POST /api/events/{id}/register`, `GET /api/events/{id}/attendees`, `POST /api/events/{id}/checkin`, `POST /api/events/{id}/checkout`, `GET /api/events/{id}/attendance`.  
**Why:** Documented for Trinity (React) and Switch (MAUI) to consume without ambiguity.

### 2026-04-05: Frontend API module and routing conventions (Trinity)
**By:** Trinity  
**What:** All API calls in `frontend/src/api/events.ts` and `frontend/src/api/users.ts`. Flat routes in `App.tsx`. Auth/session via `localStorage` (`currentUser` key). No CSS framework — inline styles with consistent patterns. Login stubs against `/api/auth/login` (not yet implemented).  
**Why:** Keeps API surface isolated and typed. Routing is flat for simplicity. Auth deferred to a later issue.

### 2026-04-07: PR Creation Summary
**By:** Gandalf  
**What:** Created 5 GitHub PRs for active branches: fix/maui-event-context-hub (#58), feat/frontend-theme-e2e (#59), squad/25-auth-jwt (#60), squad/28-door-scanner (#61), squad/29-door-log-ui (#62). All targeting `master`.  
**Why:** Standardize PR creation process; enable parallel team review. Remote-only branches required local tracking refs before GitHub API could resolve SHAs. All PRs include issue closures and architecture notes for dependent work.

### 2026-04-07: Theme System Conventions  
**By:** Morgana  
**Branch:** feat/frontend-theme-e2e  
**What:** GameVille cyberpunk theme defined entirely via CSS variables. Core palette: `--bg` (#060612), `--surface` (#0d0d2b), `--text` (#9ca3c8), neon accents (cyan, magenta, purple, orange). Reusable classes: `.btn-primary`, `.btn-ghost`, `.btn-danger`, `.badge-available`, `.badge-loan`. Playwright E2E setup with config in `playwright.config.ts`, tests in `frontend/e2e/`. Reports/results gitignored.  
**Why:** Centralized theme system eliminates color duplication. CSS variables enable runtime theme switching. Playwright foundation ready for e2e automation. e2e directory tracked (only generated artifacts ignored).  
**Best Practices:** Use semantic variables over palette hex; always use button/badge classes; input elements inherit theme automatically.

### 2026-04-07: SignalR Architecture — Real-Time Attendance
**By:** Tank + Trinity  
**Related PRs:** #21 (backend), #22 (frontend)  
**What:** Hub at `/hubs/attendance` (mapped in `Program.cs`). Broadcast-only: `UserCheckedIn` and `UserCheckedOut` events with `AttendanceBroadcast` and `CheckOutBroadcast` DTOs. CORS requires `AllowCredentials()`. Frontend connection via `HubConnectionBuilder` with `config.apiUrl/hubs/attendance`. Initial snapshot from `GET /api/events/{eventId}/attendance`.  
**Why:** Real-time user status without polling. SignalR broadcast pattern keeps hub simple and scalable. CORS credentials required for SignalR sockets. AttendancePage filters by `?eventId=<guid>` query parameter.

### 2026-04-07: MAUI Architecture Patterns
**By:** Switch  
**What:** MVVM pattern with `CommunityToolkit.Mvvm`. ViewModels inherit `ObservableObject`, use `[ObservableProperty]` and `[RelayCommand]` source generators (no code-behind logic). Centralized `ApiService` singleton wrapping `HttpClient` with DTOs co-located. DI container in `MauiProgram.CreateMauiApp()`: Services = singletons, ViewModels/Views = transient. Custom `IValueConverter` implementations in `Converters/`. Shell navigation with `IQueryAttributable` for query parameter handling. Use `Border` instead of deprecated `Frame` (.NET 10 compatibility).  
**Why:** Source generators eliminate boilerplate. Centralized API service enables easy retry/logging/auth. DI container keeps concerns separate and testable. Shell navigation is type-safe and cross-platform. Border is future-proof.  
**File Structure:** `Config.cs` (constants), `Converters/`, `Services/` (HTTP + DTOs), `ViewModels/`, `Views/` (XAML).

### 2026-04-07: Door Pass API Contracts
**By:** Tank  
**Issue:** #27  
**What:** 4 endpoints: `GET /api/events/{eventId}/attendees/{userId}/qrcode` (PNG), `POST /api/events/{eventId}/door-scan` (returns `201` with `DoorPassDto`), `GET /api/events/{eventId}/door-log` (ordered by `ScannedAt` desc), `GET /api/events/{eventId}/outside` (users with most recent Exit direction). QR payload = plain GUID string. Direction enum: `Entry` or `Exit` (stored as string).  
**DTO Shapes:** `DoorScanRequest(UserId, Direction)`, `DoorPassDto(Id, EventId, UserId, UserName, Direction, ScannedAt)`, `OutsideUserDto(UserId, UserName, ExitedAt)`.  
**Why:** QR-based door workflow for venue access tracking. Separates entry/exit scanning from check-in (which happens via API). Door log provides audit trail; outside query enables live occupancy.

### 2026-04-07: Playwright E2E scaffolding for frontend
**By:** Apoc  
**Issue:** #71  
**What:** Use Playwright config scoped to `frontend/e2e` with `npm run dev` webServer at `http://localhost:5173`. Auth fixture logs in through the UI using email/password selectors and waits for `/events`. Smoke tests minimal (root response status + login password field visibility) to validate setup without backend dependencies.  
**Why:** Provides a stable foundation for future E2E suites without requiring API seeding upfront.

### 2026-04-07: MAUI shared services library
**By:** Switch/Circe  
**Date:** 2026-04-08  
**What:** Create `LanManager.Maui.Shared` class library to hold AuthService, AuthHandler, ApiService, and Config. Both attendee and crew apps reference the shared library for unified service layer without duplication.  
**Why:** Both MAUI apps need identical auth and API wiring. Centralizing these services keeps configuration aligned and reduces maintenance.

### 2026-04-07: Crew app scaffold
**By:** Switch  
**What:** No new architectural decisions required. Crew app mirrors existing MAUI structure: MVVM with CommunityToolkit.Mvvm, centralized ApiService singleton, DI container in MauiProgram. File structure: ViewModels/, Views/, Converters/, Resources/Styles/.  
**Why:** Consistency with established MAUI patterns supports parallel development and team onboarding.
### 2026-04-07: Persistent Event Context via React Context + localStorage
**By:** Morgana  
**What:** Introduced `EventContext` (`frontend/src/context/EventContext.tsx`) to share and persist the currently selected event ID across the app.
- **Storage:** `localStorage` key `selectedEventId` — survives page refreshes
- **Provider location:** Wraps `<BrowserRouter>` in `App.tsx` so context is available everywhere including inside `NavLink`
- **Sidebar nav:** `AppLayout` reads `selectedEventId`; links for Attendance, Tournaments, Seating, Equipment include `?eventId=<id>` when set
- **Context setters:** `AttendancePage`, `SeatingPage`, `TournamentPage`, `EventDetailPage` each call `setSelectedEventId` when they receive an `eventId`

**Why:** Nav sidebar links had no event context — clicking "Attendance" after viewing an event detail page would drop `?eventId=` from the URL, breaking the page. This pattern (Context + localStorage) keeps it simple, dependency-free, and consistent with existing auth patterns in the codebase (`currentUser` via `localStorage`).

**Note:** `EquipmentPage` does not currently use `eventId` from search params (global inventory list). The nav link still carries `?eventId=` for future use if equipment becomes event-scoped.

### 2026-04-07: SignalR Door Scan Event — Backend Broadcast
**By:** Merlin  
**Status:** Implemented and compiled ✅

**What:** Added `DoorScanBroadcast` record to `AttendanceBroadcast.cs` and injected `IHubContext<AttendanceHub>` into `DoorPassController` via primary constructor. After `db.SaveChangesAsync()` in `DoorScan` endpoint, broadcasts `"UserDoorScanned"` to all connected SignalR clients.

**Broadcast Payload:**
```csharp
public record DoorScanBroadcast(
    Guid EventId,
    Guid UserId,
    string UserName,
    string Direction,   // "Entry" or "Exit"
    DateTime ScannedAt
);
```

JSON shape (camelCase from System.Text.Json):
```json
{
  "eventId": "...",
  "userId": "...",
  "userName": "...",
  "direction": "Entry",   // or "Exit"
  "scannedAt": "2026-04-07T18:00:00Z"
}
```

**Hub Details:**
- Event Name: `UserDoorScanned`
- Hub endpoint: `/hubs/attendance` (same hub as `UserCheckedIn` / `UserCheckedOut`)
- Triggered from `POST /api/events/{eventId}/door-scan` after door pass is persisted
- Broadcast to **all** connected clients

**MAUI Implementation Notes:**
- Subscribe on `AttendanceHubConnection.On<DoorScanBroadcast>("UserDoorScanned", handler)`
- `Direction == "Exit"` → user went outside; `Direction == "Entry"` → user returned
- `ScannedAt` is UTC; convert for local display
- Filter by `EventId` if the app is scoped to a specific event

### 2026-04-07: SignalR Attendee Real-Time Notifications — MAUI Implementation
**By:** Circe  
**Status:** Implemented ✅

**What:** Added real-time SignalR notifications to the attendee MAUI app (`src/LanManager.Maui`).
- New `SignalRService` singleton at `Services/SignalRService.cs` with JWT authentication
- `AttendeeHubViewModel` connects on load, disconnects on page disappear
- Added `Microsoft.AspNetCore.SignalR.Client` (version `10.*`) to the MAUI .csproj

**Hub Events Handled:**
| Event | Filter | UI message |
|---|---|---|
| `UserCheckedIn` | eventId + userId | ✓ You're checked in! (green, 4s) |
| `UserDoorScanned` direction=Exit | eventId + userId | → You went outside (blue, 4s) |
| `UserDoorScanned` direction=Entry | eventId + userId | ← Welcome back! (blue, 4s) |

**Architecture Decisions:**
1. **`SignalRService` is a singleton** — one shared connection instance per app session. `AttendeeHubViewModel` is transient, so `ConnectAsync` must be called each time the page loads (replaces any prior connection). This is correct because a user can only be in one event context at a time.

2. **`CleanupAsync` in code-behind** — `AttendeeHubPage.OnDisappearing` calls `vm.CleanupAsync()`. This is the minimum code-behind lifecycle hook needed to avoid hanging SignalR connections; no business logic lives there.

3. **JWT for SignalR**: Uses `options.AccessTokenProvider = async () => await _authService.GetTokenAsync()` — same `SecureStorage`-backed token as REST calls.

4. **Event shape:** `(Guid eventId, string userId, string userName, string direction, DateTime scannedAt)` — matches Merlin's DoorScanBroadcast signature.

### 2026-04-07: Test Coverage Expansion — API Controllers
**By:** Apoc (Tester)  
**Status:** ✅ Complete  
**Coverage Impact:** 14.45% → 47.38% line coverage (+229% relative increase)

**What:** Expanded test coverage for LanManager API from 14% to 47% line coverage by writing comprehensive xUnit tests for five previously untested controllers: AuthController, EventsController, RegistrationsController, TournamentController, and UsersController.

**New Test Files Created:**
1. **AuthControllerTests.cs** — 4 tests
   - Login with valid credentials → returns JWT token with user info
   - Invalid email → 401 Unauthorized
   - Invalid password → 401 Unauthorized  
   - Multiple roles → all roles in token

2. **EventsControllerTests.cs** — 14 tests
   - GetAll: empty, multiple events, filter by status, sort by name
   - GetById: existing event, not found
   - Create: valid event (organizer/admin only)
   - Update: existing event, not found
   - Delete: existing event, not found

**Existing Test Files Fixed:**
3. **RegistrationsControllerTests.cs** — already existed with 9 tests
4. **TournamentControllerTests.cs** — already existed with 9 tests, **fixed BracketService mocking bug** (methods aren't virtual — switched to real instance)
5. **UsersControllerTests.cs** — already existed with 5 tests, **fixed async queryable bug** (Mock UserManager.Users didn't support async — used in-memory DB context instead)

**Test Coverage Report:**
```
Module: LanManager.Api
- Line: 47.38%
- Branch: 34.05%
- Method: 59.82%

Test Summary:
- Total: 77 tests
- Failed: 0
- Succeeded: 77
- Duration: 1.3s
```

**Patterns Established:**
- **In-memory DB:** `TestDbContextFactory.Create("unique-test-name")` for isolated test data
- **Mocking UserManager:** Mock<IUserStore> + Mock<UserManager<ApplicationUser>> for identity ops
- **Mocking SignalR:** Mock<IHubContext<THub>> with chained Clients/Groups/SendCoreAsync
- **Auth context:** `ClaimsHelper.SetUser(controller, userId, roles...)` to set ControllerContext.User
- **Real services:** BracketService used as real instance (methods not virtual, can't mock)

**Recommendations:**
1. **Set CI coverage threshold:** Add `dotnet test --collect:"XPlat Code Coverage" --settings coverlet.runsettings` with minimum 45% line coverage gate
2. **Expand to missing areas:** Focus next on LanManager.Data (currently 2.88% coverage) — test repository patterns, model validations, and database constraints
3. **Integration tests:** These are unit tests with mocked dependencies. Consider E2E tests for critical flows (login → register → check-in → tournament enrol)
4. **Coverage visibility:** Add coverage badge to README.md using Codecov or Coveralls integration

### 2026-04-07: Coverage HTML Reports via Coverlet
**By:** Apoc (Tester)  
**Status:** ✅ Implemented

**What:** Configured `coverlet.msbuild` package and MSBuild properties to produce Cobertura coverage XML on every test run. CI workflow extended with `reportgenerator` tool to convert XML → HTML.

**Changes Made:**
1. **Test project** (`src/LanManager.Api.Tests/LanManager.Api.Tests.csproj`):
   - Added `coverlet.msbuild` v6.0.4 package
   - Added `<CollectCoverage>true</CollectCoverage>`
   - Added `<CoverletOutputFormat>cobertura</CoverletOutputFormat>`
   - Added `<CoverletOutput>Coverage/</CoverletOutput>`

2. **CI workflow** (`.github/workflows/ci.yml`):
   - Install `dotnet-reportgenerator-globaltool`
   - Run `reportgenerator` against `coverage.cobertura.xml`
   - Upload HTML reports as `coverage-report` artifact

3. **`.gitignore`**:
   - Added `**/Coverage/` to ignore generated reports

**Rationale:**
- **MSBuild integration:** Automatic coverage on `dotnet test` — no CLI flags required
- **Cobertura format:** Industry-standard, enables HTML report generation
- **Self-contained:** Coverage folder lives in test project, no solution-level changes
- **CI-ready:** Reports uploaded as artifacts for every PR test run

**Trade-offs:**
- **Storage:** Coverage artifacts increase CI storage usage (acceptable for test quality)
- **Build time:** ReportGenerator adds ~5-10s to CI pipeline (worth the visibility)

**Future Work:**
- Add coverage threshold enforcement (e.g., fail build if < 80%)
- Consider badge generation for README
- Explore code coverage trend tracking

### 2026-04-08: Remove admin pages from attendee MAUI app
**By:** Circe (MAUI Dev)  
**Status:** Approved ✅

**What:** LanManager.Maui is now attendee-only. Admin pages (CheckIn, Attendance, DoorScan) removed and owned by LanManager.Maui.Crew.

**Why:** Crew application owns staff workflows. Removing admin pages from attendee app prevents duplicated UI and ensures operators use the dedicated crew build.

### 2026-04-08: Crew app scaffold
**By:** Switch  
**Status:** Approved ✅

**What:** LanManager.Maui.Crew scaffolded with no new architectural decisions. Mirrors existing MAUI structure (MVVM, CommunityToolkit.Mvvm, centralized ApiService, DI container).

**Why:** Consistency with established MAUI patterns supports parallel development and team onboarding.

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
