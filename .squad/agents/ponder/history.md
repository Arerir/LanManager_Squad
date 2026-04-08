# Project Context

- **Project:** LanManager_Squad
- **Created:** 2026-04-05

## Core Context

Agent Scribe initialized and ready for work.

## Recent Updates

📌 Team initialized on 2026-04-05

## Learnings

### 2026-04-10: README overhaul with shipped features
**What:** Updated README to reflect complete project state including:
- Full architecture (API, Data, MAUI + Crew split, AppHost, Aspire, ServiceDefaults)
- Comprehensive feature set (events, registration, QR check-in, real-time attendance, PDF reports, door scanning, equipment, tournaments, seating)
- Accurate tech stack (.NET 10, EF Core SQLite/SQL Server, SignalR, QuestPDF, React 19, Vite, Playwright)
- Setup instructions for all components (API, frontend, MAUI apps, database)
- Testing guidance (API tests, E2E, linting)
- All API endpoints documented (11 categories: auth, events, users/registration, check-in, door, reports, equipment, tournaments, seats, seating)
- Real-time SignalR hub documentation
- Development notes for contributors

**Why:** Previous README was outdated—only 5 lines covering old state (.NET 10, Aspire AppHost as primary entry point). Project has shipped 111 merged PRs with major features (report pipeline, Crew app, door scanning, E2E testing). New README targets developers cloning the repo.

**Key facts learned:**
- Solution uses `.slnx` (dotnet 10 XML format) with 8 projects organized under `src/`
- Two MAUI apps: attendee-focused (check-in) and admin-focused (Crew) with shared services layer
- Frontend uses Playwright for E2E (config in repo root with `npm run test:e2e`)
- Report generation pipeline complete: API endpoint + PDF generator + frontend/Crew download UI with section toggles
- No CI workflow configured (.github/workflows empty)
- CORS configured for `localhost:5173` + `https:5173`
- Database: SQLite dev, SQL Server prod (via EF Core migrations)

### 2026-04-09: Documentation Sync — .NET 9 → .NET 10

**Task:** Updated all 8 squad agent history.md files to reflect project baseline .NET 10 instead of stale .NET 9 references.

**Files Updated:**
1. `.squad/agents/merlin/history.md` — Updated .NET 9 → .NET 10
2. `.squad/agents/gandalf/history.md` — Updated .NET 9 → .NET 10
3. `.squad/agents/circe/history.md` — Updated .NET 9 → .NET 10
4. `.squad/agents/morgana/history.md` — Updated .NET 9 → .NET 10
5. `.squad/agents/radagast/history.md` — Updated .NET 9 → .NET 10
6. `.squad/agents/apoc/history.md` — Updated .NET 9 → .NET 10
7. `.squad/agents/sibyll/history.md` — Updated .NET 9 → .NET 10
8. `.squad/agents/ponder/history.md` — Updated .NET 9 → .NET 10

**Context:** Project targets `net10.0` across all csproj files. Agent history files contained outdated references to .NET 9, creating confusion for new team members and future maintenance. All 8 corrections were documentation-only (no code changes).

**Validation:** Git diff confirmed version string corrections only.

**Status:** Complete, committed to feat/camera-toggle-scanners (merged with PR #127).

**Importance:** Squad documentation now consistently reflects the project's actual baseline (.NET 10). Reduces onboarding friction and eliminates version ambiguity.

