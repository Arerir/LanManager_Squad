# Session Log: Sprint — Door Scan Live Updates & Backend Sprint

**Date:** 2026-04-09T10:00:03Z  
**Title:** Door Scan Live Updates (SignalR), Auto-Direction, Attendee Status, QR Colors, Crew Login Gate  
**Sprint Lead:** Gandalf  
**Status:** ✅ Complete

---

## Overview

Two PRs delivered and merged to master in this sprint, advancing the door scanning experience with real-time frontend updates, smarter backend direction logic, a new attendee status endpoint, visual QR page status feedback, and a crew login role gate.

---

## Deliverables

### PR #125 — DoorLog SignalR Live Updates (Morgana)
**Branch:** `feat/doorlog-signalr-frontend` → master  
**Squash commit:** pre-344570f  

- `DoorLogTab` wired to `/hubs/attendance` SignalR via `HubConnectionBuilder` (same pattern as `LiveAttendanceTab`)
- `DoorScanBroadcast` TypeScript interface fully typed: `{ eventId, userId, userName, direction, scannedAt }`
- Event scoping guard: `payload.eventId !== eventId` filters cross-event broadcasts
- `useEffect` cleanup: `connection.stop()` on unmount and `eventId` change — no connection leaks
- `withAutomaticReconnect()` + reconnecting / reconnected / close handlers
- Direction column: colored pill badges — **Entry = green**, **Exit = red**
- Hub status badge: Live / Reconnecting... / Disconnected
- Synthetic React key: `userId + scannedAt` for broadcast rows

### PR #126 — Door Scan Sprint: Auto-Direction, Status Endpoint, QR Colors, Crew Gate (Merlin)
**Branch:** `feat/door-scan-status-sprint` → master  
**Squash commit:** 344570f  

#### 1. Auto-flip direction (API-level idempotency)
- `DoorPassController.DoorScan` queries user's latest door pass before saving
- If last pass was `Exit` → forces `dir = Entry` regardless of crew app input
- Prevents double-exits at the data layer; client direction still honored for first scan and Exit initiation

#### 2. Attendee door status endpoint
- `GET /api/events/{eventId}/attendees/{userId}/door-status`
- Returns `{ "status": "Entry" | "Exit" | "Unregistered" }`
- Auth: staff can query any user; attendees query themselves only (matches `GetQrCode` pattern)
- Additive — no existing contracts changed

#### 3. QR page background as status indicator
- `ApiService.GetAttendeeDoorStatusAsync()` added to `LanManager.Maui.Shared`
- `AttendeeQrViewModel` constructor: `(ApiService, AuthService, AppStateService)` — fetches status after QR generation
- `PageBackground` ObservableProperty: dark green (`#0a3a1a`) / dark red (`#3a0a0a`) / dark purple (`#2d0050`)
- `AttendeeQrPage.xaml`: `BackgroundColor="{Binding PageBackground}"`; StatusMessage text color = White

#### 4. Crew login role gate
- `LoginViewModel.LoginAsync` checks JWT role claims post-login
- Non-crew users: `LogoutAsync()` called first, then `ErrorMessage = "Access denied"` set
- UX boundary only; API authorization remains authoritative

---

## Merge Timeline

| PR  | Author  | Branch                        | Merge Type | CI     | Master HEAD  |
|-----|---------|-------------------------------|------------|--------|--------------|
| #125 | Morgana | feat/doorlog-signalr-frontend | Squash     | ✅ 4/4 | pre-344570f  |
| #126 | Merlin  | feat/door-scan-status-sprint  | Squash     | ✅ 4/4 | 344570f      |

---

## Architecture Decisions Captured

1. **SignalR per-tab connection** — not a shared singleton; matches established `LiveAttendanceTab` pattern
2. **Event scoping guard** — `eventId` filter prevents cross-event data bleed in multi-event deployments
3. **API-level direction idempotency** — auto-flip belongs at data layer, not client; prevents any client from double-exiting
4. **Status endpoint auth model** — follows `GetQrCode` pattern exactly; consistent authorization across door pass features
5. **MVVM QR page colors** — `ObservableProperty` + XAML binding; no code-behind color manipulation
6. **Crew gate order** — logout before error message; session invalidated before UI feedback shown

---

## Quality Metrics

- **TreatWarningsAsErrors compliance:** Both PRs — API ✅, MAUI ✅, MAUI.Crew ✅ (0 warnings)
- **CI status:** Both PRs — 4/4 checks passing (API Tests, Build API, Build Frontend, GitGuardian)
- **Breaking changes:** None — all changes additive or internal-only
- **Merge conflicts:** 0
- **Branches deleted:** 2 (feat/doorlog-signalr-frontend, feat/door-scan-status-sprint)

---

## Team Contributions

- **Morgana** — DoorLog SignalR integration, hub status badge, colored pill badges (PR #125)
- **Merlin** — Auto-direction backend, status endpoint, QR page colors, Crew login gate (PR #126)
- **Gandalf** — Architectural review, CI validation, squash merge of both PRs

---

## Next Steps

- Door scan flow is now fully real-time and resilient
- Attendee QR page visually communicates door status at a glance
- Crew app is access-controlled at login — no unauthorized crew usage
- Future: consider E2E Playwright tests for SignalR live update scenarios

---

**Sprint Status:** 🎉 **COMPLETE**  
**Master Status:** ✅ **CLEAN** — HEAD at `344570f`
