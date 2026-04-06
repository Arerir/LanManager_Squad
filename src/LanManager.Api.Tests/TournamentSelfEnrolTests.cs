using LanManager.Api.Controllers;
using LanManager.Api.Hubs;
using LanManager.Api.Services;
using LanManager.Api.Tests.Helpers;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace LanManager.Api.Tests;

public class TournamentSelfEnrolTests
{
    private static IHubContext<TournamentHub> MockTournamentHub()
    {
        var hub = new Mock<IHubContext<TournamentHub>>();
        var clients = new Mock<IHubClients>();
        var client = new Mock<IClientProxy>();
        clients.Setup(c => c.Group(It.IsAny<string>())).Returns(client.Object);
        client.Setup(c => c.SendCoreAsync(
                It.IsAny<string>(), It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        hub.Setup(h => h.Clients).Returns(clients.Object);
        return hub.Object;
    }

    private static TournamentController BuildController(
        LanManager.Data.LanManagerDbContext db, Guid callerId, params string[] roles)
    {
        var bracketService = new Mock<BracketService>();
        var controller = new TournamentController(db, bracketService.Object, MockTournamentHub());
        ClaimsHelper.SetUser(controller, callerId, roles);
        return controller;
    }

    [Fact]
    public async Task SelfEnrol_Success_Returns201()
    {
        using var db = TestDbContextFactory.Create("Tournament_Enrol_OK");
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "LAN",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Users.Add(new ApplicationUser
        {
            Id = userId, UserName = "player1", Name = "Player 1",
            Email = "p1@t.com", NormalizedEmail = "P1@T.COM", NormalizedUserName = "PLAYER1"
        });
        db.Tournaments.Add(new Tournament
        {
            Id = tournamentId, EventId = eventId, Name = "CS2 Cup", Status = "Open"
        });
        db.CheckInRecords.Add(new CheckInRecord { EventId = eventId, UserId = userId });
        await db.SaveChangesAsync();

        var controller = BuildController(db, userId);

        var result = await controller.SelfEnrol(tournamentId);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task SelfEnrol_NotCheckedIn_Returns400()
    {
        using var db = TestDbContextFactory.Create("Tournament_Enrol_NoCheckIn");
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "LAN",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Users.Add(new ApplicationUser
        {
            Id = userId, UserName = "player2", Name = "Player 2",
            Email = "p2@t.com", NormalizedEmail = "P2@T.COM", NormalizedUserName = "PLAYER2"
        });
        db.Tournaments.Add(new Tournament
        {
            Id = tournamentId, EventId = eventId, Name = "CS2 Cup", Status = "Open"
        });
        // No CheckInRecord seeded
        await db.SaveChangesAsync();

        var controller = BuildController(db, userId);

        var result = await controller.SelfEnrol(tournamentId);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SelfEnrol_AlreadyEnrolled_Returns409()
    {
        using var db = TestDbContextFactory.Create("Tournament_Enrol_Conflict");
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "LAN",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Users.Add(new ApplicationUser
        {
            Id = userId, UserName = "player3", Name = "Player 3",
            Email = "p3@t.com", NormalizedEmail = "P3@T.COM", NormalizedUserName = "PLAYER3"
        });
        db.Tournaments.Add(new Tournament
        {
            Id = tournamentId, EventId = eventId, Name = "CS2 Cup", Status = "Open"
        });
        db.CheckInRecords.Add(new CheckInRecord { EventId = eventId, UserId = userId });
        db.TournamentParticipants.Add(new TournamentParticipant
        {
            TournamentId = tournamentId, UserId = userId, DisplayName = "Player 3"
        });
        await db.SaveChangesAsync();

        var controller = BuildController(db, userId);

        var result = await controller.SelfEnrol(tournamentId);

        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task SelfEnrol_WrongStatus_Returns400()
    {
        using var db = TestDbContextFactory.Create("Tournament_Enrol_WrongStatus");
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "LAN",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Users.Add(new ApplicationUser
        {
            Id = userId, UserName = "player4", Name = "Player 4",
            Email = "p4@t.com", NormalizedEmail = "P4@T.COM", NormalizedUserName = "PLAYER4"
        });
        db.Tournaments.Add(new Tournament
        {
            Id = tournamentId, EventId = eventId, Name = "CS2 Cup", Status = "Active"  // not "Open"
        });
        db.CheckInRecords.Add(new CheckInRecord { EventId = eventId, UserId = userId });
        await db.SaveChangesAsync();

        var controller = BuildController(db, userId);

        var result = await controller.SelfEnrol(tournamentId);

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
