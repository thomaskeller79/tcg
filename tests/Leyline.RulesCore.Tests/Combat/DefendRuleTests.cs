using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Combat;

public class DefendRuleTests
{
    [Fact]
    public void Exhaust_variant_costs_1_AP_and_zeroes_remaining()
    {
        var match = Fixtures.Adjacent1v1(DefendRuleVariant.Exhaust, maxAp: 3);
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var defender = match.State.ActorsOwnedBy(Fixtures.P2).Single();

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, defender.Position));
        var combatId = match.State.ActiveCombats.Single().Id;
        RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, [defender.Id]));

        Assert.Equal(0, defender.CurrentAp); // 3 AP -> exhausted to 0 by the 1!AP defend cost
    }

    [Fact]
    public void Exhaust_variant_forbids_defending_with_zero_AP()
    {
        var match = Fixtures.Adjacent1v1(DefendRuleVariant.Exhaust, maxAp: 3);
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var defender = match.State.ActorsOwnedBy(Fixtures.P2).Single();
        defender.CurrentAp = 0;

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, defender.Position));
        var combatId = match.State.ActiveCombats.Single().Id;
        var result = RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, [defender.Id]));

        Assert.False(result.Accepted);
    }

    [Fact]
    public void DeleteDefendOnce_variant_is_free_and_does_not_touch_AP()
    {
        var match = Fixtures.Adjacent1v1(DefendRuleVariant.DeleteDefendOnce, maxAp: 3);
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var defender = match.State.ActorsOwnedBy(Fixtures.P2).Single();
        defender.CurrentAp = 0; // even with zero AP, V2 still allows defending

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, defender.Position));
        var combatId = match.State.ActiveCombats.Single().Id;
        var result = RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, [defender.Id]));

        Assert.True(result.Accepted);
        Assert.Equal(0, defender.CurrentAp); // unchanged — V2 never spends AP to defend
    }
}
