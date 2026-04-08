# Project Context

- **Owner:** Daniel Eli
- **Project:** LanManager_Squad — LAN party management platform
- **Stack:** .NET Aspire orchestration, React frontend, .NET 10 Web API backend, .NET MAUI check-in/check-out apps
- **Created:** 2026-04-05

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

## Seating Page UI Polish (2026-04-08)

Fixed two visual issues in `SeatingPage.tsx`:

**1. SVG Map Centering:**
- Problem: SVG had `minWidth: '100%'` which forced it to stretch, preventing centering
- Solution: Wrapped SVG in a `<div>` with `display: 'flex', justifyContent: 'center'`
- SVG now has exact dimensions (`width={(maxCol + 1) * (SEAT_W + GAP)}`) with `display: 'block'`
- Result: Map centers when smaller than panel, scrolls horizontally when wider

**2. Attendee Card Redesign:**
- Changed from horizontal pill layout to vertical card layout:
  - `minHeight: 80`, `flexDirection: 'column'`, `justifyContent: 'space-between'`
  - Header: seat status in uppercase with letterSpacing (green if assigned, red "UNASSIGNED" if not)
  - Body: full attendee name at `fontSize: 1rem, fontWeight: 500`
  - **Key change:** Unassigned cards now use `background: '#3a0000'` (dark red) instead of generic dark blue
  - Grid minmax reduced to `160px` to fit taller cards
- Visual impact: Unassigned attendees are immediately scannable via red cards

**Branch/PR:** `fix/seating-ui-polish` → PR #119

**Pattern:** When centering dynamic-width SVG: wrap in flex container, give SVG exact computed dimensions, parent container still handles overflow-x for scroll.

## Issue #115 — Equipment Detail Modal with QR Code (2026-04-08)

### What was done
- Installed `qrcode.react` (no QR library existed in the project)
- Created `frontend/src/components/EquipmentDetailModal.tsx`:
  - `QRCodeSVG` at 220×220px on a white background pad
  - All `EquipmentDto` fields in a read-only layout (name, type, qrCode, status badge, borrower info, notes)
  - Edit button shown only to `Admin`/`Organizer` roles via `getUser().roles`
  - Accessible: `role="dialog"`, `aria-modal`, focus trap, Escape key, click-backdrop close
- Updated `EquipmentPage.tsx`:
  - Row `onClick` opens `EquipmentDetailModal` with the clicked item
  - Return button uses `e.stopPropagation()` to avoid row click conflict
  - Edit navigates to `/equipment/:id/edit`
- PR #117 opened

### Key patterns
- Role check: `getUser()` from `api/auth.ts` returns `LoginResponse` with `roles: string[]`; use `.some(r => ['Admin', 'Organizer'].includes(r))`
- Modal close: expose both `onClose` (backdrop, Escape, close btn) and `onEdit` callbacks
- `qrcode.react` exports `QRCodeSVG` (SVG) and `QRCodeCanvas` (canvas); SVG preferred for crisp rendering
- Focus trap implemented manually via `keydown` listener; returns focus to previously focused element on unmount

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

### 2026-04-08: PDF Report Sprint Complete — Orchestration Record
**Orchestration Log:** Recorded in `.squad/orchestration-log/2026-04-08T12-20-02Z-morgana.md`

**Sprint Status:** ✅ Complete
- ReportDownloadButton component delivered with inline section picker
- PDF download pattern established: fetch → blob → createObjectURL → anchor click → revokeObjectURL
- PR #109 merged successfully as commit 931b424
- Frontend conventions established: `frontend/src/components/` directory created for reusable components
- Team: Merlin (backend service/PDF generator/endpoint), Radagast (tests), Circe (MAUI crew), Gandalf (merge orchestration)

**Session Record:** Full PDF report sprint documented in `.squad/log/2026-04-08T12-20-05Z-pdf-sprint-complete.md` (20 todos completed, 6 PRs merged clean, zero conflicts)

## Issue #114 — Seating View Dedicated Panel (2026-04-08)

Promoted the seating map from a cramped flex sidebar column to its own full-width dedicated panel, with attendees in a separate card below.

**What was done:**
- `SeatingPage.tsx`: replaced two-column `flex` layout with stacked full-width panels
- Seating SVG map now in its own card (`#12122a` bg, rounded border, horizontally scrollable)
- Selected-seat callout moved inline at the bottom of the seating panel
- Attendees section in a separate card below; rendered as a responsive grid (`auto-fill, minmax(200px, 1fr)`)
- Attendee cards highlight blue when a seat is selected + empty, clarifying the click-to-assign flow
- No functional changes — all role guards, assign/unassign, and grid setup preserved

**Branch/PR:** `squad/114-seating-dedicated-panel` → PR #118

**Layout note:** The attendees responsive grid scales nicely from narrow viewports (1 column) to wide desktop (5+ columns) without any media queries.
## Seating Display Names (2026-04-08)

Updated frontend to display user display names instead of email addresses on the seating page.

**What was done:**
- Added 
ame: string field to AttendanceDto interface in rontend/src/api/attendance.ts
- Updated SeatingPage.tsx attendee cards to show display name as primary text (1rem, bold) with email as secondary text below (0.72rem, muted)
- Modified seat assignment call to pass .name instead of .userName to backend
- Updated AttendancePage.tsx SignalR CheckedInBroadcast interface to include 
ame field for real-time check-ins

**UI Changes:**
- Attendee cards now have three lines:
  1. Header: "UNASSIGNED" or "Seat X1" (status)
  2. Body: Display name (e.g., "John Doe") — NEW primary text
  3. Sub-body: Email address (smaller, muted) — was previously the only identifier

**Backend Coordination:**
- This change depends on backend adding 
ame field to AttendanceDto responses (API + SignalR broadcasts)
- Backend also updating SeatsController.Assign to populate ssignedUserName from user's display name
- Both PRs merge independently; frontend gracefully handles missing 
ame field until backend is deployed

**Branch/PR:** ix/seating-display-name-frontend → PR #120

**Pattern:** When backend DTO changes in parallel, update TypeScript interfaces first, then usage sites, then build to catch any missed locations via compiler errors.
