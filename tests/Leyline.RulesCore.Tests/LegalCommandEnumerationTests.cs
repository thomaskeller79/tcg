using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests;

public class LegalCommandEnumerationTests
{
    [Fact]
    public void Active_player_can_move_attack_or_end_phase()
    {
        var match = Fixtures.Adjacent1v1();
        var commands = RulesEngine.LegalCommands(match, Fixtures.P1);

        Assert.Contains(commands, c => c is MoveCommand);
        Assert.Contains(commands, c => c is DeclareCombatCommand);
        Assert.Contains(commands, c => c is EndPhaseCommand);
    }

    [Fact]
    public void Inactive_player_has_no_legal_commands_outside_a_pending_decision()
    {
        var match = Fixtures.Adjacent1v1();
        Assert.Empty(RulesEngine.LegalCommands(match, Fixtures.P2));
    }

    [Fact]
    public void During_the_priority_window_only_the_current_priority_holder_may_pass()
    {
        var match = Fixtures.Adjacent1v1();
        var attacker = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var defender = match.State.ActorsOwnedBy(Fixtures.P2).Single();

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, defender.Position));
        var combatId = match.State.ActiveCombats.Single().Id;
        RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, [defender.Id]));

        // Single-defender declare auto-assigns and opens the window; defender has priority first.
        Assert.Equal([new PassPriorityCommand(Fixtures.P2)], RulesEngine.LegalCommands(match, Fixtures.P2));
        Assert.Empty(RulesEngine.LegalCommands(match, Fixtures.P1));
    }

    [Fact]
    public void Multi_occupant_hex_offers_all_defender_subsets_including_empty()
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
        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker.Id, new HexCoord(1, 0)));

        var options = RulesEngine.LegalCommands(match, Fixtures.P2);
        Assert.Equal(4, options.Count); // 2^2 subsets: {}, {a}, {b}, {a,b}
        Assert.Contains(options, c => c is DeclareDefendersCommand d && d.Defenders.Count == 0);
        Assert.Contains(options, c => c is DeclareDefendersCommand d && d.Defenders.Count == 2);
    }
}
