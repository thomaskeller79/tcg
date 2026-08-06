using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Combat;

public class CombatTests
{
    [Fact]
    public void Single_defender_combat_deals_mutual_damage()
    {
        var match = Fixtures.Adjacent1v1();
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var defender = match.State.ActorsOwnedBy(Fixtures.P2).Single();

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, defender.Position));
        var combatId = match.State.ActiveCombats.Single().Id;
        RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, [defender.Id]));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1));

        Assert.Equal(2, defender.Life); // 5 - 3 attack
        Assert.Equal(2, attacker.Life); // 5 - 3 retaliation (D19: universal)
        Assert.Empty(match.State.ActiveCombats);
        Assert.Null(match.State.ActiveWindow);
    }

    [Fact]
    public void Declaring_an_attack_costs_all_remaining_AP()
    {
        var match = Fixtures.Adjacent1v1(maxAp: 5);
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var defender = match.State.ActorsOwnedBy(Fixtures.P2).Single();

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, defender.Position));

        Assert.Equal(0, attacker.CurrentAp);
    }

    [Fact]
    public void Undefended_attack_deals_full_damage_with_no_retaliation()
    {
        var match = Fixtures.Adjacent1v1();
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var defender = match.State.ActorsOwnedBy(Fixtures.P2).Single();

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, defender.Position));
        var combatId = match.State.ActiveCombats.Single().Id;
        RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, []));
        RulesEngine.Apply(match, new ChooseUndefendedTargetCommand(Fixtures.P1, combatId, defender.Id));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1));

        Assert.Equal(2, defender.Life);
        Assert.Equal(5, attacker.Life);
    }

    [Fact]
    public void Lethal_damage_destroys_the_creature()
    {
        var match = Fixtures.Adjacent1v1(attack: 10);
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var defenderId = match.State.ActorsOwnedBy(Fixtures.P2).Single().Id;

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, match.State.GetActor(defenderId).Position));
        var combatId = match.State.ActiveCombats.Single().Id;
        RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, [defenderId]));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1));

        Assert.Null(match.State.FindActor(defenderId));
    }

    [Fact]
    public void Move_consumes_ap_and_relocates_the_actor()
    {
        var match = Fixtures.Adjacent1v1();
        var mover = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var destination = new HexCoord(0, 1);

        RulesEngine.Apply(match, new MoveCommand(Fixtures.P1, mover.Id, destination));

        Assert.Equal(destination, mover.Position);
        Assert.Equal(2, mover.CurrentAp); // 3 - 1
    }

    [Fact]
    public void Multi_defender_damage_assignment_must_sum_to_attack()
    {
        var content = Fixtures.Content();
        var board = Fixtures.SmallBoard();
        var match = MatchFactory.CreateMatch(
            board,
            [Fixtures.P1, Fixtures.P2],
            [
                new CreaturePlacement(Fixtures.P1, Fixtures.Grunt, new HexCoord(0, 0)),
                new CreaturePlacement(Fixtures.P2, Fixtures.Grunt, new HexCoord(1, 0)),
                new CreaturePlacement(Fixtures.P2, Fixtures.Grunt, new HexCoord(1, 0)),
            ],
            new MatchConfig(DefendRuleVariant.Exhaust),
            content,
            seed: 2);

        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var defenders = match.State.ActorsOwnedBy(Fixtures.P2).ToList();

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, new HexCoord(1, 0)));
        var combatId = match.State.ActiveCombats.Single().Id;
        RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, [defenders[0].Id, defenders[1].Id]));

        var badAssignment = new Dictionary<ActorId, int> { [defenders[0].Id] = 1, [defenders[1].Id] = 1 }; // sums to 2, not 3
        var rejected = RulesEngine.Apply(match, new AssignDamageCommand(Fixtures.P1, combatId, badAssignment));
        Assert.False(rejected.Accepted);

        var goodAssignment = new Dictionary<ActorId, int> { [defenders[0].Id] = 2, [defenders[1].Id] = 1 };
        var accepted = RulesEngine.Apply(match, new AssignDamageCommand(Fixtures.P1, combatId, goodAssignment));
        Assert.True(accepted.Accepted);

        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1));

        Assert.Equal(3, defenders[0].Life); // 5 - 2
        Assert.Equal(4, defenders[1].Life); // 5 - 1
        Assert.Equal(-1, attacker.Life);    // 5 - 3 - 3 gang-up retaliation
    }
}
