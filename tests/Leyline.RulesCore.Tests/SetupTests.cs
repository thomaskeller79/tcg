using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.Support;
using static Leyline.RulesCore.Tests.Support.Game;

namespace Leyline.RulesCore.Tests;

/// <summary>setup.md (D114): S1–S9, ending in Champion A's Action phase.</summary>
public class SetupTests
{
    [Fact]
    public void Every_hex_holds_exactly_one_terrain_and_void_hexes_hold_void()
    {
        var g = Load("void 1,0", withBase: true);
        Assert.Equal(37, g.State.TerrainAt.Count);
        Assert.True(g.State.IsVoid(H(1, 0)));
        Assert.Equal("terrain.fire", g.Terrain(2, 0).Definition);
        Assert.Equal(37, g.State.Permanents.Count(p => p.Kind == PermanentKind.Terrain));
    }

    [Fact]
    public void Champions_enter_on_their_home_tiles_bonded_with_at_least_4_AP_and_mana_live()
    {
        var g = Load();
        var a = g.Champion(A);
        Assert.Equal(H(0, 2), a.Hex);
        Assert.Equal(a.Id, g.Terrain(0, 2).Parent);
        Assert.Equal(4, a.CurrentAp);
        Assert.Equal(4, g.Champion(B).CurrentAp);
        Assert.Equal(1, a.Pool!.Amounts[ManaUnit.Of(Element.Fire)]);
        Assert.Equal(1, g.Champion(B).Pool!.Amounts[ManaUnit.Of(Element.Earth)]);
        Assert.True(g.Terrain(0, 2).Drawn);
    }

    [Fact]
    public void Setup_ends_in_Champion_As_Action_phase_with_A_holding_priority()
    {
        var g = Load();
        Assert.Equal(Seat.ChampionA, g.State.ActiveSeat);
        Assert.Equal(Phase.Action, g.State.Phase);
        Assert.Equal(A, g.State.PriorityHolder);
        Assert.Equal(1, g.State.Round);
    }

    [Fact]
    public void Opening_hand_is_drawn_from_the_library()
    {
        var g = Load("""
            openinghand 2
            deck A spell.firebolt creature.fire-warrior spell.mend
            deck B spell.mend
            """);
        Assert.Equal(["spell.firebolt", "creature.fire-warrior"], g.State.Player(A).Hand.Select(id => g.State.Get<CardObject>(id).Definition));
        Assert.Single(g.State.Player(A).Library);
        Assert.Single(g.State.Player(B).Hand);
    }

    [Fact]
    public void Home_ground_is_dealt_from_the_terrain_deck_and_must_match_its_size()
    {
        var text = Base.Replace("home A radius=0", "home A radius=1").Replace("terraindeck A terrain.fire", "terraindeck A terrain.water*7");
        var g = Load(text, withBase: false);
        foreach (var hex in MapDefinition.Around(H(0, 2), 1, MapDefinition.Hexagon(3)))
            Assert.Equal("terrain.water", g.State.TerrainOf(hex).Definition);

        var bad = Base.Replace("home A radius=0", "home A radius=1");
        Assert.Throws<InvalidDataException>(() => Load(bad, withBase: false));
    }

    [Fact]
    public void Setup_placed_permanents_enter_with_0_AP_and_their_enter_triggers_fire()
    {
        var g = Load("""
            deck A spell.mend spell.mend
            neutralpermanent creature.herald 0,0 behavior=Aggressive:A seat=NeutralB
            """);
        var herald = g.P("creature.herald");
        Assert.Equal(0, herald.CurrentAp);
        Assert.Null(g.State.Controller(herald));
        Assert.Empty(g.State.Pending);
        // The Neutral Herald's "draw a card" has no Champion behind it and fizzles (D74).
        Assert.Contains(g.Log, l => l.Contains("Dawn's Insight"));
    }

    [Fact]
    public void Same_seed_gives_the_same_shuffle()
    {
        const string decks = """
            deck A creature.fire-warrior*5 spell.firebolt*5 spell.mend*5
            deck B creature.stone-brute*5 spell.mend*5
            """;
        var text = Base.Replace("noshuffle", "seed 99") + "\n" + decks;
        var g1 = Load(text, withBase: false);
        var g2 = Load(text, withBase: false);
        Assert.Equal(
            g1.State.Player(A).Library.Select(id => g1.State.Get<CardObject>(id).Definition),
            g2.State.Player(A).Library.Select(id => g2.State.Get<CardObject>(id).Definition));
    }
}
