using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Champions;

/// <summary>D9 (revised 2026-08-06): the Champion has no bespoke "Channel" resource — it runs
/// AP exactly like a creature (RefreshApEffect, no special-casing).</summary>
public class ChampionApTests
{
    [Fact]
    public void Champion_has_zero_AP_until_its_own_turns_Beginning_phase_has_run()
    {
        // P2's Champion — P1 is the active player at match creation, so P2's Beginning phase
        // (and AP refresh) hasn't happened yet. Same rule a defending creature is already
        // subject to; nothing Champion-specific about it.
        var match = ChampionFixtures.AttackerVsChampion();
        var champion = match.State.ActorsOwnedBy(Fixtures.P2).Single();
        Assert.Equal(0, champion.CurrentAp);
    }

    [Fact]
    public void Champion_AP_auto_refreshes_to_max_with_no_command_needed()
    {
        var match = ChampionFixtures.ChampionOnlyMatch(maxAp: 3);
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).Single();

        // MatchFactory.CreateMatch already ran the first Beginning phase — no ChannelAct
        // (or any other command) required, unlike the old Channel-gated model.
        Assert.Equal(3, champion.CurrentAp);
    }

    [Fact]
    public void Champion_can_move_and_attack_directly_once_it_has_AP()
    {
        var match = ChampionFixtures.ChampionVsChampion(maxAp: 3);

        var legal = RulesEngine.LegalCommands(match, Fixtures.P1);
        Assert.Contains(legal, c => c is MoveCommand);
        Assert.Contains(legal, c => c is DeclareCombatCommand);
    }

    [Fact]
    public void Champion_AP_refreshes_to_max_every_Beginning_phase_not_reset_to_zero()
    {
        var match = ChampionFixtures.ChampionOnlyMatch(maxAp: 3);
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).Single();

        var legalMove = RulesEngine.LegalCommands(match, Fixtures.P1).OfType<MoveCommand>().First();
        RulesEngine.Apply(match, legalMove);
        Assert.True(champion.CurrentAp < 3); // spent some AP moving

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1)); // -> P2's turn
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2)); // -> back to P1: Beginning refreshes AP

        Assert.Equal(3, champion.CurrentAp);
    }

    [Fact]
    public void Bonding_no_longer_blocks_moving_or_attacking_the_same_turn()
    {
        // The old model shared one Channel between Bond and "act as a creature" — mutually
        // exclusive per turn. D9's revision drops that: Bond just costs 2 AP (once per turn),
        // leaving any remaining AP spendable on Move/Attack like normal.
        var match = TerrainFixtures.ChampionWithTerrainChain(championMaxAp: 3);

        var bonded = RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(1, 0)));
        Assert.True(bonded.Accepted);

        var legal = RulesEngine.LegalCommands(match, Fixtures.P1);
        Assert.Contains(legal, c => c is MoveCommand); // 1 AP left of the 3 - 2 spent on Bond
    }
}
