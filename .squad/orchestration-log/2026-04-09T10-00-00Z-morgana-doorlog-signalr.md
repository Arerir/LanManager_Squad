# Orchestration Log: morgana-doorlog-signalr

**Date:** 2026-04-09T10:00:00Z  
**Agent:** Morgana  
**Manifest Entry:** PR #125 merged — DoorLog SignalR live updates

## Summary

Morgana delivered real-time SignalR live updates for the DoorLog tab in the React frontend. New door scans broadcast by the backend via `UserDoorScanned` on `/hubs/attendance` now appear instantly in the DoorLog tab without page refresh or polling, using the same HubConnectionBuilder pattern already established in `LiveAttendanceTab`.

## Deliverables

### Components / Files Changed
- **`DoorLogTab` (frontend)** — Wired to SignalR using identical pattern from `LiveAttendanceTab`
  - `DoorScanBroadcast` TypeScript interface: `{ eventId, userId, userName, direction, scannedAt }` — fully typed, no `any`
  - Per-tab `HubConnectionBuilder` connection (not shared singleton)
  - `useEffect` cleanup: `return () => { connection.stop(); }` — no leaks on unmount or `eventId` change
  - Event scoping guard: `if (payload.eventId !== eventId) return` — prevents cross-event SignalR bleed
  - Prepends new entries (newest-first); resets pagination to page 1 on new broadcast

### UX Changes
- Direction column replaced with colored pill badges: **Entry = green (`#27ae60`)**, **Exit = red (`#c0392b`)**
- Hub status badge: Live / Reconnecting... / Disconnected with `withAutomaticReconnect()` + all three state handlers

### Synthetic Key Strategy
- React key for broadcast rows: `userId + scannedAt` — no collision with REST-fetched rows; duplicates prevented by `eventId` guard

## Technical Decisions

1. **Per-tab connection (not singleton)** — matches established `LiveAttendanceTab` pattern; avoids shared state
2. **Entry=green, Exit=red** — consistent with seating card color semantics from PR #119
3. **Synthetic id for broadcast rows** — `userId + scannedAt` is unique enough given `eventId` guard
4. **Hub status badge** — mirrors pattern from attendance hub badge on CheckIn tab

## Validation

- CI: ✅ 4/4 checks passing (API Tests, Build API, Build Frontend, GitGuardian)
- Merge: `gh pr merge 125 --squash --delete-branch --admin`
- Branch deleted: `feat/doorlog-signalr-frontend`

---
**Status:** ✅ Complete  
**PR:** #125 — feat/doorlog-signalr-frontend → master  
