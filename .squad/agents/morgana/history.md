# Project Context

- **Owner:** Daniel Eli
- **Project:** LanManager_Squad — LAN party management platform
- **Stack:** .NET Aspire orchestration, React frontend, .NET 9 Web API backend, .NET MAUI check-in/check-out apps
- **Created:** 2026-04-05

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

## Event Context Persistence (2026-04-07)

Implemented `EventContext` at `frontend/src/context/EventContext.tsx` to persist the selected event across sidebar navigation.

**What was done:**
- Created `EventProvider` + `useEventContext` hook backed by `localStorage` (key: `selectedEventId`)
- Wrapped `<BrowserRouter>` in `EventProvider` inside `App.tsx`
- `AppLayout.tsx` reads `selectedEventId` from context; nav links for Attendance, Tournaments, Seating, and Equipment build hrefs dynamically (`/page?eventId=<id>` when set, `/page` otherwise)
- `AttendancePage`, `SeatingPage`, `TournamentPage` call `setSelectedEventId` when `eventId` is present in search params
- `EventDetailPage` calls `setSelectedEventId` when an event loads (covers the "click event → use sidebar" flow)

**EquipmentPage note:** EquipmentPage currently doesn't use `eventId` from search params at all (global equipment list), so no context sync needed there — nav link still carries `?eventId=` for future use.

**TypeScript:** All changes pass `tsc --noEmit` with zero errors. No implicit `any`.

## Issue #6 — React scaffold (2026-04-05)
Scaffolded frontend/ with Vite + React 18 + TypeScript. React Router v6. AppLayout with sidebar nav. Stub pages for Dashboard, Events, Users, Attendance. VITE_API_URL config. PR opened. Next: events views (#7) and user registration forms (#8) once Tank defines API contracts.

## Theme Branch Review (2026-04-07)

Reviewed feat/frontend-theme-e2e branch containing GameVille cyberpunk theme redesign.

**Issues Found:**
1. **Duplicate code in EquipmentPage.tsx** - Original light theme version wasn't removed, causing 447-line file with duplicated component
2. **Incomplete theme application in EventDetailPage.tsx** - STATUS_COLORS was changed to object format but usage wasn't updated, causing runtime error
3. **Hardcoded colors in Login/RegisterPage** - Input styles and labels used hex colors instead of CSS variables
4. **Missing e2e tests** - Playwright configured but no test files existed
5. **e2e/ directory gitignored** - Tests were excluded from git, which is non-standard

**Fixes Applied:**
- Removed duplicate code from EquipmentPage.tsx
- Fixed EventDetailPage status badge rendering and converted all buttons to use theme classes (btn-primary, btn-ghost, btn-danger)
- Converted Login/RegisterPage hardcoded colors to CSS variables (var(--text), var(--text-muted), var(--danger))
- Added e2e/smoke.spec.ts with basic navigation and theme validation tests
- Removed e2e/ from .gitignore (test files should be tracked)
- Added missing playwright.config.ts to git

**Theme System Notes:**
- CSS variables in index.css define the complete palette (--cyan, --magenta, --purple, --bg, --surface, --text, etc.)
- Semantic variables map to specific colors (--accent = --cyan, --success = green, --danger = red)
- Reusable button classes: .btn-primary (gradient), .btn-ghost (outlined), .btn-danger (destructive)
- Badge classes: .badge-available (success), .badge-loan (danger)
- Input/select/textarea inherit theme styles automatically via global CSS
- Consistent table styling: #0d0d2b header bg, #1e1e42 borders, alternating row backgrounds

Commit: edf150a on feat/frontend-theme-e2e

## Event Context Persistence (2026-04-07)

Implemented `EventContext` at `frontend/src/context/EventContext.tsx` to persist the selected event across sidebar navigation.

**What was done:**
- Created `EventProvider` + `useEventContext` hook backed by `localStorage` (key: `selectedEventId`)
- Wrapped `<BrowserRouter>` in `EventProvider` inside `App.tsx`
- `AppLayout.tsx` reads `selectedEventId` from context; nav links for Attendance, Tournaments, Seating, and Equipment build hrefs dynamically (`/page?eventId=<id>` when set, `/page` otherwise)
- `AttendancePage`, `SeatingPage`, `TournamentPage` call `setSelectedEventId` when `eventId` is present in search params
- `EventDetailPage` calls `setSelectedEventId` when an event loads (covers the "click event → use sidebar" flow)

**EquipmentPage note:** EquipmentPage currently doesn't use `eventId` from search params at all (global equipment list), so no context sync needed there — nav link still carries `?eventId=` for future use.

**TypeScript:** All changes pass `tsc --noEmit` with zero errors. No implicit `any`.

📌 Team update (2026-04-07T15-26-09): Merlin broadcast DoorScanBroadcast via SignalR; Circe wired MAUI listener with JWT auth and auto-clearing notifications — decided by Tank, Merlin, Circe
