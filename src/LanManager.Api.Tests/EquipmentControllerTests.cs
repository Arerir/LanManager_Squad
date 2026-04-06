using LanManager.Api.Controllers;
using LanManager.Api.Tests.Helpers;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace LanManager.Api.Tests;

public class EquipmentControllerTests
{
    [Fact]
    public async Task Borrow_Available_Returns201()
    {
        using var db = TestDbContextFactory.Create("EquipBorrow_OK");
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "Test",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Users.Add(new ApplicationUser
        {
            Id = userId, UserName = "borrower", Name = "Borrower",
            Email = "b@test.com", NormalizedEmail = "B@TEST.COM", NormalizedUserName = "BORROWER"
        });
        db.Equipment.Add(new Equipment
        {
            Id = Guid.NewGuid(), Name = "VR Headset #1",
            Type = EquipmentType.VrHeadset, QrCode = "EQ-VR-TEST"
        });
        await db.SaveChangesAsync();

        var controller = new EquipmentController(db);
        ClaimsHelper.SetUser(controller, userId);

        var result = await controller.Borrow("EQ-VR-TEST", eventId);

        Assert.Equal(201, (result.Result as ObjectResult)?.StatusCode);
    }

    [Fact]
    public async Task Borrow_AlreadyBorrowed_Returns409()
    {
        using var db = TestDbContextFactory.Create("EquipBorrow_Conflict");
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var eqId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "Test",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Users.Add(new ApplicationUser
        {
            Id = userId, UserName = "u1", Name = "U1",
            Email = "u1@test.com", NormalizedEmail = "U1@TEST.COM", NormalizedUserName = "U1"
        });
        db.Users.Add(new ApplicationUser
        {
            Id = otherUserId, UserName = "u2", Name = "U2",
            Email = "u2@test.com", NormalizedEmail = "U2@TEST.COM", NormalizedUserName = "U2"
        });
        db.Equipment.Add(new Equipment
        {
            Id = eqId, Name = "PC #1",
            Type = EquipmentType.Computer, QrCode = "EQ-PC-TEST"
        });
        db.EquipmentLoans.Add(new EquipmentLoan
        {
            Id = Guid.NewGuid(), EquipmentId = eqId, UserId = otherUserId,
            EventId = eventId, BorrowedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var controller = new EquipmentController(db);
        ClaimsHelper.SetUser(controller, userId);

        var result = await controller.Borrow("EQ-PC-TEST", eventId);

        Assert.IsType<ConflictObjectResult>(result.Result);
    }

    [Fact]
    public async Task Return_ActiveLoan_SetsReturnedAt()
    {
        using var db = TestDbContextFactory.Create("EquipReturn_OK");
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var eqId = Guid.NewGuid();
        var loanId = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "Test",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Users.Add(new ApplicationUser
        {
            Id = userId, UserName = "user", Name = "User",
            Email = "u@t.com", NormalizedEmail = "U@T.COM", NormalizedUserName = "USER"
        });
        db.Equipment.Add(new Equipment
        {
            Id = eqId, Name = "KB #1",
            Type = EquipmentType.Keyboard, QrCode = "EQ-KB-TEST"
        });
        db.EquipmentLoans.Add(new EquipmentLoan
        {
            Id = loanId, EquipmentId = eqId, UserId = userId,
            EventId = eventId, BorrowedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var controller = new EquipmentController(db);
        ClaimsHelper.SetUser(controller, userId, "Admin");

        var result = await controller.Return(eqId);

        Assert.IsType<OkObjectResult>(result.Result);
        var loan = await db.EquipmentLoans.FindAsync(loanId);
        Assert.NotNull(loan!.ReturnedAt);
    }

    [Fact]
    public async Task GetMyLoans_ReturnsOnlyActiveLoansForCallingUser()
    {
        using var db = TestDbContextFactory.Create("EquipMyLoans");
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var eq1 = Guid.NewGuid();
        var eq2 = Guid.NewGuid();
        var eq3 = Guid.NewGuid();

        db.Events.Add(new Event
        {
            Id = eventId, Name = "Test",
            StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active, Capacity = 100
        });
        db.Users.Add(new ApplicationUser
        {
            Id = userId, UserName = "me", Name = "Me",
            Email = "me@t.com", NormalizedEmail = "ME@T.COM", NormalizedUserName = "ME"
        });
        db.Users.Add(new ApplicationUser
        {
            Id = otherUserId, UserName = "other", Name = "Other",
            Email = "ot@t.com", NormalizedEmail = "OT@T.COM", NormalizedUserName = "OTHER"
        });
        db.Equipment.AddRange(
            new Equipment { Id = eq1, Name = "VR1", Type = EquipmentType.VrHeadset, QrCode = "Q1" },
            new Equipment { Id = eq2, Name = "PC1", Type = EquipmentType.Computer, QrCode = "Q2" },
            new Equipment { Id = eq3, Name = "KB1", Type = EquipmentType.Keyboard, QrCode = "Q3" }
        );
        // my active loan
        db.EquipmentLoans.Add(new EquipmentLoan
        {
            Id = Guid.NewGuid(), EquipmentId = eq1, UserId = userId,
            EventId = eventId, BorrowedAt = DateTime.UtcNow
        });
        // my returned loan — should NOT appear
        db.EquipmentLoans.Add(new EquipmentLoan
        {
            Id = Guid.NewGuid(), EquipmentId = eq2, UserId = userId,
            EventId = eventId, BorrowedAt = DateTime.UtcNow.AddHours(-2),
            ReturnedAt = DateTime.UtcNow.AddHours(-1)
        });
        // other user's loan — should NOT appear
        db.EquipmentLoans.Add(new EquipmentLoan
        {
            Id = Guid.NewGuid(), EquipmentId = eq3, UserId = otherUserId,
            EventId = eventId, BorrowedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var controller = new EquipmentController(db);
        ClaimsHelper.SetUser(controller, userId);

        var result = await controller.GetMyLoans();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var loans = Assert.IsAssignableFrom<List<EquipmentLoanDto>>(ok.Value);
        Assert.Single(loans);
        Assert.Equal(eq1, loans[0].EquipmentId);
    }
}
