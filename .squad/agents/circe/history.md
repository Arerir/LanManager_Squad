# Project Context

- **Owner:** Daniel Eli
- **Project:** LanManager_Squad — LAN party management platform
- **Stack:** .NET Aspire orchestration, React frontend, .NET 9 Web API backend, .NET MAUI check-in/check-out apps
- **Created:** 2026-04-05

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

### 2026-04-08: MAUI shared services library

- Created LanManager.Maui.Shared to host AuthService, AuthHandler, ApiService, and Config for reuse across MAUI apps.
- LanManager.Maui now references the shared library; AppStateService remains app-specific.

