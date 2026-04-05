namespace LanManager.Api.DTOs;

public record CreateTournamentRequest(string Name, List<ParticipantInput> Participants);
public record ParticipantInput(Guid UserId, string DisplayName);
public record SubmitResultRequest(Guid WinnerId);
public record TournamentDto(Guid Id, Guid EventId, string Name, string Format, string Status, DateTime CreatedAt, int ParticipantCount);
public record BracketDto(Guid TournamentId, string Name, string Status, List<RoundDto> Rounds);
public record RoundDto(int Round, string RoundName, List<MatchDto> Matches);
public record MatchDto(Guid Id, int Round, int MatchNumber, Guid? Player1Id, string? Player1Name, Guid? Player2Id, string? Player2Name, Guid? WinnerId, string? WinnerName, string Status);
