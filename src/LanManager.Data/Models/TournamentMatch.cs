namespace LanManager.Data.Models;

public class TournamentMatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TournamentId { get; set; }
    public int Round { get; set; }
    public int MatchNumber { get; set; }
    public Guid? Player1Id { get; set; }
    public string? Player1Name { get; set; }
    public Guid? Player2Id { get; set; }
    public string? Player2Name { get; set; }
    public Guid? WinnerId { get; set; }
    public string? WinnerName { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Bye
    public Tournament Tournament { get; set; } = null!;
}
