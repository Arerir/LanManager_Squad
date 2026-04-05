---
updated_at: 2026-04-05T18:45:34.000Z
focus_area: Sprint 3 — Door QR scan logging (exit/re-entry tracking)
active_issues: [27, 28, 29]
---

# What We're Focused On

Issues #1–#10 closed. Master has: data layer, full REST API, SignalR attendance board, React views, MAUI check-in app.

## Sprint 3 — Planned

| Issue | Agent | Work |
|-------|-------|------|
| #27 | Tank | DoorPassRecord model + migration, QR code generation, door scan / outside / door-log API |
| #28 | Switch | MAUI DoorScanPage — ZXing QR camera, direction toggle, scan-to-log |
| #29 | Trinity | React door log view — tabbed AttendancePage (Live / Outside Now / Door Log) |

**Dependency:** Tank (#27) ships first — Switch and Trinity consume its API endpoints.

## Post-MVP (deferred)

| Issue | Work |
|-------|------|
| #11 | Tournament bracket service |
| #12 | Interactive seating/floor map |
| #24 | Development test data seeder |
| #25 | Role-based authorization |
