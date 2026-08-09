using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Spells;

public class CastCreatureTests
{
    [Fact]
    public void Casting_a_creature_spends_mana_leaves_hand_and_summons_with_summoning_sickness()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 5, hand: [SpellFixtures.Grunt]);
        var target = new HexCoord(1, 0); // bonded + connected, see SpellFixtures.ChampionWithMana

        var result = RulesEngine.Apply(match, new CastCreatureCommand(Fixtures.P1, SpellFixtures.Grunt, target));
        Assert.True(result.Accepted);

        var p1 = match.State.Players.Single(p => p.Id == Fixtures.P1);
        Assert.Empty(p1.Hand);
        Assert.Equal(3, p1.Mana); // 5 - ManaCost:2

        var summoned = match.State.ActorsOwnedBy(Fixtures.P1).OfType<CreatureState>().Single();
        Assert.Equal(target, summoned.Position);
        Assert.Equal(5, summoned.Life);
        Assert.Equal(0, summoned.CurrentAp); // D20/D14: summoning sickness — no AP until its own next Beginning phase
    }

    [Fact]
    public void Cannot_cast_without_enough_mana()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 1, hand: [SpellFixtures.Grunt]); // needs 2
        var result = RulesEngine.Apply(match, new CastCreatureCommand(Fixtures.P1, SpellFixtures.Grunt, new HexCoord(1, 0)));
        Assert.False(result.Accepted);
    }

    [Fact]
    public void Cannot_cast_a_card_not_in_hand()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 10);
        var result = RulesEngine.Apply(match, new CastCreatureCommand(Fixtures.P1, SpellFixtures.Grunt, new HexCoord(1, 0)));
        Assert.False(result.Accepted);
    }

    [Fact]
    public void Cannot_summon_onto_terrain_that_is_not_bonded_and_connected()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 10, hand: [SpellFixtures.Grunt]);
        var result = RulesEngine.Apply(match, new CastCreatureCommand(Fixtures.P1, SpellFixtures.Grunt, new HexCoord(2, 2)));
        Assert.False(result.Accepted);
    }

    [Fact]
    public void Cannot_summon_onto_a_hex_occupied_by_the_enemy()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 10, hand: [SpellFixtures.Grunt]);
        // Bond a hex adjacent to P1's Champion, then place an enemy on it directly (match-setup style).
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).OfType<ChampionState>().Single();
        var contestedHex = new HexCoord(1, 0);
        Assert.Contains(contestedHex, champion.Network.Bonded); // sanity: SpellFixtures already bonded this hex

        match.State.AddActor(new CreatureState
        {
            Id = match.State.AllocateActorId(),
            Owner = Fixtures.P2,
            Definition = SpellFixtures.Grunt,
            Position = contestedHex,
            Life = 5,
            CurrentAp = 3,
        });

        var result = RulesEngine.Apply(match, new CastCreatureCommand(Fixtures.P1, SpellFixtures.Grunt, contestedHex));
        Assert.False(result.Accepted);
    }

    [Fact]
    public void Golden_path_bond_then_refresh_mana_then_cast_uses_the_real_mana_pipeline()
    {
        var match = SpellFixtures.TwoChampionsWithTerrain(); // terrain at (1,0) NOT yet bonded
        // D8 (revised 2026-08-08): the only legal first bond is the Champion's own tile.
        RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(0, 0)));

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1)); // -> P2
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2)); // -> back to P1: mana refreshes to 1

        var p1 = match.State.Players.Single(p => p.Id == Fixtures.P1);
        Assert.Equal(1, p1.Mana);

        p1.Hand.Add(SpellFixtures.Firebolt); // 1 mana Rite — cheap enough to prove the real-mana path end to end
        var result = RulesEngine.Apply(match, new CastRiteCommand(Fixtures.P1, SpellFixtures.Firebolt, match.State.ActorsOwnedBy(Fixtures.P2).Single().Id));

        Assert.True(result.Accepted);
        Assert.Equal(0, p1.Mana);
    }
}
