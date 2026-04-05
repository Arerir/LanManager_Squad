# Project Context

- **Owner:** Daniel Eli
- **Project:** LanManager_Squad — LAN party management platform
- **Stack:** .NET Aspire orchestration, React frontend, .NET 9 Web API backend, .NET MAUI check-in/check-out apps
- **Created:** 2026-04-05

## Learnings

<!-- Append new learnings below. Each entry is something lasting about the project. -->

## Issue #2 — EF Core data models (2026-04-05)
Created Event, User, Registration, CheckInRecord entities. LanManagerDbContext in src/LanManager.Api/Data/. SQLite dev DB. Unique index on Registration(EventId, UserId). InitialCreate migration created. PR opened.
