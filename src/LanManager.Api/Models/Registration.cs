namespace LanManager.Api.Models;

public class Registration
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public RegistrationStatus Status { get; set; } = RegistrationStatus.Confirmed;
}

public enum RegistrationStatus { Confirmed, Cancelled, Waitlisted }
