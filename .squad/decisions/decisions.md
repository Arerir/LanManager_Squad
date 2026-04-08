# Decisions

This document consolidates all architectural and implementation decisions made by the squad. Entries are grouped by feature area and ordered chronologically.

---

## JSON Enum Deserialization (Global Convention)

**Date:** 2026-04-09 | **Author:** Merlin | **Reviewer:** Gandalf | **PR:** #113

**Decision:** Register `JsonStringEnumConverter` globally in `Program.cs` via `AddJsonOptions`. All C# enums in DTOs consumed by the React frontend are serialized/deserialized as strings. No per-property `[JsonConverter]` attributes needed on future DTOs.

**Root cause fixed:** `System.Text.Json` defaults to integer deserialization; React frontend always sends enum values as strings (e.g., `"Active"`, `"Draft"`). Global converter resolves all current and future DTO mismatches.

**Impact:** Affects all DTOs with enum properties (`EventStatus`, etc.) for both POST and PUT endpoints. No breaking changes — frontend already expected string values.

---

## PDF Download URL Pattern

**Date:** 2026-04-08 | **Author:** Merlin | **PR:** #112

**Decision:** All `fetch()` calls in `frontend/src/api/` must use `${config.apiUrl}/api/...` (absolute URL). Never use bare `/api/...` relative paths.

**Root cause fixed:** `downloadEventReport()` used a relative URL. The Vite dev server has no `/api` proxy, so the request hit Vite's SPA fallback and returned `index.html` (~630 bytes) with 200 OK. This was silently saved as a `.pdf`, producing an invalid file.

**Pattern:** Use `getToken()` helper from `auth.ts` rather than reading `localStorage` directly.

---

## Display Name vs Email in Attendance and Seating

**Date:** 2026-04-08 | **Authors:** Merlin (backend), Morgana (frontend) | **PRs:** #121, #120

**Decision:** Expose and use the display `Name` field (not `UserName` email) as the primary identifier throughout attendance and seating features.

**Backend changes (PR #121):**
- `AttendanceDto` gains `Name: string` field alongside `UserName`
- `CheckInController.GetAttendance` projects `c.User.Name` in LINQ query
- `SeatsController.Assign` looks up user from DB and stores `user?.Name` as `AssignedUserName`

**Frontend changes (PR #120):**
- `AttendanceDto` TypeScript interface gains `name: string`
- Seating attendee cards: display name as primary text (1rem, bold), email as secondary (0.72rem, muted)
- `AttendancePage` SignalR `CheckedInBroadcast` interface gains `name` field
- Seat assignment call passes `.name` instead of `.userName`

**Pattern:** When a DTO changes in both frontend and backend simultaneously — update TypeScript interface first, then usage sites, then build to catch missed locations via compiler errors.

---

## Equipment Detail Modal with QR Code

**Date:** 2026-04-08 | **Author:** Morgana | **Reviewer:** Gandalf | **Issue:** #115 | **PR:** #117

**Decisions:**
1. **QR library:** `qrcode.react` — installed (no prior QR library in project). Use `QRCodeSVG` (not canvas) for crisp vector rendering at any density.
2. **Component location:** `frontend/src/components/` — consistent with `ReportDownloadButton.tsx`. Reusable modals belong in components, not pages.
3. **Role check:** Use `getUser()` from `api/auth.ts` — already typed, cleaner than reading `localStorage` directly. Admin/Organizer roles see Edit button.
4. **Edit action:** `navigate('/equipment/:id/edit')` — route to be implemented; `onEdit` callback on the component.
5. **Focus trap:** Implemented manually (no external library) via `keydown` listener wrapping Tab/Shift+Tab. Restores focus to previously focused element on unmount.

---

## Seating View Layout Restructure

**Date:** 2026-04-08 | **Author:** Morgana | **Reviewer:** Gandalf | **Issue:** #114 | **PR:** #118

**Decision:** Promote the seating map from a two-column flex sidebar layout to its own full-width dedicated panel, with the attendees list in a separate card below.

**Layout (after):**
```
┌──────────────────────────────────────────────┐
│  Page header (title + count + Setup Grid btn) │
└──────────────────────────────────────────────┘
┌──────────────────────────────────────────────┐
│  Seating panel (full-width card)              │
│   · SVG map (horizontally scrollable)         │
│   · Selected-seat callout at bottom           │
└──────────────────────────────────────────────┘
┌──────────────────────────────────────────────┐
│  Attendees section (full-width card)          │
│   · Responsive grid: auto-fill, 200px min     │
│   · Blue border on cards when assignable      │
└──────────────────────────────────────────────┘
```

**No functional changes** — all role guards, assign/unassign, and grid setup controls preserved.

---

## Seating Map Centering + Attendee Card Visual Design

**Date:** 2026-04-08 | **Author:** Morgana | **Reviewer:** Gandalf | **PR:** #119

**Decisions:**

**SVG Centering:** Wrap SVG in a flex container (`justifyContent: center`). Remove `minWidth: 100%`. Give SVG exact computed dimensions. Parent container handles `overflow-x` for scroll.

**Attendee card design:** Vertical cards (min-height 80px) replacing horizontal pills.
- **Header:** "UNASSIGNED" (red `#e74c3c`) or "Seat X" (green `#2ecc71`) in small caps with letter-spacing
- **Body:** Full attendee name at 1rem, fontWeight 500
- **Unassigned background:** Dark red (`#3a0000`) — action-needed signal
- **Assigned background:** Dark green (`#1a3a1a`)
- Grid: `minmax(160px, 1fr)` for taller cards

**Rationale:** Color-coding (red/green) for status is effective; vertical layout more scannable and professional than horizontal pills.

---

## DoorLog Tab: SignalR Live Updates

**Date:** 2026-04-08 | **Author:** Morgana | **PR:** #125

**Decision:** Wire `DoorLogTab` to SignalR using the identical pattern already established in `LiveAttendanceTab`.

- Per-tab `HubConnectionBuilder` connection (not a shared singleton)
- `useEffect` with `eventId` dependency; cleanup calls `connection.stop()`
- Event scoping guard: `if (payload.eventId !== eventId) return` — no cross-event pollution
- Prepend new entries (newest-first); reset pagination to page 1 on new broadcast
- Direction column: colored pill badges — Entry = green (`#27ae60`), Exit = red (`#c0392b`)
- Hub status badge (Live/Reconnecting/Disconnected) with `withAutomaticReconnect()` + all three reconnect handlers
- Synthetic React key: `userId + scannedAt` for broadcast rows — no collision with REST-fetched rows

**Consequence:** Live door scans appear instantly in the tab without page refresh.

---

## Door Scan Auto-Direction, Attendee Status, QR Colors, Crew Login Gate

**Date:** 2026-04-09 | **Author:** Merlin | **Reviewer:** Gandalf | **PR:** #126

### 1. Auto-flip direction after Exit (API-level idempotency)
After a user's last door pass is `Exit`, the next scan is **always forced to `Entry`** at the API — regardless of what the crew app sends. The UI direction selection is still used for the first scan and for Exit scans.

**Placement:** API data layer, not MAUI app. This ensures idempotency even against buggy or misconfigured crew clients.

### 2. Attendee door status endpoint
`GET /api/events/{eventId}/attendees/{userId}/door-status` returns `{ "status": "Entry" | "Exit" | "Unregistered" }`.

- **Auth:** Staff can query any user; attendees can only query themselves — matches `GetQrCode` authorization pattern
- Additive — no existing contracts changed

### 3. QR page background color = door status indicator
Attendee QR page background encodes current scan state:
- **Dark green** (`#0a3a1a`) — Entry (checked in and inside)
- **Dark red** (`#3a0a0a`) — Exit (checked in but currently outside)
- **Dark purple** (`#2d0050`) — Unregistered / never scanned

Implementation: `BackgroundColor="{Binding PageBackground}"` (XAML binding to `ObservableProperty`). No code-behind color manipulation.

### 4. Crew login role gate
`LoginViewModel.LoginAsync` checks JWT role claims post-login. Non-crew users (no Admin/Organizer/Operator role): `LogoutAsync()` called **before** setting `ErrorMessage` — session invalidated before UI feedback, no navigation window for unauthorized users.

**Scope:** UX gate only. API authorization remains the authoritative security boundary.

**Impact on other agents:**
- Tank/Circe: No change — `UserDoorScanned` SignalR event fires with same payload
- Switch/Morgana: No change — status endpoint is additive
- Attendee MAUI: `AttendeeQrViewModel` constructor changed from `(AuthService, AppStateService)` to `(ApiService, AuthService, AppStateService)` — DI auto-resolved, `MauiProgram.cs` unchanged

---

## Gandalf PR Review Decisions

### PR #113 Merged — JSON Enum Deserialization Fix
**Date:** 2026-04-08 | See "JSON Enum Deserialization" section above.

### PRs #117 & #118 Merged — Equipment Modal & Seating Panel
**Date:** 2026-04-08 | See "Equipment Detail Modal" and "Seating View Layout Restructure" sections above.  
**Merge order:** #117 first → pull master → #118. Both required `--admin` flag. Both branches auto-deleted.

### PR #119 Merged — Seating Map Centering + Card Redesign
**Date:** 2026-04-08 | See "Seating Map Centering + Attendee Card Visual Design" section above.

### PRs #125 & #126 Merged — DoorLog SignalR + Door Scan Sprint
**Date:** 2026-04-09 | See "DoorLog Tab: SignalR Live Updates" and "Door Scan Auto-Direction..." sections above.  
**Both PRs:** 4/4 CI checks passing, `TreatWarningsAsErrors` compliant, squash merged, branches deleted.
