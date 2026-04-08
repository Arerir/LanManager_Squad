# LanManager

A comprehensive LAN party management platform with real-time attendance tracking, event organization, and comprehensive reporting.

**Key capabilities:** Event management • User registration • QR code check-in • Real-time attendance via SignalR • PDF event reports • Admin/organizer dashboard • Equipment tracking • Tournament brackets

## Architecture

### Backend (.NET 10)
- **`LanManager.Api`** — REST API (events, users, registration, check-in/check-out, door scanning, reports, equipment, tournaments)
- **`LanManager.Data`** — Entity Framework Core data layer with SQLite (dev) / SQL Server (prod)
- **`LanManager.AppHost`** — .NET Aspire orchestration
- **`LanManager.ServiceDefaults`** — Shared health checks and telemetry
- **`LanManager.Api.Tests`** — Unit and integration tests for API services

### Frontend (React)
- **`frontend/`** — React 19 + TypeScript + Vite SPA (event management, attendee dashboard, report download)
- **Playwright E2E** — Automated test suite (`npm run test:e2e`)

### Mobile & Desktop (MAUI)
- **`LanManager.Maui`** — Attendee check-in app (QR scanning, real-time status, attendance board)
- **`LanManager.Maui.Crew`** — Admin/organizer app (event management, report download with section filters, attendance tracking)
- **`LanManager.Maui.Shared`** — Shared services, API client, and models

## Features

### Event Management
- Create, edit, and close events
- Track event status (open, closed)
- Event detail view with attendee list

### User Registration & Check-in
- User registration with email/password
- QR code generation for each attendee per event
- Attendee check-in/check-out tracking (via mobile app or API)

### Real-Time Attendance
- SignalR hub broadcasts check-in/check-out events
- Live attendance board with user status
- Real-time occupancy tracking via door scanning

### Door Pass Management
- QR-based entry/exit scanning for venue access
- Door log with timestamp audit trail
- Real-time outside user list for occupancy reporting

### PDF Event Reports
- Multi-section report generation (Registrations, Check-ins, Equipment, Tournaments)
- Section-level toggles for report customization
- Download/share reports from admin dashboard and Crew app

### Authentication
- JWT-based auth for all APIs
- Role-based access (admin/organizer vs. attendee)
- Secure token refresh mechanism

### Additional Features
- Equipment tracking and loan management
- Tournament bracket service with live standings
- Seating/floor map planning
- Real-time notifications via SignalR

## Getting Started

### Prerequisites
- **.NET 10 SDK** (https://dotnet.microsoft.com/download)
- **Node.js 18+** (https://nodejs.org/)
- **SQLite** (included with .NET tooling)

### Running Locally

**1. Start the API**
```bash
dotnet run --project src/LanManager.Api
```
The API will start on `https://localhost:5000` (or configure via `appsettings.Development.json`).

**2. Start the Frontend (in a separate terminal)**
```bash
cd frontend
npm install
npm run dev
```
The frontend will be available at `http://localhost:5173`.

**3. (Optional) Start the Attendee Mobile App**
```bash
dotnet build src/LanManager.Maui
```
Then run from Visual Studio or `dotnet run --project src/LanManager.Maui`.

**4. (Optional) Start the Crew App**
```bash
dotnet build src/LanManager.Maui.Crew
```
Then run from Visual Studio or `dotnet run --project src/LanManager.Maui.Crew`.

### Database Setup
The API automatically applies EF Core migrations on startup. To manually migrate:
```bash
dotnet ef database update --project src/LanManager.Data --startup-project src/LanManager.Api
```

To create a new migration:
```bash
dotnet ef migrations add <MigrationName> --project src/LanManager.Data --startup-project src/LanManager.Api
```

## Testing

### API Tests
```bash
dotnet test src/LanManager.Api.Tests
```

### Frontend E2E Tests
```bash
cd frontend
npm run dev  # Must be running on port 5173
# In another terminal:
npm run test:e2e
```

### Linting
**Frontend:**
```bash
cd frontend
npm run lint
```

## Tech Stack

### Backend
- **.NET 10** — Runtime
- **ASP.NET Core** — Web framework
- **Entity Framework Core** — ORM
- **SQLite** — Development database (SQL Server for production)
- **JWT Bearer** — Authentication
- **SignalR** — Real-time communication
- **QuestPDF** — PDF generation
- **QRCoder** — QR code generation

### Frontend
- **React 19** — UI library
- **TypeScript** — Type safety
- **Vite** — Build tool (HMR, fast builds)
- **React Router v7** — Client-side routing
- **SignalR** — Real-time client
- **Playwright** — E2E testing

### Mobile/Desktop (MAUI)
- **.NET MAUI** — Cross-platform framework
- **MVVM Toolkit** — ViewModel architecture
- **Entity Framework Core** — Local data access

## API Endpoints

### Authentication
- `POST /api/auth/login` — Login with credentials

### Events
- `GET /api/events` — List all events
- `POST /api/events` — Create event
- `GET /api/events/{id}` — Get event details
- `PUT /api/events/{id}` — Update event
- `DELETE /api/events/{id}` — Delete event
- `POST /api/events/{id}/close` — Close event

### Users & Registration
- `POST /api/users/register` — Register new user
- `GET /api/users/{id}` — Get user profile
- `POST /api/events/{id}/register` — Register user for event
- `GET /api/events/{id}/attendees` — List event attendees

### Check-in/Check-out
- `POST /api/events/{id}/checkin` — Check in user
- `POST /api/events/{id}/checkout` — Check out user
- `GET /api/events/{id}/attendance` — Get attendance status

### Door Scanning
- `GET /api/events/{id}/attendees/{userId}/qrcode` — Get attendee QR code
- `POST /api/events/{id}/door-scan` — Scan door pass (entry/exit)
- `GET /api/events/{id}/door-log` — Get door scan log
- `GET /api/events/{id}/outside` — Get currently outside users

### Reports
- `GET /api/events/{id}/report` — Download PDF event report

### Equipment
- `GET /api/events/{id}/equipment` — List event equipment
- `POST /api/events/{id}/equipment` — Create equipment
- `POST /api/events/{id}/equipment/{id}/loan` — Loan equipment
- `POST /api/events/{id}/equipment/{id}/return` — Return equipment

### Tournaments
- `POST /api/events/{id}/tournaments` — Create tournament
- `GET /api/events/{id}/tournaments/{id}/bracket` — Get tournament bracket

### Seating
- `GET /api/events/{id}/seats` — Get seating map
- `POST /api/events/{id}/seats` — Reserve seat

## Real-Time Features (SignalR)

**Hub:** `/hubs/attendance`

**Broadcast Events:**
- `UserCheckedIn` — User checked in to event (includes attendance snapshot)
- `UserCheckedOut` — User checked out from event
- `DoorScanned` — Door pass scanned (entry/exit event)

## Development Notes

- **API documentation:** OpenAPI/Swagger available at `https://localhost:5000/openapi/v1.json`
- **Frontend environment:** Configure `VITE_API_URL` in `.env.development` (defaults to `http://localhost:5000`)
- **MAUI apps:** Use shared `ApiService` singleton for HTTP calls; ViewModels follow MVVM pattern with `CommunityToolkit.Mvvm`
- **Theme system:** Frontend uses CSS variables for GameVille cyberpunk theme (editable in `src/css/theme.css`)

## Project Structure

```
LanManager_Squad/
├── src/
│   ├── LanManager.Api/              # REST API
│   ├── LanManager.Data/             # EF Core models & DbContext
│   ├── LanManager.Maui/             # Attendee mobile/desktop app
│   ├── LanManager.Maui.Crew/        # Admin/organizer app
│   ├── LanManager.Maui.Shared/      # Shared MAUI services
│   ├── LanManager.AppHost/          # Aspire orchestration
│   └── LanManager.Api.Tests/        # API test suite
├── frontend/                        # React SPA
├── LanManager.slnx                  # Solution file (dotnet 10)
└── README.md                        # This file
```

## Contributing

When adding features:
1. Update API controllers and services in `src/LanManager.Api/`
2. Add/update data models in `src/LanManager.Data/Models/`
3. Create EF Core migrations for schema changes
4. Update frontend pages/components in `frontend/src/pages/` and `frontend/src/components/`
5. Add tests for new API services
6. Update Playwright E2E tests if UI changes
7. Test locally before opening a PR

## License

This project is open source. See LICENSE file for details.
