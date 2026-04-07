using LanManager.Api.Controllers;
using LanManager.Api.DTOs;
using LanManager.Api.Hubs;
using LanManager.Api.Services;
using LanManager.Api.Tests.Helpers;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace LanManager.Api.Tests;

public class TournamentControllerTests
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

    private static BracketService CreateBracketService()
    {
        return new BracketService();
    }

    [Fact]
    public async Task GetAll_ReturnsAllTournamentsForEvent()
    {
        using var db = TestDbContextFactory.Create("Tournament_GetAll_OK");
        var eventId = Guid.NewGuid();
        var t1Id = Guid.NewGuid();
        var t2Id = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "LAN Party",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Tournaments.Add(new Tournament
        {
            Id = t1Id, EventId = eventId, Name = "CS2 Tournament", Status = "Active"
        });
        db.Tournaments.Add(new Tournament
        {
            Id = t2Id, EventId = eventId, Name = "Valorant Cup", Status = "Draft"
        });
        await db.SaveChangesAsync();

        var controller = new TournamentController(db, CreateBracketService(), MockTournamentHub());
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetAll(eventId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var tournaments = Assert.IsAssignableFrom<List<TournamentDto>>(ok.Value);
        Assert.Equal(2, tournaments.Count);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedTournament()
    {
        using var db = TestDbContextFactory.Create("Tournament_Create_OK");
        var eventId = Guid.NewGuid();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "LAN Party",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        await db.SaveChangesAsync();

        var controller = new TournamentController(db, CreateBracketService(), MockTournamentHub());
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Admin");

        var request = new CreateTournamentRequest(
            "CS2 Cup",
            new List<ParticipantInput>
            {
                new(userId1, "Player1"),
                new(userId2, "Player2")
            }
        );

        var result = await controller.Create(eventId, request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var dto = Assert.IsType<TournamentDto>(created.Value);
        Assert.Equal("CS2 Cup", dto.Name);
        Assert.Equal(2, dto.ParticipantCount);
    }

    [Fact]
    public async Task Create_EventNotFound_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create("Tournament_Create_NoEvent");
        var controller = new TournamentController(db, CreateBracketService(), MockTournamentHub());
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Admin");

        var request = new CreateTournamentRequest("Tournament", new List<ParticipantInput>());

        var result = await controller.Create(Guid.NewGuid(), request);

        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.NotNull(notFound.Value);
    }

    [Fact]
    public async Task GetBracket_ExistingTournament_ReturnsBracket()
    {
        using var db = TestDbContextFactory.Create("Tournament_GetBracket_OK");
        var eventId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "LAN",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Tournaments.Add(new Tournament
        {
            Id = tournamentId, EventId = eventId, Name = "CS2 Cup", Status = "Active"
        });
        db.TournamentMatches.Add(new TournamentMatch
        {
            TournamentId = tournamentId, Round = 1, MatchNumber = 1,
            Player1Name = "Player1", Player2Name = "Player2", Status = "Pending"
        });
        await db.SaveChangesAsync();

        var controller = new TournamentController(db, CreateBracketService(), MockTournamentHub());
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetBracket(eventId, tournamentId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var bracket = Assert.IsType<BracketDto>(ok.Value);
        Assert.Equal(tournamentId, bracket.TournamentId);
        Assert.Equal("CS2 Cup", bracket.Name);
        Assert.Single(bracket.Rounds);
    }

    [Fact]
    public async Task GetBracket_TournamentNotFound_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create("Tournament_GetBracket_NotFound");
        var eventId = Guid.NewGuid();
        db.Events.Add(new Event
        {
            Id = eventId, Name = "LAN",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        await db.SaveChangesAsync();

        var controller = new TournamentController(db, CreateBracketService(), MockTournamentHub());
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetBracket(eventId, Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task SubmitResult_ValidMatch_UpdatesMatchAndAdvancesWinner()
    {
        using var db = TestDbContextFactory.Create("Tournament_SubmitResult_OK");
        var eventId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var matchId = Guid.NewGuid();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "LAN",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Tournaments.Add(new Tournament
        {
            Id = tournamentId, EventId = eventId, Name = "CS2 Cup", Status = "Active"
        });
        db.TournamentMatches.Add(new TournamentMatch
        {
            Id = matchId, TournamentId = tournamentId, Round = 1, MatchNumber = 1,
            Player1Id = player1Id, Player1Name = "Player1",
            Player2Id = player2Id, Player2Name = "Player2",
            Status = "Pending"
        });
        await db.SaveChangesAsync();

        var controller = new TournamentController(db, CreateBracketService(), MockTournamentHub());
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Admin");

        var request = new SubmitResultRequest(player1Id);

        var result = await controller.SubmitResult(eventId, tournamentId, matchId, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var matchDto = Assert.IsType<MatchDto>(ok.Value);
        Assert.Equal(player1Id, matchDto.WinnerId);
        Assert.Equal("Player1", matchDto.WinnerName);
        Assert.Equal("Completed", matchDto.Status);
    }

    [Fact]
    public async Task SubmitResult_MatchNotFound_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create("Tournament_SubmitResult_NotFound");
        var eventId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "LAN",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Tournaments.Add(new Tournament
        {
            Id = tournamentId, EventId = eventId, Name = "CS2 Cup", Status = "Active"
        });
        await db.SaveChangesAsync();

        var controller = new TournamentController(db, CreateBracketService(), MockTournamentHub());
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Admin");

        var result = await controller.SubmitResult(eventId, tournamentId, Guid.NewGuid(), new SubmitResultRequest(Guid.NewGuid()));

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task SubmitResult_AlreadyCompleted_ReturnsBadRequest()
    {
        using var db = TestDbContextFactory.Create("Tournament_SubmitResult_Completed");
        var eventId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var matchId = Guid.NewGuid();
        var player1Id = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "LAN",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Tournaments.Add(new Tournament
        {
            Id = tournamentId, EventId = eventId, Name = "CS2 Cup", Status = "Active"
        });
        db.TournamentMatches.Add(new TournamentMatch
        {
            Id = matchId, TournamentId = tournamentId, Round = 1, MatchNumber = 1,
            Player1Id = player1Id, Player1Name = "Player1",
            Status = "Completed", WinnerId = player1Id
        });
        await db.SaveChangesAsync();

        var controller = new TournamentController(db, CreateBracketService(), MockTournamentHub());
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Admin");

        var result = await controller.SubmitResult(eventId, tournamentId, matchId, new SubmitResultRequest(player1Id));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task SubmitResult_InvalidWinner_ReturnsBadRequest()
    {
        using var db = TestDbContextFactory.Create("Tournament_SubmitResult_InvalidWinner");
        var eventId = Guid.NewGuid();
        var tournamentId = Guid.NewGuid();
        var matchId = Guid.NewGuid();
        var player1Id = Guid.NewGuid();
        var player2Id = Guid.NewGuid();
        var invalidId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "LAN",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Tournaments.Add(new Tournament
        {
            Id = tournamentId, EventId = eventId, Name = "CS2 Cup", Status = "Active"
        });
        db.TournamentMatches.Add(new TournamentMatch
        {
            Id = matchId, TournamentId = tournamentId, Round = 1, MatchNumber = 1,
            Player1Id = player1Id, Player1Name = "Player1",
            Player2Id = player2Id, Player2Name = "Player2",
            Status = "Pending"
        });
        await db.SaveChangesAsync();

        var controller = new TournamentController(db, CreateBracketService(), MockTournamentHub());
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Admin");

        var result = await controller.SubmitResult(eventId, tournamentId, matchId, new SubmitResultRequest(invalidId));

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
