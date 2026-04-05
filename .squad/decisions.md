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

## Governance

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction

- All meaningful changes require team consensus
- Document architectural decisions here
- Keep history focused on work, decisions focused on direction
