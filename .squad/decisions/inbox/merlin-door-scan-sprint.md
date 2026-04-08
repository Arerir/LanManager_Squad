# Decision: Door Scan Auto-Direction + Attendee Status API

**Date:** 2026-04-09  
**Author:** Merlin  
**PR:** https://github.com/Arerir/LanManager_Squad/pull/126

## Context

The crew door scanner was sending an explicit direction value (`Entry`/`Exit`) from the app UI. This allowed double-exits if the crew tapped the wrong direction. Attendees also had no way to see their current check-in state from the QR page.

## Decisions

### 1. Auto-flip direction after Exit (backend)
After a user's last door pass is `Exit`, the next scan is **always forced to `Entry`** regardless of what the crew app sends. This is an idempotency guarantee at the API level. The UI direction selection in the crew app is still used for the first scan and for Exit scans.

### 2. Attendee door status endpoint
`GET /api/events/{eventId}/attendees/{userId}/door-status` returns `{ "status": "Entry" | "Exit" | "Unregistered" }`. Authorization: staff can query any user; attendees can only query their own status (matches pattern from `GetQrCode`).

### 3. QR page background color = door status indicator
Rather than a separate status screen, the attendee QR page background changes color based on current scan status:
- **Dark green** (`#0a3a1a`) — Entry (checked in and inside)
- **Dark red** (`#3a0a0a`) — Exit (checked in but currently outside)
- **Dark purple** (`#2d0050`) — Unregistered / never scanned

This gives crew an instant visual confirmation when scanning without requiring them to read text.

### 4. Crew login role gate
The Crew app (`LanManager.Maui.Crew`) now enforces that only users with Admin, Organizer, or Operator roles can log in. Non-crew users are immediately logged out with a clear error message. This is done client-side after login (JWT role claims are already available) as a UX gate; the API remains the authoritative security boundary.

## Impact
- **Tank / Circe**: No API contract changes for door scan broadcast — `UserDoorScanned` SignalR event still fires with same payload.
- **Switch / Morgana**: No frontend API changes — the new status endpoint is additive.
- **Attendee MAUI**: ViewModel constructor signature changed from `(AuthService, AppStateService)` to `(ApiService, AuthService, AppStateService)`. DI registration in `MauiProgram.cs` is unchanged (auto-resolved by container).
