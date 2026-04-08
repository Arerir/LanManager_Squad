using LanManager.Api.Controllers;
using LanManager.Api.DTOs;
using LanManager.Api.Hubs;
using LanManager.Api.Tests.Helpers;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace LanManager.Api.Tests;

public class DoorPassControllerTests
{
    private static UserManager<ApplicationUser> MockUserManager(ApplicationUser? returnUser = null)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(returnUser);
        return mgr.Object;
    }

    private static IHubContext<AttendanceHub> MockHubContext()
    {
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
        
        var mockHubContext = new Mock<IHubContext<AttendanceHub>>();
        mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
        
        return mockHubContext.Object;
    }

    [Fact]
    public async Task DoorScan_WhenNotCheckedIn_AutoChecksInAndReturnsEntry()
    {
        using var db = TestDbContextFactory.Create("DoorScan_AutoCheckIn");
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "Test Event",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        var user = new ApplicationUser
        {
            Id = userId, UserName = "dooruser", Name = "Door User",
            Email = "d@test.com", NormalizedEmail = "D@TEST.COM", NormalizedUserName = "DOORUSER"
        };
        db.Users.Add(user);
        db.Registrations.Add(new Registration
        {
            EventId = eventId, UserId = userId, Status = RegistrationStatus.Confirmed
        });
        await db.SaveChangesAsync();

        var controller = new DoorPassController(db, MockUserManager(user), MockHubContext());
        ClaimsHelper.SetUser(controller, userId, "Operator");

        var result = await controller.DoorScan(eventId, new DoorScanRequest(userId, "Entry"));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<DoorScanResultDto>(created.Value);
        Assert.True(dto.WasAutoCheckedIn);
        Assert.Equal("Entry", dto.DoorPass.Direction);
    }

    [Fact]
    public async Task DoorScan_WhenCheckedIn_LogsPassWithRequestedDirection()
    {
        using var db = TestDbContextFactory.Create("DoorScan_AlreadyCheckedIn");
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "Test Event",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        var user = new ApplicationUser
        {
            Id = userId, UserName = "dooruser2", Name = "Door User 2",
            Email = "d2@test.com", NormalizedEmail = "D2@TEST.COM", NormalizedUserName = "DOORUSER2"
        };
        db.Users.Add(user);
        db.CheckInRecords.Add(new CheckInRecord { EventId = eventId, UserId = userId });
        await db.SaveChangesAsync();

        var controller = new DoorPassController(db, MockUserManager(user), MockHubContext());
        ClaimsHelper.SetUser(controller, userId, "Operator");

        var result = await controller.DoorScan(eventId, new DoorScanRequest(userId, "Exit"));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<DoorScanResultDto>(created.Value);
        Assert.False(dto.WasAutoCheckedIn);
        Assert.Equal("Exit", dto.DoorPass.Direction);
    }

    [Fact]
    public async Task GetOutside_ReturnsOnlyUsersWhoseLastPassIsExit()
    {
        using var db = TestDbContextFactory.Create("DoorScan_GetOutside");
        var eventId = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "Test Event",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Users.Add(new ApplicationUser
        {
            Id = userId1, UserName = "outside", Name = "Outside",
            Email = "out@t.com", NormalizedEmail = "OUT@T.COM", NormalizedUserName = "OUTSIDE"
        });
        db.Users.Add(new ApplicationUser
        {
            Id = userId2, UserName = "inside", Name = "Inside",
            Email = "in@t.com", NormalizedEmail = "IN@T.COM", NormalizedUserName = "INSIDE"
        });
        // user1: Entry then Exit → last pass is Exit → outside
        db.DoorPasses.Add(new DoorPassRecord
        {
            EventId = eventId, UserId = userId1,
            Direction = DoorPassDirection.Entry,
            ScannedAt = DateTime.UtcNow.AddMinutes(-10)
        });
        db.DoorPasses.Add(new DoorPassRecord
        {
            EventId = eventId, UserId = userId1,
            Direction = DoorPassDirection.Exit,
            ScannedAt = DateTime.UtcNow.AddMinutes(-5)
        });
        // user2: Entry → last pass is Entry → not outside
        db.DoorPasses.Add(new DoorPassRecord
        {
            EventId = eventId, UserId = userId2,
            Direction = DoorPassDirection.Entry,
            ScannedAt = DateTime.UtcNow.AddMinutes(-8)
        });
        await db.SaveChangesAsync();

        var controller = new DoorPassController(db, MockUserManager(), MockHubContext());
        ClaimsHelper.SetUser(controller, userId1, "Operator");

        var result = await controller.GetOutside(eventId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var outside = Assert.IsAssignableFrom<List<OutsideUserDto>>(ok.Value);
        Assert.Single(outside);
        Assert.Equal(userId1, outside[0].UserId);
    }
}
