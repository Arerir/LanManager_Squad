# Project Context

- **Owner:** Daniel Eli
- **Project:** LanManager_Squad — LAN party management platform
- **Stack:** .NET Aspire orchestration, React frontend, .NET 9 Web API backend, .NET MAUI check-in/check-out apps
- **Created:** 2026-04-05

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-09: Dashboard Playwright Coverage (#75)
**What:** Added dashboard E2E checks for authenticated rendering, welcome guidance copy, and sidebar navigation link targets using localStorage auth bypass.

### 2026-04-08: Playwright E2E Infrastructure Setup (#71)
**What:** Added Playwright config and e2e scaffolding in `frontend/`, including auth fixture (email/password selectors) and smoke tests. Tests run via `npm run test:e2e` and expect login to redirect to `/events`.
