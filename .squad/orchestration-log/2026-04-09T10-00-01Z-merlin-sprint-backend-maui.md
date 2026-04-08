# Orchestration Log: merlin-sprint-backend-maui

**Date:** 2026-04-09T10:00:01Z  
**Agent:** Merlin  
**Manifest Entry:** PR #126 merged — door auto-direction, attendee status endpoint, QR colors, Crew login gate

## Summary

Merlin delivered four focused improvements to the door scanning flow, attendee visibility, and crew app security in a single sprint PR. All changes maintain `TreatWarningsAsErrors` compliance (zero warnings across API, MAUI, MAUI.Crew).

## Deliverables

### Task 1 — Backend: Auto-flip door direction after Exit
- **File:** `DoorPassController.cs`
- Before saving a door pass, queries the user's latest door pass for the event
- If last pass was `Exit`, forces `dir = Entry` regardless of what the crew app sends
- **Guarantee:** API-level idempotency — prevents double-exits even from buggy or misconfigured crew clients
- Crew UI direction selection still respected for first scan and for Exit initiation

### Task 2 — Backend: Attendee door status endpoint
- **Route:** `GET /api/events/{eventId}/attendees/{userId}/door-status`
- **Response:** `{ "status": "Entry" | "Exit" | "Unregistered" }`
- **Authorization:** Staff can query any user; attendees can only query themselves (matches `GetQrCode` pattern)
- Additive — no existing contracts broken

### Task 3 — Attendee MAUI: QR page status colors
- **File:** `ApiService.cs` (LanManager.Maui.Shared) — added `GetAttendeeDoorStatusAsync()`
- **File:** `AttendeeQrViewModel.cs` — now takes `(ApiService, AuthService, AppStateService)`; fetches door status after QR generation
- `PageBackground` ObservableProperty drives page color:
  - Dark green `#0a3a1a` — Entry (inside)
  - Dark red `#3a0a0a` — Exit (outside)
  - Dark purple `#2d0050` — Unregistered / never scanned
- **File:** `AttendeeQrPage.xaml` — `BackgroundColor="{Binding PageBackground}"`; `StatusMessage` TextColor = White

### Task 4 — Crew MAUI: Restrict login to crew roles
- **File:** `LoginViewModel.cs` — post-login role check against JWT claims
- If no Admin/Organizer/Operator role: calls `LogoutAsync()` first, then sets `ErrorMessage = "Access denied"`
- **Order:** Logout before UI feedback — prevents navigation window for unauthorized users
- UX gate only; API authorization remains the authoritative security boundary

## Technical Decisions

1. **Auto-flip at API layer** — correct placement; client cannot bypass or misconfigure it
2. **Status endpoint follows GetQrCode auth pattern** — consistent authorization model
3. **MVVM binding for QR page color** — `ObservableProperty` with `BackgroundColor` binding; no code-behind color manipulation
4. **Logout-before-error-message order** — session invalidated before any UI feedback shown

## Validation

- Build: API ✅ MAUI ✅ MAUI.Crew ✅ (0 warnings, 0 errors each)
- CI: ✅ 4/4 checks passing (API Tests, Build API, Build Frontend, GitGuardian)
- Merge: `gh pr merge 126 --squash --delete-branch --admin`
- Branch deleted: `feat/door-scan-status-sprint`
- Master HEAD advanced to `344570f`

---
**Status:** ✅ Complete  
**PR:** #126 — feat/door-scan-status-sprint → master  
