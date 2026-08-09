using Leyline.RulesCore;
using Leyline.RulesCore.Champions;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Terrain;

public class TerrainNetworkTests
{
    [Fact]
    public void Only_the_Champions_own_tile_is_bondable_before_any_network_exists()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain();
        Assert.True(Query.CanBondTo(Fixtures.P1, new HexCoord(0, 0), match.State));
        Assert.False(Query.CanBondTo(Fixtures.P1, new HexCoord(1, 0), match.State));
    }

    [Fact]
    public void Any_cell_is_bondable_once_a_network_exists_no_terrain_type_required()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain();
        RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(0, 0)));

        // (0,1) carries no "terrain" tag at all — D8's terrain-type restriction is gone, so an
        // ordinary plain cell adjacent to the Champion is just as bondable as a marked one.
        Assert.True(Query.CanBondTo(Fixtures.P1, new HexCoord(0, 1), match.State));
    }

    [Fact]
    public void Unreached_cell_is_not_bondable_until_the_chain_reaches_it()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain();
        RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(0, 0)));
        Assert.False(Query.CanBondTo(Fixtures.P1, new HexCoord(2, 0), match.State)); // two hops from the Champion, chain hasn't reached (1,0) yet
    }

    [Fact]
    public void Bonding_is_permanent_and_gated_once_per_turn_and_costs_AP()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain(); // championMaxAp defaults to 2
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).OfType<ChampionState>().Single();
        Assert.Equal(2, champion.CurrentAp);

        var first = RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(0, 0)));
        Assert.True(first.Accepted);
        Assert.Equal(0, champion.CurrentAp); // 2*AP spent
        Assert.False(Query.CanUseOncePerTurnAction(champion.Id, ChampionActionIds.Bond, match.State));

        var second = RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(1, 0)));
        Assert.False(second.Accepted); // once-per-turn gate, not raw AP (which is also 0 here)

        Assert.Contains(new HexCoord(0, 0), champion.Network.Bonded);
    }

    [Fact]
    public void Mana_refreshes_next_Beginning_phase_to_the_connected_producing_count()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain();
        RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(0, 0)));

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1)); // -> P2's Beginning -> Action
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2)); // -> P1's Beginning (mana refresh) -> Action

        var p1 = match.State.Players.Single(p => p.Id == Fixtures.P1);
        Assert.Equal(1, p1.Mana); // the Champion's own bonded tile counts as connected while standing on it
    }

    [Fact]
    public void Mana_is_summoning_sick_a_bond_made_this_turn_does_not_add_mana_until_next_Beginning_phase()
    {
        // D21 (under test note, 2026-08-08): mana is a snapshot taken once per Beginning phase
        // (RefreshManaEffect), not a live recomputation — bonding mid-turn claims the tile
        // permanently but its mana doesn't count until the *following* Beginning phase, the
        // same "no retroactive unlock this turn" shape as D14's summoning sickness.
        var match = TerrainFixtures.ChampionWithTerrainChain();
        var p1 = match.State.Players.Single(p => p.Id == Fixtures.P1);
        Assert.Equal(0, p1.Mana);

        RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(0, 0)));

        Assert.Equal(0, p1.Mana); // bonded and already producing (self-tile), but not credited yet this turn
        Assert.Single(Query.ResolveConnectedProducingTerrain(Fixtures.P1, match.State)); // the query itself is live

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1));
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2)); // -> P1's Beginning: mana snapshot taken

        Assert.Equal(1, p1.Mana);
    }

    [Fact]
    public void An_enemy_on_the_only_path_pauses_production_without_unbonding()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain();

        RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(0, 0))); // self — the only legal first bond
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1));
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2)); // AP refreshes and the once-per-turn gate resets
        RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(1, 0))); // adjacent to the Champion
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1));
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2));
        RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(2, 0))); // adjacent to the now-producing (1,0)

        Assert.Equal(3, Query.ResolveConnectedProducingTerrain(Fixtures.P1, match.State).Count);

        match.State.AddActor(new CreatureState
        {
            Id = match.State.AllocateActorId(),
            Owner = Fixtures.P2,
            Definition = Fixtures.Grunt,
            Position = new HexCoord(1, 0),
            Life = 5,
            CurrentAp = 3,
        });

        // (1,0) is now enemy-occupied, so (2,0) behind it is unreachable too — path-blocking,
        // not node-blocking (D8): the whole downstream chain pauses. The Champion's own tile
        // (0,0) needs no path at all, so it alone keeps producing.
        var producing = Query.ResolveConnectedProducingTerrain(Fixtures.P1, match.State);
        Assert.Single(producing);
        Assert.Contains(new HexCoord(0, 0), producing);

        var champion = match.State.ActorsOwnedBy(Fixtures.P1).OfType<ChampionState>().Single();
        Assert.Equal(3, champion.Network.Bonded.Count); // the bonds themselves are untouched — permanent
    }

    [Fact]
    public void ResolveNetworkStatus_reports_owner_and_producing_for_every_bonded_cell()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain();
        RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(0, 0)));

        var status = Query.ResolveNetworkStatus(match.State);

        Assert.True(status.TryGetValue(new HexCoord(0, 0), out var bonded));
        Assert.Equal(Fixtures.P1, bonded.Owner);
        Assert.True(bonded.Producing);
        Assert.False(status.ContainsKey(new HexCoord(1, 0))); // never bonded — absent, not just false

        // Bond a second, adjacent cell and block that one to demonstrate pause — the Champion's
        // own tile can't be enemy-occupied out from under it (CanOccupyLayer blocks enemy sharing).
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1));
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2));
        RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(1, 0)));

        match.State.AddActor(new CreatureState
        {
            Id = match.State.AllocateActorId(),
            Owner = Fixtures.P2,
            Definition = Fixtures.Grunt,
            Position = new HexCoord(1, 0),
            Life = 5,
            CurrentAp = 3,
        });

        var pausedStatus = Query.ResolveNetworkStatus(match.State);
        Assert.True(pausedStatus.TryGetValue(new HexCoord(1, 0), out var paused));
        Assert.Equal(Fixtures.P1, paused.Owner);
        Assert.False(paused.Producing);
    }
}
