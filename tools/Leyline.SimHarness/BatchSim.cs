using Leyline.RulesCore;
using Leyline.RulesCore.Rng;
using Leyline.RulesCore.State;

namespace Leyline.SimHarness;

public sealed record BatchReport(double WinRateP1, double WinRateP2, double DrawRate, double AverageTurns);

/// <summary>
/// The literal instrument for the user's stated first M1 goal: empirically compare the D15
/// defend-rule variants. Drives matches with a uniform-random-over-legal-commands policy,
/// bypassing Host directly (the "sanctioned bypass" for batch throughput — see the M1 plan's
/// test/harness approach).
/// </summary>
public static class BatchSim
{
    public static BatchReport Run(DefendRuleVariant variant, ICardDefinitionRepository content, int matchCount, int turnCap, ulong seed)
    {
        int p1Wins = 0, p2Wins = 0, draws = 0;
        long totalTurns = 0;
        var policyRng = RngState.FromSeed(seed);

        for (var i = 0; i < matchCount; i++)
        {
            var (winner, turns, nextRng) = RunOneMatch(variant, content, turnCap, policyRng);
            policyRng = nextRng;
            totalTurns += turns;
            switch (winner)
            {
                case 1: p1Wins++; break;
                case 2: p2Wins++; break;
                default: draws++; break;
            }
        }

        return new BatchReport(
            (double)p1Wins / matchCount,
            (double)p2Wins / matchCount,
            (double)draws / matchCount,
            (double)totalTurns / matchCount);
    }

    private static (int Winner, int Turns, RngState NextRng) RunOneMatch(
        DefendRuleVariant variant, ICardDefinitionRepository content, int turnCap, RngState policyRng)
    {
        var p1 = new PlayerId(1);
        var p2 = new PlayerId(2);
        var match = TestMatches.TwoVsTwoGruntsWithChampions(variant, content);

        const int maxActions = 5000; // safety valve independent of turn count
        for (var actionCount = 0; actionCount < maxActions; actionCount++)
        {
            if (match.State.Winner is not null)
                break;
            if (match.State.TurnNumber > turnCap)
                break;

            var acted = false;
            foreach (var player in new[] { p1, p2 })
            {
                var options = RulesEngine.LegalCommands(match, player);
                if (options.Count == 0)
                    continue;

                var (index, nextRng) = policyRng.NextInt(options.Count);
                policyRng = nextRng;
                var result = RulesEngine.Apply(match, options[index]);
                acted = true;
                if (!result.Accepted)
                    throw new InvalidOperationException($"A command drawn from LegalCommands was rejected: {result.RejectionReason}");
                break;
            }

            if (!acted)
                break; // nobody had anything to do — shouldn't happen, but avoid spinning forever
        }

        var winner = match.State.Winner switch
        {
            { } w when w == p1 => 1,
            { } w when w == p2 => 2,
            _ => 0, // no Champion died before the turn cap = a draw for this sandbox's purposes
        };
        return (winner, match.State.TurnNumber, policyRng);
    }
}
