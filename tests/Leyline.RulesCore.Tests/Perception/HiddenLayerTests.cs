using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Perception;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Perception;

public class HiddenLayerTests
{
    [Fact]
    public void A_submerged_creature_is_hidden_from_the_opponents_view()
    {
        var match = HiddenLayerFixtures.GroundVsBelow();
        var hidden = match.State.ActorsOwnedBy(Fixtures.P2).Single();

        var opponentView = ViewProjector.Project(match.State, Fixtures.P1);

        Assert.DoesNotContain(opponentView.Actors, a => a.Id == hidden.Id);
        var cell = opponentView.Cells.Single(c => c.Coord == hidden.Position);
        Assert.Empty(cell.Below);
    }

    [Fact]
    public void A_submerged_creature_is_visible_in_its_owners_own_view()
    {
        var match = HiddenLayerFixtures.GroundVsBelow();
        var hidden = match.State.ActorsOwnedBy(Fixtures.P2).Single();

        var ownerView = ViewProjector.Project(match.State, Fixtures.P2);

        Assert.Contains(ownerView.Actors, a => a.Id == hidden.Id);
    }

    [Fact]
    public void The_opponent_cannot_target_a_concealed_submerged_creature()
    {
        var match = HiddenLayerFixtures.GroundVsBelow();
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();

        var legalAttacks = Query.ResolveLegalAttackTargets(attacker.Id, match.State);

        Assert.Empty(legalAttacks); // the only adjacent enemy is hidden
    }

    [Fact]
    public void Attacking_from_Below_reveals_the_attacker_to_the_opponent()
    {
        var match = HiddenLayerFixtures.GroundVsBelow();
        var submerged = match.State.ActorsOwnedBy(Fixtures.P2).Single();
        var groundTarget = match.State.ActorsOwnedBy(Fixtures.P1).Single();

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1)); // -> P2's Action phase
        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P2, submerged.Id, groundTarget.Position));

        Assert.True(submerged.Located);
        var opponentView = ViewProjector.Project(match.State, Fixtures.P1);
        Assert.Contains(opponentView.Actors, a => a.Id == submerged.Id);
    }

    [Fact]
    public void Moving_away_re_conceals_a_revealed_submerged_creature()
    {
        var match = HiddenLayerFixtures.GroundVsBelow();
        var submerged = match.State.ActorsOwnedBy(Fixtures.P2).Single();
        var groundTarget = match.State.ActorsOwnedBy(Fixtures.P1).Single();

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1));
        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P2, submerged.Id, groundTarget.Position));
        Assert.True(submerged.Located);

        var combatId = match.State.ActiveCombats.Single().Id;
        RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P1, combatId, [])); // undefended
        RulesEngine.Apply(match, new ChooseUndefendedTargetCommand(Fixtures.P2, combatId, groundTarget.Id));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2)); // -> P1's turn
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1)); // -> P2's Beginning (AP refresh) -> Action

        RulesEngine.Apply(match, new MoveCommand(Fixtures.P2, submerged.Id, new HexCoord(1, 1)));

        Assert.False(submerged.Located);
    }
}
