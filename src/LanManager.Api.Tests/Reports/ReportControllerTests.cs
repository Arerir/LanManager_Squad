using LanManager.Api.Controllers;
using LanManager.Api.Services;
using LanManager.Api.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Infrastructure;
using DataModels = LanManager.Data.Models;

namespace LanManager.Api.Tests.Reports;

public class ReportControllerTests
{
    public ReportControllerTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static DataModels.ApplicationUser MakeUser(string name = "Player One") => new()
    {
        Id = Guid.NewGuid(),
        Name = name,
        UserName = name.Replace(" ", "").ToLower(),
        Email = $"{name.Replace(" ", "").ToLower()}@test.com",
        NormalizedEmail = $"{name.Replace(" ", "").ToUpper()}@TEST.COM",
        NormalizedUserName = name.Replace(" ", "").ToUpper()
    };

    private static ReportController MakeController(
        LanManager.Data.LanManagerDbContext db,
        Guid? userId = null,
        params string[] roles)
    {
        var service = new EventReportService(db);
        var generator = new EventReportPdfGenerator();
        var controller = new ReportController(service, generator);
        ClaimsHelper.SetUser(controller, userId ?? Guid.NewGuid(), roles);
        return controller;
    }

    [Fact]
    public async Task DownloadReport_Returns404_WhenEventNotFound()
    {
        using var db = TestDbContextFactory.Create("Report_Controller_404");
        var controller = MakeController(db, roles: ["Admin"]);

        var result = await controller.DownloadReport(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DownloadReport_Returns422_WhenEventNotClosed()
    {
        using var db = TestDbContextFactory.Create("Report_Controller_422");
        var ev = new DataModels.Event
        {
            Id = Guid.NewGuid(),
            Name = "Active LAN",
            StartDate = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 6, 2, 22, 0, 0, DateTimeKind.Utc),
            Status = DataModels.EventStatus.Active,
            Capacity = 50
        };
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var controller = MakeController(db, roles: ["Admin"]);
        var result = await controller.DownloadReport(ev.Id);

        Assert.IsType<UnprocessableEntityObjectResult>(result);
    }

    [Fact]
    public async Task DownloadReport_Returns200WithPdf_ForClosedEvent_AsAdmin()
    {
        using var db = TestDbContextFactory.Create("Report_Controller_200_Admin");
        var user = MakeUser();
        var ev = new DataModels.Event
        {
            Id = Guid.NewGuid(),
            Name = "Closed LAN 2026",
            StartDate = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 6, 2, 22, 0, 0, DateTimeKind.Utc),
            Status = DataModels.EventStatus.Closed,
            Capacity = 50
        };
        db.Users.Add(user);
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var controller = MakeController(db, userId: user.Id, roles: ["Admin"]);
        var result = await controller.DownloadReport(ev.Id, sections: "All");

        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("application/pdf", fileResult.ContentType);
        Assert.NotEmpty(fileResult.FileContents);
        Assert.EndsWith("-report.pdf", fileResult.FileDownloadName);
    }

    [Fact]
    public async Task DownloadReport_Returns400_ForInvalidSectionsParam()
    {
        using var db = TestDbContextFactory.Create("Report_Controller_400_BadSection");
        var ev = new DataModels.Event
        {
            Id = Guid.NewGuid(),
            Name = "Some LAN",
            StartDate = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2026, 6, 2, 22, 0, 0, DateTimeKind.Utc),
            Status = DataModels.EventStatus.Closed,
            Capacity = 50
        };
        db.Events.Add(ev);
        await db.SaveChangesAsync();

        var controller = MakeController(db, roles: ["Admin"]);
        var result = await controller.DownloadReport(ev.Id, sections: "InvalidSection");

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact(Skip = "[Authorize] is not enforced in unit tests without middleware — auth tested at integration/E2E level")]
    public async Task DownloadReport_Returns403_ForNonAdminUser()
    {
        // This test requires HTTP middleware to enforce [Authorize(Roles = "Admin,Organizer")].
        // Direct controller instantiation bypasses auth attributes.
        await Task.CompletedTask;
    }
}
