using LanManager.Data.Models;

namespace LanManager.Api.Services;

public class BracketService
{
    public List<TournamentMatch> GenerateSingleElimination(Guid tournamentId, List<TournamentParticipant> participants)
    {
        var matches = new List<TournamentMatch>();
        int n = participants.Count;
        int slots = 1;
        while (slots < n) slots *= 2;

        var seeded = participants.OrderBy(p => p.Seed).ToList();

        int matchNum = 1;
        for (int i = 0; i < slots / 2; i++)
        {
            var p1 = i < seeded.Count ? seeded[i] : null;
            var p2Index = slots - 1 - i;
            var p2 = p2Index < seeded.Count ? seeded[p2Index] : null;
            var isBye = p1 == null || p2 == null;

            matches.Add(new TournamentMatch
            {
                TournamentId = tournamentId,
                Round = 1,
                MatchNumber = matchNum++,
                Player1Id = p1?.UserId,
                Player1Name = p1?.DisplayName,
                Player2Id = p2?.UserId,
                Player2Name = p2?.DisplayName,
                WinnerId = isBye ? (p1 ?? p2)?.UserId : null,
                WinnerName = isBye ? (p1 ?? p2)?.DisplayName : null,
                Status = isBye ? "Bye" : "Pending"
            });
        }

        int totalRounds = (int)Math.Log2(slots);
        for (int round = 2; round <= totalRounds; round++)
        {
            int matchesInRound = slots / (int)Math.Pow(2, round);
            for (int m = 1; m <= matchesInRound; m++)
                matches.Add(new TournamentMatch { TournamentId = tournamentId, Round = round, MatchNumber = m, Status = "Pending" });
        }

        return matches;
    }

    public void AdvanceWinner(List<TournamentMatch> allMatches, TournamentMatch completedMatch)
    {
        if (completedMatch.WinnerId == null) return;
        int nextRound = completedMatch.Round + 1;
        int nextMatchNum = (int)Math.Ceiling(completedMatch.MatchNumber / 2.0);
        var nextMatch = allMatches.FirstOrDefault(m => m.Round == nextRound && m.MatchNumber == nextMatchNum);
        if (nextMatch == null) return;

        if (completedMatch.MatchNumber % 2 == 1)
        {
            nextMatch.Player1Id = completedMatch.WinnerId;
            nextMatch.Player1Name = completedMatch.WinnerName;
        }
        else
        {
            nextMatch.Player2Id = completedMatch.WinnerId;
            nextMatch.Player2Name = completedMatch.WinnerName;
        }

        if (nextMatch.Player1Id != null && nextMatch.Player2Id != null) nextMatch.Status = "Pending";
    }
}
