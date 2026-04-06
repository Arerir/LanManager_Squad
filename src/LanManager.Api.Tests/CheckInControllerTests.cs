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

public class CheckInControllerTests
{
    private static UserManager<ApplicationUser> MockUserManager(ApplicationUser? returnUser = null)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(returnUser);
        return mgr.Object;
    }

    private static IHubContext<AttendanceHub> MockHub()
    {
        var hub = new Mock<IHubContext<AttendanceHub>>();
        var clients = new Mock<IHubClients>();
        var client = new Mock<IClientProxy>();
        clients.Setup(c => c.All).Returns(client.Object);
        client.Setup(c => c.SendCoreAsync(
                It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        hub.Setup(h => h.Clients).Returns(clients.Object);
        return hub.Object;
    }

    [Fact]
    public async Task CheckIn_RegisteredUser_ReturnsCreated()
    {
        using var db = TestDbContextFactory.Create("CheckIn_OK");
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
            Id = userId, UserName = "testuser", Name = "Test User",
            Email = "t@test.com", NormalizedEmail = "T@TEST.COM", NormalizedUserName = "TESTUSER"
        };
        db.Users.Add(user);
        db.Registrations.Add(new Registration
        {
            EventId = eventId, UserId = userId, Status = RegistrationStatus.Confirmed
        });
        await db.SaveChangesAsync();

        var controller = new CheckInController(db, MockUserManager(user), MockHub());
        ClaimsHelper.SetUser(controller, userId, "Operator");

        var result = await controller.CheckIn(eventId, new CheckInRequest(userId));

        Assert.IsType<CreatedAtActionResult>(result.Result);
    }

    [Fact]
    public async Task CheckIn_AlreadyCheckedIn_ReturnsConflict()
    {
        using var db = TestDbContextFactory.Create("CheckIn_Conflict");
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
            Id = userId, UserName = "testuser", Name = "Test User",
            Email = "t@test.com", NormalizedEmail = "T@TEST.COM", NormalizedUserName = "TESTUSER"
        };
        db.Users.Add(user);
        db.Registrations.Add(new Registration
        {
            EventId = eventId, UserId = userId, Status = RegistrationStatus.Confirmed
        });
        db.CheckInRecords.Add(new CheckInRecord { EventId = eventId, UserId = userId });
        await db.SaveChangesAsync();

        var controller = new CheckInController(db, MockUserManager(user), MockHub());
        ClaimsHelper.SetUser(controller, userId, "Operator");

        var result = await controller.CheckIn(eventId, new CheckInRequest(userId));

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task CheckOut_ActiveCheckIn_ReturnsOkWithCheckedOutAt()
    {
        using var db = TestDbContextFactory.Create("CheckOut_OK");
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
            Id = userId, UserName = "testuser", Name = "Test User",
            Email = "t@test.com", NormalizedEmail = "T@TEST.COM", NormalizedUserName = "TESTUSER"
        };
        db.Users.Add(user);
        db.CheckInRecords.Add(new CheckInRecord { EventId = eventId, UserId = userId });
        await db.SaveChangesAsync();

        var controller = new CheckInController(db, MockUserManager(user), MockHub());
        ClaimsHelper.SetUser(controller, userId, "Operator");

        var result = await controller.CheckOut(eventId, new CheckInRequest(userId));

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CheckInDto>(ok.Value);
        Assert.NotNull(dto.CheckedOutAt);
    }

    [Fact]
    public async Task CheckOut_NoActiveCheckIn_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create("CheckOut_NotFound");
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "Test Event",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Users.Add(new ApplicationUser
        {
            Id = userId, UserName = "testuser", Name = "Test User",
            Email = "t@test.com", NormalizedEmail = "T@TEST.COM", NormalizedUserName = "TESTUSER"
        });
        await db.SaveChangesAsync();

        var controller = new CheckInController(db, MockUserManager(), MockHub());
        ClaimsHelper.SetUser(controller, userId, "Operator");

        var result = await controller.CheckOut(eventId, new CheckInRequest(userId));

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}
