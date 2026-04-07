# Test Coverage Expansion — May 2026

**Date:** 2026-05-02  
**Author:** Apoc (Tester)  
**Status:** Completed

## Summary

Expanded test coverage for LanManager.Api from **14.45% to 37.65% line coverage** (160% relative increase). Added comprehensive tests for previously untested controllers (Events, Users, Registrations, Auth, Tournament) and expanded existing tests (Seats, CheckIn, DoorPass, Equipment). All 77 tests passing.

## Metrics

### Before
- Line coverage: 14.45%
- Branch coverage: 8.46%
- Method coverage: 14.95%

### After
- Line coverage: 37.65% (+23.2pp, +160% relative)
- Branch coverage: 25.78% (+17.32pp, +204% relative)
- Tests: 77 total, 0 failed

## What Was Covered

### New Test Files (5)
1. **AuthControllerTests** — Login validation, token generation, invalid credentials, multiple roles
2. **EventsControllerTests** — CRUD operations, filtering by status, sorting, validation, not-found cases
3. **RegistrationsControllerTests** — Event registration, capacity checks, duplicate registration, attendee listing
4. **UsersControllerTests** — User registration, password validation, GetById, pagination
5. **TournamentControllerTests** — GetAll, Create, GetBracket, SubmitResult (validation, completion, invalid winner)

### Expanded Test Files (4)
6. **CheckInControllerTests** — Added GetAttendance tests (active vs checked-out, empty event, not-found)
7. **DoorPassControllerTests** — Added GetQrCode, GetDoorLog tests
8. **EquipmentControllerTests** — Added GetAll, GetById, Create, Update tests (CRUD completion)
9. **SeatsControllerTests** — Added GetAll, CreateGrid, Assign, Unassign tests (full CRUD coverage)

## Test Patterns Established

- **In-memory EF DbContext** via `TestDbContextFactory.Create(dbName)` for isolated test databases
- **Mock UserManager** for identity operations (FindByIdAsync, CreateAsync, CheckPasswordAsync, GetRolesAsync)
- **Mock SignalR hubs** (AttendanceHub, TournamentHub) to avoid real-time dependencies
- **ClaimsHelper.SetUser** to inject authentication context (userId, roles)
- **Comprehensive assertions** for HTTP status codes (200, 201, 400, 401, 404, 409), DTOs, and database state

## What Remains

### Controllers with Low/Zero Coverage
- **DevController** (0%) — Seed endpoint not tested (intentionally — dev-only)
- **Some edge cases in covered controllers** — e.g., Forbid() paths in DoorPassController, some SignalR broadcast scenarios

### Suggested Next Steps
1. **Set coverage threshold in CI** — Enforce 35% line coverage minimum (below current 37.65% will fail)
2. **Integration tests** — Test full request pipelines with TestServer (auth middleware, model validation, error handling)
3. **Authorization tests** — Verify [Authorize] attributes block unauthorized access (currently tests assume auth works)
4. **Async/concurrency edge cases** — Race conditions in check-in/check-out, tournament match updates

## Lessons Learned

- **Cobertura XML analysis** is invaluable for identifying zero-coverage methods — grep for `line-rate="0"` and class names
- **Mock UserManager carefully** — EF's `IAsyncQueryProvider` breaks when mocking `Users` IQueryable. Use real DbContext or skip those tests.
- **BracketService** needed concrete instantiation, not mock, because it has logic tests depend on
- **Test naming convention** — `{Method}_{Scenario}_{ExpectedResult}` keeps tests readable and discoverable

## Related Commits

- `5dab421` — test: expand coverage — added comprehensive tests for Events, Users, Registrations, Auth, Tournament, Seats, CheckIn, DoorPass, Equipment controllers
