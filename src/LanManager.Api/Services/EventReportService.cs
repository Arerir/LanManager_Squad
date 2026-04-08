using LanManager.Api.Models;
using LanManager.Data;
using LanManager.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace LanManager.Api.Services;

public class EventReportService(LanManagerDbContext db)
{
    public async Task<EventReportData?> GetReportDataAsync(
        Guid eventId,
        ReportSections sections,
        CancellationToken ct = default)
    {
        IQueryable<Data.Models.Event> query = db.Events.Where(e => e.Id == eventId);

        if (sections.HasFlag(ReportSections.Registrations))
            query = query.Include(e => e.Registrations).ThenInclude(r => r.User);

        if (sections.HasFlag(ReportSections.CheckIns))
            query = query.Include(e => e.CheckInRecords).ThenInclude(c => c.User);

        var ev = await query.FirstOrDefaultAsync(ct);
        if (ev is null)
            return null;

        RegistrationSummary[]? registrations = null;
        if (sections.HasFlag(ReportSections.Registrations))
        {
            registrations = ev.Registrations
                .Select(r => new RegistrationSummary
                {
                    UserId = r.UserId,
                    UserName = r.User.UserName ?? string.Empty,
                    Status = r.Status,
                    RegisteredAt = r.RegisteredAt
                })
                .ToArray();
        }

        CheckInSummary[]? checkIns = null;
        if (sections.HasFlag(ReportSections.CheckIns))
        {
            checkIns = ev.CheckInRecords
                .Select(c => new CheckInSummary
                {
                    UserId = c.UserId,
                    UserName = c.User.UserName ?? string.Empty,
                    CheckedInAt = c.CheckedInAt,
                    CheckedOutAt = c.CheckedOutAt,
                    Duration = c.CheckedOutAt.HasValue
                        ? c.CheckedOutAt.Value - c.CheckedInAt
                        : null
                })
                .ToArray();
        }

        return new EventReportData
        {
            Id = ev.Id,
            Name = ev.Name,
            Description = ev.Description,
            Location = ev.Location,
            StartDate = ev.StartDate,
            EndDate = ev.EndDate,
            Capacity = ev.Capacity,
            Status = ev.Status,
            Registrations = registrations,
            CheckIns = checkIns,
            Equipment = null,
            Tournaments = null
        };
    }
}
