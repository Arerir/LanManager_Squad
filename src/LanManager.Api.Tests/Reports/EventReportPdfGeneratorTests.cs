using LanManager.Api.Models;
using LanManager.Api.Services;
using QuestPDF.Infrastructure;
using DataModels = LanManager.Data.Models;

namespace LanManager.Api.Tests.Reports;

public class EventReportPdfGeneratorTests
{
    public EventReportPdfGeneratorTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static EventReportData MakeFullReportData() => new()
    {
        Id = Guid.NewGuid(),
        Name = "LAN Party 2026",
        Description = "Annual gaming event",
        Location = "Main Hall",
        StartDate = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
        EndDate = new DateTime(2026, 6, 2, 22, 0, 0, DateTimeKind.Utc),
        Capacity = 50,
        Status = DataModels.EventStatus.Closed,
        Registrations =
        [
            new RegistrationSummary
            {
                UserId = Guid.NewGuid(),
                UserName = "player1",
                Status = DataModels.RegistrationStatus.Confirmed,
                RegisteredAt = new DateTime(2026, 5, 1, 12, 0, 0, DateTimeKind.Utc)
            }
        ],
        CheckIns =
        [
            new CheckInSummary
            {
                UserId = Guid.NewGuid(),
                UserName = "player1",
                CheckedInAt = new DateTime(2026, 6, 1, 11, 0, 0, DateTimeKind.Utc),
                CheckedOutAt = new DateTime(2026, 6, 1, 15, 0, 0, DateTimeKind.Utc),
                Duration = TimeSpan.FromHours(4)
            }
        ]
    };

    [Fact]
    public void GeneratePdf_ReturnsNonEmptyBytes_ForEventWithAllSections()
    {
        var generator = new EventReportPdfGenerator();
        var data = MakeFullReportData();

        var bytes = generator.GeneratePdf(data);

        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void GeneratePdf_ReturnsNonEmptyBytes_ForEventWithNoOptionalSections()
    {
        var generator = new EventReportPdfGenerator();
        var data = new EventReportData
        {
            Id = Guid.NewGuid(),
            Name = "Minimal LAN",
            Location = null,
            StartDate = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 6, 2, 22, 0, 0, DateTimeKind.Utc),
            Capacity = 20,
            Status = DataModels.EventStatus.Closed,
            Registrations = null,
            CheckIns = null,
            Equipment = null,
            Tournaments = null
        };

        var bytes = generator.GeneratePdf(data);

        Assert.NotNull(bytes);
        Assert.NotEmpty(bytes);
    }

    [Fact]
    public void GeneratePdf_DoesNotThrow_WhenCheckInsHaveNullCheckOutAt()
    {
        var generator = new EventReportPdfGenerator();
        var data = new EventReportData
        {
            Id = Guid.NewGuid(),
            Name = "Open CheckIn LAN",
            StartDate = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 6, 2, 22, 0, 0, DateTimeKind.Utc),
            Capacity = 30,
            Status = DataModels.EventStatus.Closed,
            CheckIns =
            [
                new CheckInSummary
                {
                    UserId = Guid.NewGuid(),
                    UserName = "player_still_in",
                    CheckedInAt = new DateTime(2026, 6, 1, 10, 30, 0, DateTimeKind.Utc),
                    CheckedOutAt = null,
                    Duration = null
                }
            ]
        };

        var exception = Record.Exception(() => generator.GeneratePdf(data));

        Assert.Null(exception);
    }
}
