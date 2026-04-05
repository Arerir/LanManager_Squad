namespace LanManager.Data.Models;

public class Tournament
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Format { get; set; } = "SingleElimination";
    public string Status { get; set; } = "Pending"; // Pending, Active, Completed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Event Event { get; set; } = null!;
    public List<TournamentParticipant> Participants { get; set; } = new();
    public List<TournamentMatch> Matches { get; set; } = new();
}
