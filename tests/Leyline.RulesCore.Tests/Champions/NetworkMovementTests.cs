using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Champions;

/// <summary>D9 (under test, tightened 2026-08-08): a Champion connected to its network is
/// "rooted" — it may only move onto hexes that are themselves part of its own bonded territory,
/// at `2AP`; disconnected (never bonded, fully blockaded, or deliberately Collapsed) it moves
/// freely at `1AP`, like any creature, with no restriction on the destination.</summary>
public class NetworkMovementTests
{
    private static Board SmallBoard()
    {
        var cells = new List<Cell>();
        for (var q = 0; q < 4; q++)
            for (var r = 0; r < 4; r++)
                cells.Add(new Cell { Coord = new HexCoord(q, r) });
        return new Board(cells);
    }

    /// <summary>Terrain bonded at (0,0); Champion at (1,0), adjacent to it — connected from
    /// the start (bonds applied before the first Beginning phase, MatchFactory's `bonds` param).</summary>
    private static Match ConnectedChampion()
    {
        var board = SmallBoard();
        board.GetCell(new HexCoord(0, 0)).Terrain = "basic";

        return MatchFactory.CreateMatch(
            board,
            [Fixtures.P1, Fixtures.P2],
            creatures: [],
            new MatchConfig(DefendRuleVariant.Exhaust),
            SpellFixtures.Content(),
            seed: 20,
            champions: [new ChampionPlacement(Fixtures.P1, SpellFixtures.Champion, new HexCoord(1, 0))],
            bonds: [new TerrainBond(Fixtures.P1, new HexCoord(0, 0))]);
    }

    private static Match DisconnectedChampion() =>
        MatchFactory.CreateMatch(
            SmallBoard(),
            [Fixtures.P1, Fixtures.P2],
            creatures: [],
            new MatchConfig(DefendRuleVariant.Exhaust),
            SpellFixtures.Content(),
            seed: 21,
            champions: [new ChampionPlacement(Fixtures.P1, SpellFixtures.Champion, new HexCoord(1, 0))]);

    [Fact]
    public void A_connected_Champion_may_only_move_onto_its_own_bonded_territory()
    {
        var match = ConnectedChampion();
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).Single();

        var legal = Query.ResolveLegalMoveTargets(champion.Id, match.State);

        Assert.Contains(new HexCoord(0, 0), legal); // the Champion's own bonded tile
        Assert.DoesNotContain(new HexCoord(0, 1), legal); // adjacent to the network, but not itself bonded — no longer enough
        Assert.DoesNotContain(new HexCoord(2, 0), legal); // never part of the network
    }

    [Fact]
    public void Moving_while_connected_costs_2AP()
    {
        var match = ConnectedChampion();
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        var startAp = champion.CurrentAp;

        var result = RulesEngine.Apply(match, new MoveCommand(Fixtures.P1, champion.Id, new HexCoord(0, 0)));

        Assert.True(result.Accepted);
        Assert.Equal(startAp - 2, champion.CurrentAp);
    }

    [Fact]
    public void Disconnected_Champion_moves_freely_at_1AP()
    {
        var match = DisconnectedChampion();
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).Single();

        var legal = Query.ResolveLegalMoveTargets(champion.Id, match.State);
        Assert.Contains(new HexCoord(2, 0), legal); // unrestricted — no network to protect

        var startAp = champion.CurrentAp;
        var result = RulesEngine.Apply(match, new MoveCommand(Fixtures.P1, champion.Id, new HexCoord(2, 0)));
        Assert.True(result.Accepted);
        Assert.Equal(startAp - 1, champion.CurrentAp);
    }

    [Fact]
    public void Collapsing_the_network_drops_every_bond_costs_0AP_and_frees_movement()
    {
        var match = ConnectedChampion();
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).OfType<ChampionState>().Single();
        var apBeforeCollapse = champion.CurrentAp;

        Assert.Contains(new HexCoord(0, 0), champion.Network.Bonded);
        Assert.DoesNotContain(new HexCoord(2, 0), Query.ResolveLegalMoveTargets(champion.Id, match.State));

        var result = RulesEngine.Apply(match, new CollapseNetworkCommand(Fixtures.P1));

        Assert.True(result.Accepted);
        Assert.Empty(champion.Network.Bonded);
        Assert.Equal(apBeforeCollapse, champion.CurrentAp); // 0AP — nothing spent

        // Disconnected now: movement is unrestricted and cheap again.
        Assert.Contains(new HexCoord(2, 0), Query.ResolveLegalMoveTargets(champion.Id, match.State));
    }

    [Fact]
    public void Cannot_collapse_an_already_empty_network()
    {
        var match = DisconnectedChampion();
        var result = RulesEngine.Apply(match, new CollapseNetworkCommand(Fixtures.P1));
        Assert.False(result.Accepted);
        Assert.Empty(RulesEngine.LegalCommands(match, Fixtures.P1).OfType<CollapseNetworkCommand>());
    }
}
