# SignalR Architecture — Real-Time Attendance (Issue #10)

**By:** Tank + Trinity  
**PRs:** #21 (backend), #22 (frontend)

---

## Hub Route

```
/hubs/attendance
```

Mapped in `Program.cs` via `app.MapHub<AttendanceHub>("/hubs/attendance")`.

---

## SignalR Event Names

| Event Name | Direction | Trigger |
|---|---|---|
| `UserCheckedIn` | Server → All clients | POST /api/events/{id}/checkin succeeds |
| `UserCheckedOut` | Server → All clients | POST /api/events/{id}/checkout succeeds |

No client-to-server methods — the hub is broadcast-only for now.

---

## Broadcast DTO Shapes

```csharp
// Sent with UserCheckedIn
record AttendanceBroadcast(Guid EventId, Guid UserId, string UserName, DateTime CheckedInAt);

// Sent with UserCheckedOut
record CheckOutBroadcast(Guid EventId, Guid UserId, string UserName, DateTime CheckedOutAt);
```

Defined in `src/LanManager.Api/Hubs/AttendanceBroadcast.cs`.

---

## CORS Configuration for SignalR

SignalR requires `AllowCredentials()`. The default CORS policy is:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173") // Vite dev server
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});
```

`app.UseCors()` is placed **before** `app.UseAuthorization()` in the middleware pipeline.

For production, replace `http://localhost:5173` with the deployed frontend origin.

---

## Frontend Hub URL Pattern

```ts
const connection = new HubConnectionBuilder()
  .withUrl(`${config.apiUrl}/hubs/attendance`)
  .withAutomaticReconnect()
  .build();
```

`config.apiUrl` resolves from `VITE_API_URL` env var (defaults to `http://localhost:5000`).  
The hub URL is therefore: `http://localhost:5000/hubs/attendance` in dev.

---

## Attendance API

Initial attendance snapshot is fetched from the existing REST endpoint:

```
GET /api/events/{eventId}/attendance
```

Returns `AttendanceDto[]` — users currently checked in (no `checkedOutAt`).

---

## Frontend Scoping

`AttendancePage` reads `?eventId=<guid>` from the URL query string.  
If no `eventId` is present, the page prompts the user to select an event.  
The SignalR handler filters incoming broadcasts to the current `eventId` when one is set.
