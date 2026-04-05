namespace LanManager.Data.Models;

public class DoorPassRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public DoorPassDirection Direction { get; set; }
    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;
}

public enum DoorPassDirection { Exit, Entry }
