# LanManager Squad

A LAN party management platform built with .NET Aspire, React, and .NET MAUI.

## Architecture

- **AppHost** (`src/LanManager.AppHost`) — Aspire orchestration
- **ServiceDefaults** (`src/LanManager.ServiceDefaults`) — Shared telemetry, health
- **API** (`src/LanManager.Api`) — .NET 9 Web API (events, users, check-in/check-out)
- **Frontend** (`frontend/`) — React + TypeScript + Vite
- **MAUI** (`src/LanManager.Maui`) — Check-in operator app

## Running locally

```bash
dotnet run --project src/LanManager.AppHost
```

> Solution file: `LanManager.slnx` (dotnet 10 XML solution format)
