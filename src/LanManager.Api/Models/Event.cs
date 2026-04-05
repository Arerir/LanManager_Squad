namespace LanManager.Api.Models;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Capacity { get; set; }
    public EventStatus Status { get; set; } = EventStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Registration> Registrations { get; set; } = [];
    public ICollection<CheckInRecord> CheckInRecords { get; set; } = [];
}

public enum EventStatus { Draft, Published, Active, Closed }
