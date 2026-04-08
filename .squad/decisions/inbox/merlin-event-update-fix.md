# Fix: JSON Deserialization Error on Event Update Endpoint

**Date:** 2026-04-09  
**Author:** Merlin  
**Branch:** squad/fix-event-update-dto

## Root Cause

`UpdateEventRequest.Status` (and `CreateEventRequest.Status`) are typed as `EventStatus` — a C# enum.  
By default, `System.Text.Json` (used by ASP.NET Core) deserializes enums as **integers**.  
The React frontend sends the enum value as a **string** (e.g. `"Active"`, `"Draft"`).

This mismatch caused a 400/deserialization error whenever the frontend called `PUT /api/events/{id}`.

## Fix

Added `JsonStringEnumConverter` globally in `Program.cs`:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
```

This enables both:
- **Deserialisation** of incoming string enum values (`"Active"` → `EventStatus.Active`)
- **Serialisation** of outgoing enum values as strings (consistent with how `EventDto.Status` was already returned as `.ToString()`)

## Files Changed

- `src/LanManager.Api/Program.cs` — added `using System.Text.Json.Serialization;` and `.AddJsonOptions(...)` to `AddControllers()`

## Verification

`dotnet build` clean; all 62 unit tests pass.

## Impact

Affects **both** `POST /api/events` (create) and `PUT /api/events/{id}` (update) since both DTOs had the same issue. No migration needed; no schema change.
