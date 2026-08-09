using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Champions;

public class DrawCardTests
{
    [Fact]
    public void Drawing_moves_the_top_of_library_into_hand_and_costs_5AP_once_per_turn()
    {
        var match = SpellFixtures.ChampionWithMana(library: [SpellFixtures.Grunt, SpellFixtures.Firebolt]);
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        Assert.Equal(7, champion.CurrentAp);

        var result = RulesEngine.Apply(match, new DrawCardCommand(Fixtures.P1));
        Assert.True(result.Accepted);

        var p1 = match.State.Players.Single(p => p.Id == Fixtures.P1);
        Assert.Equal([SpellFixtures.Grunt], p1.Hand);
        Assert.Equal([SpellFixtures.Firebolt], p1.Library); // top card consumed, rest shifts up
        Assert.Equal(2, champion.CurrentAp); // 7 - 5

        var again = RulesEngine.Apply(match, new DrawCardCommand(Fixtures.P1));
        Assert.False(again.Accepted); // once-per-turn gate, independent of the 2 AP still available
    }

    [Fact]
    public void Cannot_draw_from_an_empty_library()
    {
        var match = SpellFixtures.ChampionWithMana();
        Assert.False(RulesEngine.Apply(match, new DrawCardCommand(Fixtures.P1)).Accepted);
        Assert.Empty(RulesEngine.LegalCommands(match, Fixtures.P1).OfType<DrawCardCommand>());
    }

    [Fact]
    public void Draw_is_available_again_after_a_full_turn_cycle()
    {
        var match = SpellFixtures.ChampionWithMana(library: [SpellFixtures.Grunt, SpellFixtures.Grunt]);
        RulesEngine.Apply(match, new DrawCardCommand(Fixtures.P1));

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1)); // -> P2
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2)); // -> back to P1: AP + once-per-turn gate reset

        var champion = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        Assert.Equal(7, champion.CurrentAp);
        Assert.Contains(RulesEngine.LegalCommands(match, Fixtures.P1), c => c is DrawCardCommand);
    }
}
