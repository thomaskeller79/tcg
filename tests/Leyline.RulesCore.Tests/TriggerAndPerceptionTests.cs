using Leyline.RulesCore.Model;
using Leyline.RulesCore.Perception;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.Support;
using static Leyline.RulesCore.Tests.Support.Game;

namespace Leyline.RulesCore.Tests;

public class TriggerTests
{
    [Fact]
    public void An_enters_trigger_goes_to_Pending_after_its_trace_resolves_and_resolves_like_any_trace()
    {
        var g = Load("""
            handcard A creature.herald
            mana A Light 2
            deck A spell.mend spell.mend
            """);
        g.Ok(g.Cast(A, "creature.herald", At(0, 2)));
        g.Pass(A);
        g.Pass(B);
        var trigger = Assert.Single(g.State.Pending);
        Assert.Contains("Dawn's Insight", g.State.Get<TraceObject>(trigger).Text);
        Assert.Empty(g.State.Player(A).Hand);
        g.ResolveAll();
        Assert.Single(g.State.Player(A).Hand);
    }

    [Fact]
    public void A_falls_trigger_fires_from_the_permanent_that_fell()
    {
        var g = Load("""
            place A creature.cinder-spirit 0,1
            handcard B spell.flame-dart
            deck A spell.mend
            """);
        g.Pass(A);
        g.Champion(B).Pool!.Add(Element.Fire, 1);
        g.Ok(g.Cast(B, "spell.flame-dart", Obj(g.P("creature.cinder-spirit"))));
        g.ResolveAll();
        Assert.Single(g.Remnants);
        Assert.Single(g.State.Player(A).Hand);
    }

    [Fact]
    public void Triggers_of_one_instruction_enter_APNAP_then_by_source_ID()
    {
        // Two spirits fall from one Combat instruction: the defender's (B) and the attacker's (A).
        var g = Load("""
            place A creature.cinder-spirit 0,1
            place B creature.cinder-spirit 0,0
            deck A spell.mend
            deck B spell.mend
            """);
        var a = g.P("creature.cinder-spirit");
        var b = g.P("creature.cinder-spirit", 1);
        g.Ok(g.Act(A, a, DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, B)));
        g.Pass(A);
        g.Ok(g.Act(B, b, DefaultAbilities.Defend, Obj(g.State.Get<TraceObject>(g.State.Pending[^1]))));
        g.Pass(B);
        g.Pass(A);
        g.Pass(A);
        g.Pass(B);
        Assert.Equal(2, g.State.Pending.Count);
        // The active seat's (A's) trigger entered first, so B's sits on top and resolves first.
        Assert.Equal(B, g.State.Get<TraceObject>(g.State.Pending[^1]).You);
        Assert.Equal(A, g.State.Get<TraceObject>(g.State.Pending[0]).You);
    }
}

public class PerceptionTests
{
    [Fact]
    public void Hand_and_Discard_contents_and_the_live_mana_balance_are_private_but_counts_are_public()
    {
        var g = Load("""
            handcard A spell.firebolt spell.mend
            place B test.grunt 0,0
            mana A Fire 1
            """);
        g.Ok(g.Cast(A, "spell.firebolt", Obj(g.P("test.grunt"))));
        g.ResolveAll();

        var seenByB = ViewProjector.Project(g.State, B).Players.Single(p => p.Player == "A");
        Assert.Equal(1, seenByB.HandCount);
        Assert.Null(seenByB.Hand);
        Assert.Equal(1, seenByB.DiscardCount);
        Assert.Null(seenByB.Discard);
        Assert.Null(seenByB.Pool);
        Assert.Null(ViewProjector.Project(g.State, B).Permanents.Single(p => p.Card == "champion.pyra").Pool);

        var seenByA = ViewProjector.Project(g.State, A).Players.Single(p => p.Player == "A");
        Assert.NotNull(seenByA.Hand);
        Assert.NotNull(seenByA.Discard);
        Assert.NotNull(seenByA.Pool);
    }

    [Fact]
    public void The_owner_sees_its_Library_contents_in_a_canonical_order_never_the_draw_order()
    {
        var g = Load("deck A spell.mend spell.firebolt creature.fire-warrior");
        var lib = ViewProjector.Project(g.State, A).Players.Single(p => p.Player == "A").Library!;
        Assert.Equal(["creature.fire-warrior", "spell.firebolt", "spell.mend"], lib.Select(c => c.Card));
        Assert.Null(ViewProjector.Project(g.State, B).Players.Single(p => p.Player == "A").Library);
    }

    [Fact]
    public void The_network_is_public()
    {
        var g = Load("bond A 0,1");
        var hex = ViewProjector.Project(g.State, B).Hexes.Single(h => h.Coord == H(0, 1));
        Assert.Equal("A", hex.BondedByChampion);
        Assert.True(hex.Flowing);
    }

    [Fact]
    public void Legal_commands_never_mention_hidden_permanents()
    {
        var g = Load("""
            place A test.grunt 0,1
            place B test.mole 0,0 slice=Root
            handcard A spell.firebolt
            mana A Fire 1
            """);
        var hidden = g.P("test.mole").Id;
        var commands = RulesEngine.LegalCommands(g.State, A);
        Assert.DoesNotContain(commands.OfType<Commands.ActivateCommand>(),
            c => c.Targets.SelectMany(t => t).Any(t => t.Object == hidden));
    }
}
