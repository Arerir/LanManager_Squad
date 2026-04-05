namespace LanManager.Data.Models;

public class Seat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public string Label { get; set; } = string.Empty;
    public Guid? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public DateTime? AssignedAt { get; set; }
    public Event Event { get; set; } = null!;
}
