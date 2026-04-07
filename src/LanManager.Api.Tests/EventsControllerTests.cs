using LanManager.Api.Controllers;
using LanManager.Api.DTOs;
using LanManager.Api.Tests.Helpers;
using LanManager.Data.Models;
using Microsoft.AspNetCore.Mvc;

namespace LanManager.Api.Tests;

public class EventsControllerTests
{
    [Fact]
    public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
    {
        using var db = TestDbContextFactory.Create("Events_GetAll_Empty");
        var controller = new EventsController(db);
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetAll(null, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var events = Assert.IsAssignableFrom<IEnumerable<EventDto>>(ok.Value);
        Assert.Empty(events);
    }

    [Fact]
    public async Task GetAll_WithEvents_ReturnsAllEvents()
    {
        using var db = TestDbContextFactory.Create("Events_GetAll_Multiple");
        db.Events.Add(new Event
        {
            Name = "Event 1",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active,
            Capacity = 100
        });
        db.Events.Add(new Event
        {
            Name = "Event 2",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(8),
            Status = EventStatus.Draft,
            Capacity = 50
        });
        await db.SaveChangesAsync();

        var controller = new EventsController(db);
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetAll(null, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var events = Assert.IsAssignableFrom<IEnumerable<EventDto>>(ok.Value);
        Assert.Equal(2, events.Count());
    }

    [Fact]
    public async Task GetAll_FilterByStatus_ReturnsFilteredEvents()
    {
        using var db = TestDbContextFactory.Create("Events_GetAll_Filter");
        db.Events.Add(new Event
        {
            Name = "Active Event",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active,
            Capacity = 100
        });
        db.Events.Add(new Event
        {
            Name = "Draft Event",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(8),
            Status = EventStatus.Draft,
            Capacity = 50
        });
        await db.SaveChangesAsync();

        var controller = new EventsController(db);
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetAll(EventStatus.Active, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var events = Assert.IsAssignableFrom<IEnumerable<EventDto>>(ok.Value);
        Assert.Single(events);
        Assert.Equal("Active Event", events.First().Name);
    }

    [Fact]
    public async Task GetAll_SortByName_ReturnsSortedEvents()
    {
        using var db = TestDbContextFactory.Create("Events_GetAll_Sort");
        db.Events.Add(new Event
        {
            Name = "Zebra Event",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active,
            Capacity = 100
        });
        db.Events.Add(new Event
        {
            Name = "Alpha Event",
            StartDate = DateTime.UtcNow.AddDays(7),
            EndDate = DateTime.UtcNow.AddDays(8),
            Status = EventStatus.Active,
            Capacity = 50
        });
        await db.SaveChangesAsync();

        var controller = new EventsController(db);
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetAll(null, "name");

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var events = Assert.IsAssignableFrom<IEnumerable<EventDto>>(ok.Value).ToList();
        Assert.Equal("Alpha Event", events.First().Name);
        Assert.Equal("Zebra Event", events.Last().Name);
    }

    [Fact]
    public async Task GetById_ExistingEvent_ReturnsEvent()
    {
        using var db = TestDbContextFactory.Create("Events_GetById_Exists");
        var eventId = Guid.NewGuid();
        db.Events.Add(new Event
        {
            Id = eventId,
            Name = "Test Event",
            Description = "Test Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Active,
            Capacity = 100
        });
        await db.SaveChangesAsync();

        var controller = new EventsController(db);
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetById(eventId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var eventDto = Assert.IsType<EventDto>(ok.Value);
        Assert.Equal(eventId, eventDto.Id);
        Assert.Equal("Test Event", eventDto.Name);
        Assert.Equal("Test Description", eventDto.Description);
    }

    [Fact]
    public async Task GetById_NonExistingEvent_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create("Events_GetById_NotFound");
        var controller = new EventsController(db);
        ClaimsHelper.SetUser(controller, Guid.NewGuid());

        var result = await controller.GetById(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidEvent_ReturnsCreated()
    {
        using var db = TestDbContextFactory.Create("Events_Create_Valid");
        var controller = new EventsController(db);
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Admin");

        var request = new CreateEventRequest(
            "New Event",
            "Description",
            "Location",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            100,
            EventStatus.Draft
        );

        var result = await controller.Create(request);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var eventDto = Assert.IsType<EventDto>(created.Value);
        Assert.Equal("New Event", eventDto.Name);
        Assert.Equal("Description", eventDto.Description);
        Assert.Equal("Location", eventDto.Location);
        Assert.Equal(100, eventDto.Capacity);
    }

    [Fact]
    public async Task Update_ExistingEvent_ReturnsUpdated()
    {
        using var db = TestDbContextFactory.Create("Events_Update_Exists");
        var eventId = Guid.NewGuid();
        db.Events.Add(new Event
        {
            Id = eventId,
            Name = "Old Name",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Draft,
            Capacity = 100
        });
        await db.SaveChangesAsync();

        var controller = new EventsController(db);
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Organizer");

        var request = new UpdateEventRequest(
            "Updated Name",
            "Updated Description",
            "Updated Location",
            DateTime.UtcNow.AddDays(2),
            DateTime.UtcNow.AddDays(3),
            150,
            EventStatus.Active
        );

        var result = await controller.Update(eventId, request);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var eventDto = Assert.IsType<EventDto>(ok.Value);
        Assert.Equal("Updated Name", eventDto.Name);
        Assert.Equal("Updated Description", eventDto.Description);
        Assert.Equal(150, eventDto.Capacity);
        Assert.Equal("Active", eventDto.Status);
    }

    [Fact]
    public async Task Update_NonExistingEvent_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create("Events_Update_NotFound");
        var controller = new EventsController(db);
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Admin");

        var request = new UpdateEventRequest(
            "Updated Name",
            null,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            100,
            EventStatus.Active
        );

        var result = await controller.Update(Guid.NewGuid(), request);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Delete_ExistingEvent_ReturnsNoContent()
    {
        using var db = TestDbContextFactory.Create("Events_Delete_Exists");
        var eventId = Guid.NewGuid();
        db.Events.Add(new Event
        {
            Id = eventId,
            Name = "Event To Delete",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = EventStatus.Draft,
            Capacity = 100
        });
        await db.SaveChangesAsync();

        var controller = new EventsController(db);
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Admin");

        var result = await controller.Delete(eventId);

        Assert.IsType<NoContentResult>(result);
        var deletedEvent = await db.Events.FindAsync(eventId);
        Assert.Null(deletedEvent);
    }

    [Fact]
    public async Task Delete_NonExistingEvent_ReturnsNotFound()
    {
        using var db = TestDbContextFactory.Create("Events_Delete_NotFound");
        var controller = new EventsController(db);
        ClaimsHelper.SetUser(controller, Guid.NewGuid(), "Admin");

        var result = await controller.Delete(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }
}
