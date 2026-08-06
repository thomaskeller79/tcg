using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Turns;

public class TurnPhaseTests
{
    [Fact]
    public void EndPhase_cycles_to_the_other_player_and_refreshes_their_AP()
    {
        var match = Fixtures.Adjacent1v1(maxAp: 3);
        var p1Actor = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var p2Actor = match.State.ActorsOwnedBy(Fixtures.P2).Single();
        p2Actor.CurrentAp = 0; // simulate P2 having spent AP on a prior turn

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1)); // Action -> End -> next Beginning -> Action

        Assert.Equal(Fixtures.P2, match.State.ActivePlayer);
        Assert.Equal(2, match.State.TurnNumber);
        Assert.Equal("Action", match.State.CurrentPhase.Id);
        Assert.Equal(3, p2Actor.CurrentAp); // refreshed on P2's Beginning
        Assert.Equal(3, p1Actor.CurrentAp); // untouched — P1 never spent any AP this test
    }

    [Fact]
    public void Only_the_active_player_may_end_the_phase()
    {
        var match = Fixtures.Adjacent1v1();
        var result = RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2));
        Assert.False(result.Accepted);
    }
}
