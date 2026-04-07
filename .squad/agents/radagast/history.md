# Project Context

- **Owner:** Daniel Eli
- **Project:** LanManager_Squad — LAN party management platform
- **Stack:** .NET Aspire orchestration, React frontend, .NET 9 Web API backend, .NET MAUI check-in/check-out apps
- **Created:** 2026-04-05

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-08: Playwright E2E Infrastructure Setup (#71)
**What:** Added Playwright config and e2e scaffolding in `frontend/`, including auth fixture (email/password selectors) and smoke tests. Tests run via `npm run test:e2e` and expect login to redirect to `/events`.

### 2026-04-09: Auth flow Playwright coverage (#72)
**What:** Added auth flow E2E checks for login form fields, invalid credentials, and unauthenticated route redirects. Valid-credential login is skipped pending a live API.

### 2026-04-10: Events Playwright flow tests (#73)
**What:** Added event flow E2E coverage for `/events`, detail navigation, unknown ids, and localStorage context persistence, with skips for missing backend data.

### 2026-04-10: Attendance Playwright coverage (#74)
**What:** Added attendance E2E checks for rendering without an event, rendering with an eventId, and outside-tab empty state with mocked API responses.

### 2026-04-10: Profile Page E2E Coverage (#76)
**What:** Added Playwright coverage for the profile page to validate user info rendering, sidebar navigation access, and sign-out redirect behavior.


