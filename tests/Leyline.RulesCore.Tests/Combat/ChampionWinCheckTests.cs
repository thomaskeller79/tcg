using Leyline.RulesCore;
using Leyline.RulesCore.Combat;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Combat;

public class ChampionWinCheckTests
{
    [Fact]
    public void Killing_the_enemy_Champion_ends_the_match()
    {
        var match = ChampionFixtures.AttackerVsChampion(championLife: 3); // one Attack=3 undefended hit is lethal
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var championId = match.State.ActorsOwnedBy(Fixtures.P2).Single().Id;

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, match.State.GetActor(championId).Position));
        var combatId = match.State.ActiveCombats.Single().Id;
        RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, []));
        RulesEngine.Apply(match, new ChooseUndefendedTargetCommand(Fixtures.P1, combatId, championId));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1));

        Assert.Equal(Fixtures.P1, match.State.Winner);
    }

    [Fact]
    public void Once_the_match_is_over_no_further_commands_are_legal_for_either_player()
    {
        var match = ChampionFixtures.AttackerVsChampion(championLife: 3);
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var championId = match.State.ActorsOwnedBy(Fixtures.P2).Single().Id;

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, match.State.GetActor(championId).Position));
        var combatId = match.State.ActiveCombats.Single().Id;
        RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, []));
        RulesEngine.Apply(match, new ChooseUndefendedTargetCommand(Fixtures.P1, combatId, championId));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1));

        Assert.Empty(RulesEngine.LegalCommands(match, Fixtures.P1));
        Assert.Empty(RulesEngine.LegalCommands(match, Fixtures.P2));

        var rejected = RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1));
        Assert.False(rejected.Accepted);
    }

    [Fact]
    public void The_Champion_is_a_valid_combat_target_with_no_Combat_pipeline_changes()
    {
        // Combat targets ActorState uniformly — a Champion needs no special-casing to be
        // attackable, declared as a defender, or chosen as the undefended target.
        var match = ChampionFixtures.AttackerVsChampion();
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var championId = match.State.ActorsOwnedBy(Fixtures.P2).Single().Id;

        var legalAttacks = Query.ResolveLegalAttackTargets(attacker.Id, match.State);
        Assert.Contains(match.State.GetActor(championId).Position, legalAttacks);

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, match.State.GetActor(championId).Position));
        var combatId = match.State.ActiveCombats.Single().Id;
        var choices = CombatPipeline.LegalUndefendedChoices(match.State, match.State.GetCombat(combatId), Fixtures.P1);

        Assert.Contains(choices, c => c.Target == championId);
    }
}
