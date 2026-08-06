using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Champions;

public class ChannelActTests
{
    [Fact]
    public void Champion_has_zero_AP_until_the_Channel_is_spent_acting()
    {
        var match = ChampionFixtures.AttackerVsChampion();
        var champion = match.State.ActorsOwnedBy(Fixtures.P2).Single();
        Assert.Equal(0, champion.CurrentAp);
    }

    [Fact]
    public void ChannelAct_grants_the_Champion_its_max_AP_for_the_turn()
    {
        var match = ChampionFixtures.ChampionOnlyMatch(maxAp: 3);
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).Single();

        var result = RulesEngine.Apply(match, new ChannelActCommand(Fixtures.P1));

        Assert.True(result.Accepted);
        Assert.Equal(3, champion.CurrentAp);
    }

    [Fact]
    public void Once_Channel_acted_the_Champion_can_move_and_attack_with_no_special_casing()
    {
        var match = ChampionFixtures.ChampionVsChampion(maxAp: 3);
        var mine = match.State.ActorsOwnedBy(Fixtures.P1).Single();

        RulesEngine.Apply(match, new ChannelActCommand(Fixtures.P1));

        var legal = RulesEngine.LegalCommands(match, Fixtures.P1);
        Assert.Contains(legal, c => c is MoveCommand);
        Assert.Contains(legal, c => c is DeclareCombatCommand);
    }

    [Fact]
    public void ChannelAct_and_Bond_share_one_Channel_and_are_mutually_exclusive_for_the_turn()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain();

        var acted = RulesEngine.Apply(match, new ChannelActCommand(Fixtures.P1));
        Assert.True(acted.Accepted);

        var thenBond = RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(1, 0)));
        Assert.False(thenBond.Accepted);
    }

    [Fact]
    public void Champion_AP_resets_to_zero_next_turn_unless_Channel_acted_again()
    {
        var match = ChampionFixtures.ChampionOnlyMatch(maxAp: 3);
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).Single();

        RulesEngine.Apply(match, new ChannelActCommand(Fixtures.P1));
        Assert.Equal(3, champion.CurrentAp);

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1)); // -> P2's turn
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2)); // -> back to P1's Beginning -> Action

        Assert.Equal(0, champion.CurrentAp); // RefreshApEffect only refreshes CreatureState — no special-casing needed
    }
}
