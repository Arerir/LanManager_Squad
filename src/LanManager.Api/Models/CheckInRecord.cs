namespace LanManager.Api.Models;

public class CheckInRecord
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CheckedInAt { get; set; } = DateTime.UtcNow;
    public DateTime? CheckedOutAt { get; set; }
}
