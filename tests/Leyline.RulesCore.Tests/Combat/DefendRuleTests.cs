using Leyline.RulesCore;
using Leyline.RulesCore.Combat;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Combat;

/// <summary>D15 (resolved 2026-08-09): defending costs `0*AP` — free, but at most once per turn
/// per actor. Supersedes the earlier Exhaust/DeleteDefendOnce config toggle; see Query.CanDefend's
/// doc comment for why (Exhaust punished whoever attacked first with a full-round exposure gap).</summary>
public class DefendRuleTests
{
    /// <summary>Two P1 attackers flanking one P2 defender, so the defender can be attacked
    /// twice in the same turn by two independently-AP'd creatures (a single attacker can't —
    /// default Attack is `3!AP`, draining itself after the first swing).</summary>
    private static Match TwoAttackersOneDefender() =>
        MatchFactory.CreateMatch(
            Fixtures.SmallBoard(),
            [Fixtures.P1, Fixtures.P2],
            [
                new CreaturePlacement(Fixtures.P1, Fixtures.Grunt, new HexCoord(0, 0)),
                new CreaturePlacement(Fixtures.P1, Fixtures.Grunt, new HexCoord(2, 0)),
                new CreaturePlacement(Fixtures.P2, Fixtures.Grunt, new HexCoord(1, 0)),
            ],
            Fixtures.Content(attack: 3, life: 20, maxAp: 3),
            seed: 2);

    [Fact]
    public void Defending_costs_0AP_and_does_not_touch_remaining_AP()
    {
        var match = Fixtures.Adjacent1v1(maxAp: 3);
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var defender = match.State.ActorsOwnedBy(Fixtures.P2).Single();

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, defender.Position));
        var combatId = match.State.ActiveCombats.Single().Id;
        var result = RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, [defender.Id]));

        Assert.True(result.Accepted);
        Assert.Equal(3, defender.CurrentAp); // unchanged — 0AP cost
    }

    [Fact]
    public void Defending_is_legal_even_at_zero_AP()
    {
        // The whole point of the fix: an attacker that just spent all its AP attacking must
        // still be able to defend later — Defend never checks remaining AP at all.
        var match = Fixtures.Adjacent1v1(maxAp: 3);
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var defender = match.State.ActorsOwnedBy(Fixtures.P2).Single();
        defender.CurrentAp = 0;

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, defender.Position));
        var combatId = match.State.ActiveCombats.Single().Id;
        var result = RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, [defender.Id]));

        Assert.True(result.Accepted);
    }

    [Fact]
    public void A_creature_may_only_defend_once_per_turn()
    {
        var match = TwoAttackersOneDefender();
        var attackers = match.State.ActorsOwnedBy(Fixtures.P1).ToList();
        var defender = match.State.ActorsOwnedBy(Fixtures.P2).Single();

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attackers[0].Id, defender.Position));
        var combat1 = match.State.ActiveCombats.Single().Id;
        var firstDefend = RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combat1, [defender.Id]));
        Assert.True(firstDefend.Accepted);

        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1)); // resolves combat1

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attackers[1].Id, defender.Position));
        var combat2 = match.State.ActiveCombats.Single().Id;

        Assert.False(Query.CanDefend(defender.Id, match.State));
        var secondDefend = RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combat2, [defender.Id]));
        Assert.False(secondDefend.Accepted);

        var combat2State = match.State.GetCombat(combat2);
        Assert.DoesNotContain(defender.Id,
            CombatPipeline.LegalDefenderDeclarations(match.State, combat2State, Fixtures.P2).SelectMany(c => c.Defenders));
    }

    [Fact]
    public void Defend_becomes_available_again_next_Beginning_phase()
    {
        var match = TwoAttackersOneDefender();
        var attackers = match.State.ActorsOwnedBy(Fixtures.P1).ToList();
        var defender = match.State.ActorsOwnedBy(Fixtures.P2).Single();

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attackers[0].Id, defender.Position));
        var combat1 = match.State.ActiveCombats.Single().Id;
        RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combat1, [defender.Id]));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1));

        Assert.False(Query.CanDefend(defender.Id, match.State));

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1)); // -> P2's turn: Beginning resets the once-per-turn gate

        Assert.True(Query.CanDefend(defender.Id, match.State));
    }
}
