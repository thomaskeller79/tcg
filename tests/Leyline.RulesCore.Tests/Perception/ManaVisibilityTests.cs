using Leyline.RulesCore.Perception;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Perception;

/// <summary>D18: the mana *network* (bonded/producing terrain) is public, but the live mana
/// *balance* is the one deliberately-hidden standing quantity (the engine of cost-deception) —
/// see design-asymmetric-information.md. Regression test for a real leak: ViewProjector used to
/// report every player's Mana unconditionally.</summary>
public class ManaVisibilityTests
{
    [Fact]
    public void Live_mana_balance_is_hidden_from_every_observer_but_its_own()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 5);

        var p1View = ViewProjector.Project(match.State, Fixtures.P1);
        Assert.Equal(5, p1View.Mana.Single(m => m.Player == Fixtures.P1).Mana);
        Assert.Null(p1View.Mana.Single(m => m.Player == Fixtures.P2).Mana);

        var p2View = ViewProjector.Project(match.State, Fixtures.P2);
        Assert.Null(p2View.Mana.Single(m => m.Player == Fixtures.P1).Mana);
    }
}
