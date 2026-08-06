using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Terrain;

public class TerrainNetworkTests
{
    [Fact]
    public void Terrain_adjacent_to_the_Champion_is_bondable()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain();
        Assert.True(Query.CanBondTo(Fixtures.P1, new HexCoord(1, 0), match.State));
    }

    [Fact]
    public void A_cell_with_no_terrain_is_never_bondable()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain();
        Assert.False(Query.CanBondTo(Fixtures.P1, new HexCoord(0, 1), match.State));
    }

    [Fact]
    public void Unreached_terrain_is_not_bondable_until_the_chain_reaches_it()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain();
        Assert.False(Query.CanBondTo(Fixtures.P1, new HexCoord(2, 0), match.State)); // two hops from the Champion
    }

    [Fact]
    public void Bonding_is_permanent_and_gated_once_per_turn_by_the_Channel()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain();

        var first = RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(1, 0)));
        Assert.True(first.Accepted);

        var second = RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(2, 0)));
        Assert.False(second.Accepted); // Channel already used this turn

        var champion = match.State.ActorsOwnedBy(Fixtures.P1).OfType<ChampionState>().Single();
        Assert.Contains(new HexCoord(1, 0), champion.Network.Bonded);
    }

    [Fact]
    public void Mana_refreshes_next_Beginning_phase_to_the_connected_producing_count()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain();
        RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(1, 0)));

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1)); // -> P2's Beginning -> Action
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2)); // -> P1's Beginning (mana refresh) -> Action

        var p1 = match.State.Players.Single(p => p.Id == Fixtures.P1);
        Assert.Equal(1, p1.Mana);
    }

    [Fact]
    public void An_enemy_on_the_only_path_pauses_production_without_unbonding()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain();
        RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(1, 0)));
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1));
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2)); // Channel resets on P1's next Beginning
        RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(2, 0)));

        Assert.Equal(2, Query.ResolveConnectedProducingTerrain(Fixtures.P1, match.State).Count);

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
        // not node-blocking (D8): the whole downstream chain pauses.
        Assert.Empty(Query.ResolveConnectedProducingTerrain(Fixtures.P1, match.State));

        var champion = match.State.ActorsOwnedBy(Fixtures.P1).OfType<ChampionState>().Single();
        Assert.Equal(2, champion.Network.Bonded.Count); // the bond itself is untouched — permanent
    }
}
