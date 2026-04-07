using LanManager.Api.Controllers;
using LanManager.Api.DTOs;
using LanManager.Api.Tests.Helpers;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LanManager.Api.Tests;

public class RegistrationsControllerTests
{
    private static UserManager<ApplicationUser> MockUserManager(ApplicationUser? returnUser = null)
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var mgr = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        mgr.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(returnUser);
        return mgr.Object;
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsCreated()
    {
        using var db = TestDbContextFactory.Create("Reg_Register_OK");
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "Test Event",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        await db.SaveChangesAsync();

        var user = new ApplicationUser
        {
            Id = userId, UserName = "testuser", Name = "Test User",
            Email = "test@example.com"
        };
        var controller = new RegistrationsController(db, MockUserManager(user));
        ClaimsHelper.SetUser(controller, userId, "Attendee");

        var result = await controller.Register(eventId, new RegisterForEventRequest(userId));

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<RegistrationDto>(created.Value);
        Assert.Equal(eventId, dto.EventId);
        Assert.Equal(userId, dto.UserId);
        Assert.Equal("Confirmed", dto.Status);
    }

    [Fact]
    public async Task Register_EventNotFound_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create("Reg_Register_NoEvent");
        var userId = Guid.NewGuid();
        var user = new ApplicationUser { Id = userId, UserName = "testuser" };
        var controller = new RegistrationsController(db, MockUserManager(user));
        ClaimsHelper.SetUser(controller, userId, "Attendee");

        var result = await controller.Register(Guid.NewGuid(), new RegisterForEventRequest(userId));

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.NotNull(notFound.Value);
    }

    [Fact]
    public async Task Register_UserNotFound_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create("Reg_Register_NoUser");
        var eventId = Guid.NewGuid();
        db.Events.Add(new Event
        {
            Id = eventId, Name = "Test Event",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        await db.SaveChangesAsync();

        var controller = new RegistrationsController(db, MockUserManager(null));
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Attendee");

        var result = await controller.Register(eventId, new RegisterForEventRequest(Guid.NewGuid()));

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.NotNull(notFound.Value);
    }

    [Fact]
    public async Task Register_AlreadyRegistered_ReturnsConflict()
    {
        using var db = TestDbContextFactory.Create("Reg_Register_AlreadyReg");
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
            Email = "test@example.com"
        };
        db.Users.Add(user);
        db.Registrations.Add(new Registration
        {
            EventId = eventId, UserId = userId, Status = RegistrationStatus.Confirmed
        });
        await db.SaveChangesAsync();

        var controller = new RegistrationsController(db, MockUserManager(user));
        ClaimsHelper.SetUser(controller, userId, "Attendee");

        var result = await controller.Register(eventId, new RegisterForEventRequest(userId));

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.NotNull(conflict.Value);
    }

    [Fact]
    public async Task Register_EventAtCapacity_ReturnsConflict()
    {
        using var db = TestDbContextFactory.Create("Reg_Register_AtCapacity");
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "Full Event",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 1
        });
        var user = new ApplicationUser
        {
            Id = userId, UserName = "newuser", Name = "New User",
            Email = "new@example.com"
        };
        db.Users.Add(user);
        db.Users.Add(new ApplicationUser
        {
            Id = otherUserId, UserName = "registered", Name = "Registered",
            Email = "reg@example.com"
        });
        db.Registrations.Add(new Registration
        {
            EventId = eventId, UserId = otherUserId, Status = RegistrationStatus.Confirmed
        });
        await db.SaveChangesAsync();

        var controller = new RegistrationsController(db, MockUserManager(user));
        ClaimsHelper.SetUser(controller, userId, "Attendee");

        var result = await controller.Register(eventId, new RegisterForEventRequest(userId));

        var conflict = Assert.IsType<ConflictObjectResult>(result.Result);
        Assert.NotNull(conflict.Value);
    }

    [Fact]
    public async Task GetAttendees_ExistingEvent_ReturnsAllRegistrations()
    {
        using var db = TestDbContextFactory.Create("Reg_GetAttendees_OK");
        var eventId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "Test Event",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Users.Add(new ApplicationUser
        {
            Id = user1Id, UserName = "user1", Name = "User 1", Email = "u1@test.com"
        });
        db.Users.Add(new ApplicationUser
        {
            Id = user2Id, UserName = "user2", Name = "User 2", Email = "u2@test.com"
        });
        db.Registrations.Add(new Registration
        {
            EventId = eventId, UserId = user1Id, Status = RegistrationStatus.Confirmed
        });
        db.Registrations.Add(new Registration
        {
            EventId = eventId, UserId = user2Id, Status = RegistrationStatus.Confirmed
        });
        await db.SaveChangesAsync();

        var controller = new RegistrationsController(db, MockUserManager());
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetAttendees(eventId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var registrations = Assert.IsAssignableFrom<IEnumerable<RegistrationDto>>(ok.Value);
        Assert.Equal(2, registrations.Count());
    }

    [Fact]
    public async Task GetAttendees_EventNotFound_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create("Reg_GetAttendees_NotFound");
        var controller = new RegistrationsController(db, MockUserManager());
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetAttendees(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetAttendees_EmptyEvent_ReturnsEmptyList()
    {
        using var db = TestDbContextFactory.Create("Reg_GetAttendees_Empty");
        var eventId = Guid.NewGuid();
        db.Events.Add(new Event
        {
            Id = eventId, Name = "Empty Event",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        await db.SaveChangesAsync();

        var controller = new RegistrationsController(db, MockUserManager());
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetAttendees(eventId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var registrations = Assert.IsAssignableFrom<IEnumerable<RegistrationDto>>(ok.Value);
        Assert.Empty(registrations);
    }
}
