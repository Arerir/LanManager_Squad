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

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
