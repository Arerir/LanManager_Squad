namespace LanManager.Data.Models;

public class TournamentParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TournamentId { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int Seed { get; set; }
    public Tournament Tournament { get; set; } = null!;
}
