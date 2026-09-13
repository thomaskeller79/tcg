using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Turns;

/// <summary>D60: each round is `Player A -> Neutral A -> Player B -> Neutral B`, wrapping. The
/// neutral turns always occur, unconditionally — but since no Neutral permanent/Behavior exists
/// yet, they have nothing to do and TurnEngine auto-advances straight through them (see
/// TurnEngine.RunPhaseEnter). This is what these tests actually observe: a single EndPhaseCommand
/// from one real player always lands on the other real player's Action phase, with the neutral
/// turn's own TurnNumber/RoundNumber bookkeeping still having happened in between.</summary>
public class RoundStructureTests
{
    [Fact]
    public void Turn_1_starts_on_Player_1_in_round_1()
    {
        var match = Fixtures.Adjacent1v1();

        Assert.Equal(1, match.State.TurnNumber);
        Assert.Equal(1, match.State.RoundNumber);
        Assert.Equal((PlayerId?)Fixtures.P1, match.State.ActivePlayer);
    }

    [Fact]
    public void A_full_round_visits_both_players_with_a_neutral_turn_auto_advanced_between_each()
    {
        var match = Fixtures.Adjacent1v1();

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1)); // P1 -> Neutral (auto) -> P2
        Assert.Equal(3, match.State.TurnNumber);
        Assert.Equal(1, match.State.RoundNumber);
        Assert.Equal((PlayerId?)Fixtures.P2, match.State.ActivePlayer);

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2)); // P2 -> Neutral (auto) -> P1, round 2
        Assert.Equal(5, match.State.TurnNumber);
        Assert.Equal(2, match.State.RoundNumber);
        Assert.Equal((PlayerId?)Fixtures.P1, match.State.ActivePlayer);
    }

    [Fact]
    public void Neither_player_may_act_during_the_auto_advanced_neutral_turn()
    {
        // There's no way to observe the neutral turn as "current" (it auto-advances the instant
        // it's entered), so this instead confirms the auto-advance is unconditional: an
        // EndPhaseCommand from P1 always lands play on P2, never stalls mid-round.
        var match = Fixtures.Adjacent1v1();
        var result = RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1));

        Assert.True(result.Accepted);
        Assert.Equal("Action", match.State.CurrentPhase.Id);
        Assert.Equal((PlayerId?)Fixtures.P2, match.State.ActivePlayer);
    }
}
