using Leyline.RulesCore.Model;
using Leyline.RulesCore.Perception;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.Support;
using static Leyline.RulesCore.Tests.Support.Game;

namespace Leyline.RulesCore.Tests;

/// <summary>asymmetric-information.md §Beliefs (D149, D154, D155): what each Champion believes
/// about cards and traces.</summary>
public class BeliefTests
{
    private static MindView Mind(Game g, PlayerId observer, PlayerId of) =>
        ViewProjector.Project(g.State, observer).Players.Single(p => p.Player == of.ToString());

    [Fact]
    public void A_drawn_card_is_known_to_its_owner_and_unknown_to_the_opponent()
    {
        var g = Load("deck A test.grunt test.brute");
        Assert.Empty(Mind(g, A, A).Library); // Library content is `?` for everyone at the start
        Rules.Mind.Draw(g.State, A, 1);
        Assert.Equal(["test.grunt"], Mind(g, A, A).Hand.Select(c => c.Card));
        var theirs = Assert.Single(Mind(g, B, A).Hand);
        Assert.Null(theirs.Card);
        Assert.Null(theirs.Id);
        Assert.Equal(1, Mind(g, B, A).LibraryCount);
    }

    [Fact]
    public void A_cast_card_is_known_to_everyone_in_Discard_and_its_trace_is_shown()
    {
        var g = Load("handcard A spell.firebolt\nmana A Fire 2\nplace B test.brute 0,0");
        g.Ok(g.Cast(A, "spell.firebolt", Obj(g.P("test.brute"))));
        Assert.Equal(["spell.firebolt"], Mind(g, B, A).Discard.Select(c => c.Card));
        Assert.Equal("Cast Firebolt", Assert.Single(ViewProjector.Project(g.State, B).Pending).Text);
    }

    [Fact]
    public void A_bounced_permanent_is_a_card_both_Champions_know()
    {
        var g = Load("place A test.grunt 0,1\nhandcard A spell.recall\nmana A Water 2");
        g.Ok(g.Cast(A, "spell.recall", Obj(g.P("test.grunt"))));
        g.ResolveAll();
        Assert.Contains("test.grunt", Mind(g, B, A).Hand.Select(c => c.Card));
    }

    [Fact]
    public void A_shuffled_known_card_becomes_anonymous_to_whoever_cannot_identify_a_draw()
    {
        // S2 (Track B item 26): B saw the Grunt go into A's Library; after a shuffle and a draw
        // B can't tell which card left, A saw it.
        var g = Load("place A test.grunt 0,1\nhandcard A spell.recall test.stash test.shuffle\nmana A Water 2\ndeck A test.brute test.mole");
        g.Ok(g.Cast(A, "spell.recall", Obj(g.P("test.grunt"))));
        g.ResolveAll();
        var grunt = g.HandCard(A, "test.grunt");
        g.Ok(g.Cast(A, "test.stash", Obj(grunt)));
        g.ResolveAll();
        Assert.Equal((CardSection.Library, true), (grunt.Section, g.State.Beliefs.Card(B, grunt.Id).PositionKnown));
        g.Ok(g.Cast(A, "test.shuffle"));
        g.ResolveAll();
        Assert.Equal(["test.grunt"], Mind(g, B, A).Library.Select(c => c.Card)); // known, position `?`
        Assert.Null(Mind(g, B, A).Library[0].Position);

        var top = g.State.Get<CardObject>(g.State.Player(A).Library[0]);
        Rules.Mind.Draw(g.State, A, 1);
        Assert.Empty(Mind(g, B, A).Library); // anonymous for B
        Assert.Equal(top.Definition == "test.grunt" ? [] : ["test.grunt"], Mind(g, A, A).Library.Select(c => c.Card)); // A saw what it drew
    }

    [Fact]
    public void A_trace_names_a_hidden_creature_but_not_where_it_goes()
    {
        var g = Load("place A test.mole 0,0");
        var mole = g.P("test.mole");
        g.Ok(g.Act(A, mole, DefaultAbilities.Descend));
        var descend = Assert.Single(ViewProjector.Project(g.State, B).Pending);
        Assert.Equal("Test Mole " + mole.Id + ": Descend", descend.Text);
        Assert.Contains("location: a hidden location", descend.Targets);
        Assert.Contains("location: (0,0) Root", Assert.Single(ViewProjector.Project(g.State, A).Pending).Targets);
    }

    [Fact]
    public void Darkness_takes_away_sight_and_a_target_nobody_knows_the_place_of_fizzles()
    {
        // D152 sight lost, D159 visibility: B targets A's grunt; A darkens its Ground in response.
        var g = Load("place A test.grunt 0,1\nhandcard A test.darkness\nhandcard B spell.firebolt");
        var grunt = g.P("test.grunt");
        g.AdvanceTo(Seat.ChampionB);
        g.Champion(B).Pool!.Add(Element.Fire, 2);
        g.Ok(g.Cast(B, "spell.firebolt", Obj(grunt)));
        g.Pass(B);
        g.Ok(g.Cast(A, "test.darkness", Obj(g.Terrain(0, 1))));
        g.ResolveAll();
        Assert.Null(g.State.BelievedLocation(B, grunt)); // sight lost: `?`
        Assert.Equal((H(0, 1), Slice.Ground), g.State.BelievedLocation(A, grunt));
        Assert.Equal(5, grunt.CurrentLife); // the Firebolt fizzled
        Assert.Null(ViewProjector.Project(g.State, B).Permanents.Single(p => p.Id == grunt.Id.Value).Hex);
    }

    [Fact]
    public void A_Root_presence_next_to_it_observes_a_hidden_creature()
    {
        var g = Load("place A test.mole 0,0 slice=Root\nplace B test.mole 1,0 slice=Root");
        g.Ok(g.Act(A, g.P("test.mole"), DefaultAbilities.Ascend));
        g.ResolveAll();
        g.Ok(g.Act(A, g.P("test.mole"), DefaultAbilities.Descend));
        g.ResolveAll();
        Assert.Equal((H(0, 0), Slice.Root), g.State.BelievedLocation(B, g.P("test.mole"))); // D67
    }
}
