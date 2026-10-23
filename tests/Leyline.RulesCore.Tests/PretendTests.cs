using Leyline.RulesCore.Model;
using Leyline.RulesCore.Perception;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.Support;
using static Leyline.RulesCore.Tests.Support.Game;

namespace Leyline.RulesCore.Tests;

/// <summary>asymmetric-information.md §Representing deception and §When a belief breaks (D149–D151,
/// D155, D159): Pretend, per-copy Durations, comparison and the large reveal.</summary>
public class PretendTests
{
    private static PermanentView Seen(Game g, PlayerId observer, Permanent p) =>
        ViewProjector.Project(g.State, observer).Permanents.Single(v => v.Id == p.Id.Value);

    [Fact]
    public void Mirage_Might_shows_the_opponent_a_pump_that_isnt_there()
    {
        var g = Load("place A test.grunt 0,1\nhandcard A test.mirage-might");
        var grunt = g.P("test.grunt");
        g.Ok(g.Cast(A, "test.mirage-might", Obj(grunt)));
        var trace = Assert.Single(ViewProjector.Project(g.State, B).Pending);
        Assert.Contains($"{g.State.NameOf(grunt.Id)} gets +1 Power.", trace.Instructions);
        g.ResolveAll();
        Assert.Equal(3, g.State.Attack(grunt));
        Assert.Equal(3, Seen(g, A, grunt).Attack);
        Assert.Equal(4, Seen(g, B, grunt).Attack);
    }

    [Fact]
    public void A_bluff_caught_by_a_hit_too_weak_reveals_the_Power_and_the_card()
    {
        // B believes the grunt has Power 4, truly 3; it hits B's 5-Health grunt: B's copy predicted
        // 1 Health left, B sees 2 — a contradiction, the large reveal (D151).
        var g = Load("place A test.grunt 0,1\nplace B test.grunt 0,0\nhandcard A test.mirage-might");
        var mine = g.P("test.grunt");
        var theirs = g.P("test.grunt", 1);
        g.Ok(g.Cast(A, "test.mirage-might", Obj(mine)));
        g.ResolveAll();
        var mirage = g.State.Get<TraceObject>(g.State.Past.Single());
        g.Ok(g.Act(A, mine, DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, B)));
        g.ResolveAll();
        Assert.Equal(2, theirs.CurrentLife);
        Assert.Equal(3, Seen(g, B, mine).Attack);
        Assert.Contains(B, mirage.RevealedTo);
        Assert.Equal("test.mirage-might", g.State.Beliefs.Card(B, g.State.Player(A).Discard.Single()).Definition);
    }

    [Fact]
    public void A_consistent_bluff_survives_a_lethal_hit()
    {
        var g = Load("place A test.grunt 0,1\nplace B test.mole 0,0\nhandcard A test.mirage-might");
        var mine = g.P("test.grunt");
        g.Ok(g.Cast(A, "test.mirage-might", Obj(mine)));
        g.ResolveAll();
        g.Ok(g.Act(A, mine, DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, B)));
        g.ResolveAll();
        Assert.Null(g.Find("test.mole")); // 3 or 4, it falls either way
        Assert.Equal(4, Seen(g, B, mine).Attack);
    }

    [Fact]
    public void Mirage_3_ends_early_in_the_opponents_copy_only()
    {
        var g = Load("place A test.grunt 0,1\nhandcard A test.mirage-3");
        var grunt = g.P("test.grunt");
        g.Ok(g.Cast(A, "test.mirage-3", Obj(grunt)));
        g.ResolveAll();
        Assert.Equal(4, Seen(g, B, grunt).Attack);
        g.NextTurnOf(Seat.ChampionB); // the end of A's turn: Duration 1 in B's copy (D149)
        Assert.Equal(3, Seen(g, B, grunt).Attack);
        Assert.Equal(4, Seen(g, A, grunt).Attack);
        Assert.Equal(4, g.State.Attack(grunt));
        Assert.Empty(ViewProjector.Project(g.State, B).Past);
    }

    [Fact]
    public void A_secret_curse_is_unseen_until_the_cursed_creature_hits_too_weakly()
    {
        // S8 (Track B item 26): B curses A's grunt (−1 Power, Pretend: no effect). A believes 3,
        // truly 2; it attacks B's 3-Health mole: A expected it to fall, it lives.
        var g = Load("place A test.grunt 0,1\nplace B test.mole 0,0\nhandcard B test.curse");
        var grunt = g.P("test.grunt");
        g.AdvanceTo(Seat.ChampionB);
        g.Ok(g.Cast(B, "test.curse", Obj(grunt)));
        Assert.Empty(Assert.Single(ViewProjector.Project(g.State, A).Pending).Targets); // the target is unseen
        g.ResolveAll();
        Assert.Equal(3, Seen(g, A, grunt).Attack);
        g.NextTurnOf(Seat.ChampionA);
        g.Ok(g.Act(A, grunt, DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, B)));
        g.ResolveAll();
        Assert.Equal(1, g.P("test.mole").CurrentLife);
        Assert.Equal(2, Seen(g, A, grunt).Attack); // revealed, with the curse
    }

    [Fact]
    public void A_Mimic_shows_its_face_until_a_hit_too_strong_for_it()
    {
        var g = Load("handcard A test.mimic\nplace B test.grunt 1,1");
        g.Ok(g.Cast(A, "test.mimic", At(0, 2)));
        Assert.Equal("Cast Goblin", Assert.Single(ViewProjector.Project(g.State, B).Pending).Text);
        g.ResolveAll();
        var mimic = g.P("test.mimic");
        Assert.Equal(("test.goblin", 1, 1), (Seen(g, B, mimic).Card, Seen(g, B, mimic).Attack, Seen(g, B, mimic).Life));
        Assert.Equal("test.goblin", g.State.Beliefs.Card(B, g.State.Player(A).Discard.Single()).Definition);

        g.NextTurnOf(Seat.ChampionA);
        g.Ok(g.Act(A, mimic, DefaultAbilities.Attack, AttackAt(1, 1, Slice.Ground, B)));
        g.ResolveAll();
        Assert.Null(g.Find("test.grunt")); // B expected 1 damage, its grunt fell
        Assert.Equal("test.mimic", Seen(g, B, mimic).Card);
        Assert.Equal("test.mimic", g.State.Beliefs.Card(B, g.State.Player(A).Discard.Single()).Definition);
    }

    [Fact]
    public void Destroy_target_Structure_on_a_false_Wall_fizzles_and_the_face_collapses()
    {
        // S9 (D159): legal to target by B's belief, the true state decides.
        var g = Load("handcard A test.false-wall\nhandcard B test.wreck");
        g.Ok(g.Cast(A, "test.false-wall", At(0, 2)));
        g.ResolveAll();
        var wall = g.P("test.false-wall");
        Assert.Equal("Structure", Seen(g, B, wall).Kind);
        g.AdvanceTo(Seat.ChampionB);
        g.Ok(g.Cast(B, "test.wreck", Obj(wall)));
        g.ResolveAll();
        Assert.True(g.State.Exists(wall.Id));
        Assert.Equal(("Creature", "test.false-wall"), (Seen(g, B, wall).Kind, Seen(g, B, wall).Card));
    }
}
