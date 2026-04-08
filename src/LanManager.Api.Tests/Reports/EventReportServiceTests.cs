// TODO: This file is anticipatory scaffolding for issue #103.
// EventReportService, ReportSections, and EventReportData are being implemented by Merlin.
// Once those types exist, minor adjustments to usings/type names may be needed, but
// the structure and test cases below are final.

using LanManager.Api.Services;
using LanManager.Api.Tests.Helpers;
using LanManager.Data.Models;
using ApiModels = LanManager.Api.Models;

namespace LanManager.Api.Tests.Reports;

public class EventReportServiceTests
{
    // ──────────────────────────────────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private static ApplicationUser MakeUser(string name = "Test User") =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserName = name.Replace(" ", "").ToLower(),
            Email = $"{name.Replace(" ", "").ToLower()}@test.com",
            NormalizedEmail = $"{name.Replace(" ", "").ToUpper()}@TEST.COM",
            NormalizedUserName = name.Replace(" ", "").ToUpper()
        };

    private static Event MakeEvent(
        string name = "LAN Party 2026",
        string? location = "Basement LAN Room") =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            Location = location,
            StartDate = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 6, 2, 22, 0, 0, DateTimeKind.Utc),
            Status = EventStatus.Active,
            Capacity = 50
        };

    // ──────────────────────────────────────────────────────────────────────────
    //  Tests
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetReportDataAsync_ReturnsNull_WhenEventNotFound()
    {
        using var db = TestDbContextFactory.Create("Report_EventNotFound");
        var svc = new EventReportService(db);

        var result = await svc.GetReportDataAsync(Guid.NewGuid(), ApiModels.ReportSections.All);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetReportDataAsync_ReturnsEventMeta_ForAllSections()
    {
        using var db = TestDbContextFactory.Create("Report_EventMeta");
        var ev = MakeEvent("Summer LAN 2026", "Server Room B");
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var svc = new EventReportService(db);
        var result = await svc.GetReportDataAsync(ev.Id, ApiModels.ReportSections.All);

        Assert.NotNull(result);
        Assert.Equal(ev.Name, result.EventName);
        Assert.Equal(ev.StartDate, result.StartDate);
        Assert.Equal(ev.EndDate, result.EndDate);
        Assert.Equal(ev.Location, result.Location);
    }

    [Fact]
    public async Task GetReportDataAsync_PopulatesRegistrations_WhenSectionRequested()
    {
        using var db = TestDbContextFactory.Create("Report_Registrations_Populated");
        var ev = MakeEvent();
        var user = MakeUser();
        db.Events.Add(ev);
        db.Users.Add(user);
        db.Registrations.Add(new Registration
        {
            EventId = ev.Id,
            UserId = user.Id,
            Status = RegistrationStatus.Confirmed
        });
        await db.SaveChangesAsync();

        var svc = new EventReportService(db);
        var result = await svc.GetReportDataAsync(ev.Id, ApiModels.ReportSections.Registrations);

        Assert.NotNull(result);
        Assert.NotNull(result.Registrations);
        Assert.True(result.Registrations.Count > 0);
    }

    [Fact]
    public async Task GetReportDataAsync_DoesNotPopulateRegistrations_WhenSectionNotRequested()
    {
        using var db = TestDbContextFactory.Create("Report_Registrations_NotPopulated");
        var ev = MakeEvent();
        var user = MakeUser();
        db.Events.Add(ev);
        db.Users.Add(user);
        db.Registrations.Add(new Registration
        {
            EventId = ev.Id,
            UserId = user.Id,
            Status = RegistrationStatus.Confirmed
        });
        await db.SaveChangesAsync();

        var svc = new EventReportService(db);
        // Request CheckIns only — Registrations section is NOT included
        var result = await svc.GetReportDataAsync(ev.Id, ApiModels.ReportSections.CheckIns);

        Assert.NotNull(result);
        Assert.Null(result.Registrations);
    }

    [Fact]
    public async Task GetReportDataAsync_PopulatesCheckIns_WhenSectionRequested()
    {
        using var db = TestDbContextFactory.Create("Report_CheckIns_Populated");
        var ev = MakeEvent();
        var user = MakeUser();
        db.Events.Add(ev);
        db.Users.Add(user);
        db.CheckInRecords.Add(new CheckInRecord
        {
            EventId = ev.Id,
            UserId = user.Id,
            CheckedInAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var svc = new EventReportService(db);
        var result = await svc.GetReportDataAsync(ev.Id, ApiModels.ReportSections.CheckIns);

        Assert.NotNull(result);
        Assert.NotNull(result.CheckIns);
        Assert.True(result.CheckIns.Count > 0);
    }

    [Fact]
    public async Task GetReportDataAsync_CalculatesDuration_ForCompletedCheckIns()
    {
        using var db = TestDbContextFactory.Create("Report_Duration_Completed");
        var ev = MakeEvent();
        var user = MakeUser();
        var checkedIn = new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc);
        var checkedOut = new DateTime(2026, 6, 1, 14, 30, 0, DateTimeKind.Utc);
        var expectedDuration = checkedOut - checkedIn;

        db.Events.Add(ev);
        db.Users.Add(user);
        db.CheckInRecords.Add(new CheckInRecord
        {
            EventId = ev.Id,
            UserId = user.Id,
            CheckedInAt = checkedIn,
            CheckedOutAt = checkedOut
        });
        await db.SaveChangesAsync();

        var svc = new EventReportService(db);
        var result = await svc.GetReportDataAsync(ev.Id, ApiModels.ReportSections.CheckIns);

        Assert.NotNull(result);
        Assert.NotNull(result.CheckIns);
        var checkIn = Assert.Single(result.CheckIns);
        Assert.NotNull(checkIn.Duration);
        Assert.Equal(expectedDuration, checkIn.Duration);
    }

    [Fact]
    public async Task GetReportDataAsync_DurationIsNull_ForOpenCheckIns()
    {
        using var db = TestDbContextFactory.Create("Report_Duration_Open");
        var ev = MakeEvent();
        var user = MakeUser();

        db.Events.Add(ev);
        db.Users.Add(user);
        db.CheckInRecords.Add(new CheckInRecord
        {
            EventId = ev.Id,
            UserId = user.Id,
            CheckedInAt = DateTime.UtcNow,
            CheckedOutAt = null
        });
        await db.SaveChangesAsync();

        var svc = new EventReportService(db);
        var result = await svc.GetReportDataAsync(ev.Id, ApiModels.ReportSections.CheckIns);

        Assert.NotNull(result);
        Assert.NotNull(result.CheckIns);
        var checkIn = Assert.Single(result.CheckIns);
        Assert.Null(checkIn.Duration);
    }
}
