using Leyline.RulesCore.State;
using Leyline.Scenarios;

namespace Leyline.Scenarios.Tests;

public class ScenarioLoaderTests
{
    [Fact]
    public void Loads_board_size_and_creature_placements()
    {
        const string text = """
        seed 1
        board 3x3
        card test.grunt Creature "Grunt" attack=3 life=5 ap=3 abilities=core.move,core.attack
        creature P1 test.grunt 0,0
        creature P2 test.grunt 1,0
        """;

        var match = ScenarioLoader.Load(text);

        Assert.True(match.State.Board.Contains(new HexCoord(2, 2)));
        Assert.False(match.State.Board.Contains(new HexCoord(3, 0)));

        var actors = match.State.AllActors.OfType<CreatureState>().ToList();
        Assert.Equal(2, actors.Count);
        Assert.Contains(actors, a => a.Owner == new PlayerId(1) && a.Position == new HexCoord(0, 0));
        Assert.Contains(actors, a => a.Owner == new PlayerId(2) && a.Position == new HexCoord(1, 0));
    }

    [Fact]
    public void Loads_champion_placements_with_AP_refreshed_by_the_first_Beginning_phase()
    {
        const string text = """
        seed 2
        board 2x2
        card test.champion Champion "Champion" attack=2 life=15 ap=3 abilities=core.move,core.attack,champion.bond
        champion P1 test.champion 0,0
        """;

        var match = ScenarioLoader.Load(text);
        var champion = match.State.AllActors.OfType<ChampionState>().Single();

        Assert.Equal(new PlayerId(1), champion.Owner);
        Assert.Equal(15, champion.Life);
        Assert.Equal(3, champion.CurrentAp); // P1 is active; MatchFactory runs the first Beginning phase
    }

    [Fact]
    public void Terrain_lines_apply_terrain_and_move_cost()
    {
        const string text = """
        seed 3
        board 2x2
        terrain 1,0 basic moveCost=2
        """;

        var match = ScenarioLoader.Load(text);
        var cell = match.State.Board.GetCell(new HexCoord(1, 0));

        Assert.Equal("basic", cell.Terrain);
        Assert.Equal(2, cell.MoveCost);
        Assert.Null(match.State.Board.GetCell(new HexCoord(0, 0)).Terrain);
    }

    [Fact]
    public void Library_and_hand_lines_populate_zones_in_listed_order()
    {
        const string text = """
        seed 4
        board 2x2
        card test.grunt Creature "Grunt" attack=1 life=1 ap=1
        card test.firebolt Rite "Firebolt" mana=1 effect=damage amount=3
        champion P1 test.grunt 0,0
        library P1 test.grunt,test.firebolt
        library P1 test.grunt
        hand P1 test.firebolt
        """;

        var match = ScenarioLoader.Load(text);
        var p1 = match.State.Players.Single(p => p.Id == new PlayerId(1));

        Assert.Equal(
            [new CardDefinitionId("test.grunt"), new CardDefinitionId("test.firebolt"), new CardDefinitionId("test.grunt")],
            p1.Library); // two "library P1" lines append, in file order
        Assert.Equal([new CardDefinitionId("test.firebolt")], p1.Hand);
    }

    [Fact]
    public void Bond_lines_give_live_mana_from_turn_1_with_no_Bond_command_needed()
    {
        const string text = """
        seed 6
        board 2x2
        terrain 1,0 basic
        card test.champion Champion "Champion" attack=2 life=15 ap=7
        champion P1 test.champion 0,0
        bond P1 1,0
        """;

        var match = ScenarioLoader.Load(text);
        var p1 = match.State.Players.Single(p => p.Id == new PlayerId(1));

        Assert.Equal(1, p1.Mana); // first Beginning phase already ran with the bond in place
    }

    [Fact]
    public void Rite_cards_carry_effect_and_mana_cost()
    {
        const string text = """
        seed 5
        board 2x2
        card test.firebolt Rite "Firebolt" mana=2 effect=damage amount=5
        """;

        var match = ScenarioLoader.Load(text);
        var def = match.State.Content.Get(new CardDefinitionId("test.firebolt"));

        Assert.Equal(CardType.Rite, def.Type);
        Assert.Equal(2, def.ManaCost);
        Assert.Equal("rite.damage", def.EffectId);
        Assert.Equal(5, def.EffectAmount);
    }

    [Fact]
    public void Comments_and_blank_lines_are_ignored()
    {
        const string text = """
        # a full scenario, minimal
        seed 1

        board 2x2
        # nothing else to declare
        """;

        var match = ScenarioLoader.Load(text);
        Assert.True(match.State.Board.Contains(new HexCoord(1, 1)));
    }

    [Fact]
    public void Rejects_an_owner_that_is_not_P1_or_P2()
    {
        const string text = """
        seed 1
        board 2x2
        card test.grunt Creature "Grunt" attack=1 life=1 ap=1
        creature P3 test.grunt 0,0
        """;

        var ex = Assert.Throws<InvalidDataException>(() => ScenarioLoader.Load(text));
        Assert.Contains("line 4", ex.Message);
    }

    [Fact]
    public void Rejects_an_unknown_keyword_with_a_line_number()
    {
        const string text = """
        seed 1
        board 2x2
        creture P1 test.grunt 0,0
        """;

        var ex = Assert.Throws<InvalidDataException>(() => ScenarioLoader.Load(text));
        Assert.Contains("line 3", ex.Message);
        Assert.Contains("creture", ex.Message);
    }
}
