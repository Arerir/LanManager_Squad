using LanManager.Api.Controllers;
using LanManager.Api.DTOs;
using LanManager.Api.Tests.Helpers;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace LanManager.Api.Tests;

public class SeatsControllerTests
{
    [Fact]
    public async Task GetMySeat_WhenAssigned_ReturnsSeat()
    {
        using var db = TestDbContextFactory.Create("Seats_Assigned");
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
            Id = userId, UserName = "seatuser", Name = "Seat User",
            Email = "s@t.com", NormalizedEmail = "S@T.COM", NormalizedUserName = "SEATUSER"
        });
        var seatId = Guid.NewGuid();
        db.Seats.Add(new Seat
        {
            Id = seatId, EventId = eventId, Row = 0, Column = 0, Label = "A1",
            AssignedUserId = userId, AssignedUserName = "seatuser",
            AssignedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var controller = new SeatsController(db);
        ClaimsHelper.SetUser(controller, userId);

        var result = await controller.GetMySeat(eventId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<SeatDto>(ok.Value);
        Assert.Equal(seatId, dto.Id);
        Assert.Equal(userId, dto.AssignedUserId);
    }

    [Fact]
    public async Task GetMySeat_WhenNotAssigned_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create("Seats_NotAssigned");
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
            Id = userId, UserName = "seatuser2", Name = "Seat User 2",
            Email = "s2@t.com", NormalizedEmail = "S2@T.COM", NormalizedUserName = "SEATUSER2"
        });
        // Seat exists but assigned to nobody
        db.Seats.Add(new Seat
        {
            Id = Guid.NewGuid(), EventId = eventId, Row = 0, Column = 1, Label = "A2"
        });
        await db.SaveChangesAsync();

        var controller = new SeatsController(db);
        ClaimsHelper.SetUser(controller, userId);

        var result = await controller.GetMySeat(eventId);

        Assert.IsType<NotFoundResult>(result.Result);
    }
}
