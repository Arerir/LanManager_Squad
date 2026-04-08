# Session Log: 2026-04-07T15-26-09 — SignalR + EventContext Integration

**Timestamp:** 2026-04-07T15:26:09 UTC  
**Topic:** signalr-eventcontext  
**Agents:** Morgana (Frontend), Merlin (Backend), Circe (MAUI)

## What Happened

Three agents completed concurrent work to integrate SignalR door scan notifications across the stack:

1. **Morgana** — EventContext persistence (Frontend)
   - React Context + localStorage to carry event ID across navigation
   - Nav links dynamically include ?eventId= when set
   - 5 pages updated to sync context

2. **Merlin** — DoorScanBroadcast (Backend)
   - Added DoorScanBroadcast record
   - Wired IHubContext<AttendanceHub> into DoorPassController
   - Broadcasts "UserDoorScanned" after each door scan

3. **Circe** — SignalRService + ViewModel (MAUI)
   - Singleton SignalRService with JWT auth
   - AttendeeHubViewModel wires UserCheckedIn + UserDoorScanned
   - Auto-clearing status messages (4s)

## Key Decisions

- **Event Context:** localStorage key `selectedEventId`, wraps BrowserRouter
- **SignalR Shape:** DoorScanBroadcast with EventId, UserId, UserName, Direction, ScannedAt
- **MAUI Pattern:** Singleton service, transient ViewModel, reconnect on load
- **Filtering:** Both eventId AND userId to prevent cross-user notifications

## Verification

- All builds clean ✓
- TypeScript/C# zero errors ✓
- Follows existing patterns (currentUser localStorage, CheckInController broadcast) ✓

## Decisions Merged

- morgana-event-context.md
- merlin-doorscan-signalr.md
- circe-signalr-attendee.md

