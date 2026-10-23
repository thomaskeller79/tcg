using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.Support;
using static Leyline.RulesCore.Tests.Support.Game;

namespace Leyline.RulesCore.Tests;

/// <summary>effect-form.md §Instructions (D141): the instructions beyond damage, draw, heal,
/// create, unbond, raise, fell, destroy and the stat changes. Any game object can be a target.</summary>
public class InstructionTests
{
    private static CardObject Card(Game g, PlayerId player, CardSection section, string card) =>
        g.State.Player(player).Section(section).Select(id => g.State.Get<CardObject>(id)).First(c => c.Definition == card);

    [Fact]
    public void Discard_target_card_moves_it_from_a_Hand_to_its_Discard()
    {
        var g = Load("handcard A test.discard\nhandcard B test.grunt");
        var theirs = Card(g, B, CardSection.Hand, "test.grunt");
        g.Ok(g.Cast(A, "test.discard", Obj(theirs)));
        g.ResolveAll();
        Assert.Equal(CardSection.Discard, theirs.Section); // a move: the same card
        Assert.Contains(theirs.Id, g.State.Player(B).Discard);
    }

    [Fact]
    public void Mill_puts_the_top_cards_of_a_Champions_Library_into_its_Discard()
    {
        var g = Load("handcard A test.mill\ndeck B test.grunt test.brute test.mole");
        var library = g.State.Player(B).Library.ToList();
        g.Ok(g.Cast(A, "test.mill", Obj(g.Champion(B))));
        g.ResolveAll();
        Assert.Equal(library.Take(2), g.State.Player(B).Discard);
        Assert.Equal([library[2]], g.State.Player(B).Library);
    }

    [Fact]
    public void Shuffle_reorders_a_section_of_your_Mind_and_keeps_its_cards()
    {
        var g = Load("handcard A test.shuffle\ndeck A test.grunt test.brute test.mole test.mirror test.knotter test.digger");
        var before = g.State.Player(A).Library.ToList();
        g.Ok(g.Cast(A, "test.shuffle"));
        g.ResolveAll();
        Assert.Equal(before.Order(), g.State.Player(A).Library.Order());
    }

    [Fact]
    public void Move_returns_a_card_from_your_Discard_to_your_Hand()
    {
        var g = Load("handcard A test.recover test.discard test.grunt");
        g.Ok(g.Cast(A, "test.discard", Obj(g.HandCard(A, "test.grunt"))));
        g.ResolveAll();
        var grunt = Card(g, A, CardSection.Discard, "test.grunt");
        g.Ok(g.Cast(A, "test.recover", Obj(grunt)));
        g.ResolveAll();
        Assert.Equal(CardSection.Hand, grunt.Section);
    }

    [Fact]
    public void Destroying_a_trace_in_Pending_counters_it()
    {
        var g = Load("handcard A creature.fire-warrior test.counter\nmana A Fire 1");
        g.Ok(g.Cast(A, "creature.fire-warrior", At(0, 2)));
        var cast = g.State.Get<TraceObject>(g.State.Pending.Single());
        g.Ok(g.Cast(A, "test.counter", Obj(cast)));
        g.ResolveAll();
        Assert.Null(g.Find("creature.fire-warrior"));
        Assert.False(g.State.Exists(cast.Id));
    }

    [Fact]
    public void A_copied_trace_goes_on_top_of_Pending_and_resolves_first()
    {
        var g = Load("place B test.brute 0,0\nhandcard A spell.firebolt test.echo\nmana A Fire 2");
        g.Ok(g.Cast(A, "spell.firebolt", Obj(g.P("test.brute"))));
        var original = g.State.Get<TraceObject>(g.State.Pending.Single());
        g.Ok(g.Cast(A, "test.echo", Obj(original)));
        g.ResolveAll();
        Assert.Equal(1, g.P("test.brute").CurrentLife); // 7 - 3 - 3
    }

    [Fact]
    public void A_copied_creature_enters_on_a_terrain_you_control_and_the_original_stays()
    {
        var g = Load("place B test.brute 0,0\nhandcard A test.twin");
        var brute = g.P("test.brute");
        g.Ok(g.Cast(A, "test.twin", Obj(brute), Obj(g.Terrain(0, 2))));
        g.ResolveAll();
        var copy = g.P("test.brute", 1);
        Assert.Equal(H(0, 2), copy.Hex);
        Assert.Equal(A, g.State.Controller(copy));
        Assert.Equal(brute.Id, copy.Source);
        Assert.True(g.State.Exists(brute.Id));
    }

    [Fact]
    public void A_copied_card_goes_into_your_Hand()
    {
        var g = Load("handcard A test.duplicate test.grunt");
        var grunt = g.HandCard(A, "test.grunt");
        g.Ok(g.Cast(A, "test.duplicate", Obj(grunt)));
        g.ResolveAll();
        Assert.Equal(2, g.State.Player(A).Hand.Count(id => g.State.Get<CardObject>(id).Definition == "test.grunt"));
    }

    [Fact]
    public void Move_puts_a_creature_on_the_target_terrain_without_a_path()
    {
        var g = Load("place A test.grunt 0,1\nhandcard A test.teleport");
        g.Ok(g.Cast(A, "test.teleport", Obj(g.P("test.grunt")), Obj(g.Terrain(2, -2))));
        g.ResolveAll();
        Assert.Equal(H(2, -2), g.P("test.grunt").Hex);
    }

    [Fact]
    public void A_gained_keyword_and_a_lost_ability_end_with_their_trace()
    {
        var g = Load("place A test.grunt 0,1\nhandcard A test.wings test.rooted");
        var grunt = g.P("test.grunt");
        g.Ok(g.Cast(A, "test.wings", Obj(grunt)));
        g.ResolveAll();
        g.Ok(g.Cast(A, "test.rooted", Obj(grunt)));
        g.ResolveAll();
        Assert.True(g.State.HasKeyword(grunt, Keyword.Flying));
        Assert.DoesNotContain(g.State.Abilities(grunt), a => a.Id == DefaultAbilities.Move);
        g.NextTurnOf(Seat.ChampionB); // Duration 1: both fade at the end of A's turn (D144)
        Assert.False(g.State.HasKeyword(grunt, Keyword.Flying));
        Assert.Contains(g.State.Abilities(grunt), a => a.Id == DefaultAbilities.Move);
    }

    [Fact]
    public void Add_mana_adds_a_fixed_amount_to_your_pool()
    {
        var g = Load("handcard A test.ritual");
        g.Ok(g.Cast(A, "test.ritual"));
        g.ResolveAll();
        Assert.Equal(3, g.Champion(A).Pool!.Amounts[ManaUnit.Of(Element.Darkness)]);
    }

    [Fact]
    public void Gain_control_makes_your_Champion_the_creatures_parent()
    {
        var g = Load("place B test.brute 0,0\nhandcard A test.dominate");
        var brute = g.P("test.brute");
        g.Ok(g.Cast(A, "test.dominate", Obj(brute)));
        g.ResolveAll();
        Assert.Equal(g.Champion(A).Id, brute.Parent);
        Assert.Equal(A, g.State.Controller(brute));
    }

    [Fact]
    public void Attach_and_detach_an_Item_by_instruction()
    {
        var g = Load("place A test.grunt 0,1\nplace A item.flame-blade 1,0\nhandcard A test.arm test.disarm");
        var grunt = g.P("test.grunt");
        var blade = g.P("item.flame-blade");
        g.Ok(g.Cast(A, "test.arm", Obj(blade), Obj(grunt))); // from anywhere, unlike Equip
        g.ResolveAll();
        Assert.Equal(grunt.Id, blade.Carrier);
        Assert.Equal(5, g.State.Attack(grunt));
        g.Ok(g.Cast(A, "test.disarm", Obj(blade)));
        g.ResolveAll();
        Assert.Null(blade.Carrier);
        Assert.Equal(H(0, 1), blade.Hex);
    }
}
