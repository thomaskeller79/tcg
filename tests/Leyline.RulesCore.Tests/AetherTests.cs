using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.Support;
using static Leyline.RulesCore.Tests.Support.Game;

namespace Leyline.RulesCore.Tests;

/// <summary>interaction-stack.md: every cast and activation goes through Pending; Speed is
/// checked live against Pending (D45); `Now` advances when both Champions pass (G2); an Instant
/// auto-advances and blocks all active play (D92); Past fades (D50); physical traces leave no
/// Past (D45).</summary>
public class AetherTests
{
    private const string Hands = """
        mana A Fire 5
        mana A Water 3
        mana B Fire 5
        mana B Water 3
        handcard A spell.firebolt spell.flame-dart spell.battle-surge spell.quicken spell.study
        handcard B spell.firebolt spell.flame-dart*2 spell.battle-surge spell.quicken
        place A creature.fire-warrior 0,1
        place B creature.fire-warrior 0,-1
        """;

    [Fact]
    public void A_cast_spell_waits_in_Pending_until_both_Champions_pass_and_its_card_is_already_in_Discard()
    {
        var g = Load(Hands);
        var target = g.P("creature.fire-warrior", 1);
        g.Ok(g.Cast(A, "spell.firebolt", Obj(target)));
        Assert.Single(g.State.Pending);
        Assert.Equal(3, target.CurrentLife);
        Assert.Contains(g.State.Player(A).Discard, id => g.State.Get<CardObject>(id).Definition == "spell.firebolt");
        Assert.Equal(A, g.State.PriorityHolder); // G2: the caster first

        g.Pass(A);
        Assert.Single(g.State.Pending);
        g.Pass(B);
        Assert.Empty(g.State.Pending);
        Assert.Null(g.Find("creature.fire-warrior", 1)); // 3 damage felled it
        Assert.Single(g.State.Past);
    }

    [Fact]
    public void Slow_needs_your_own_main_phase_and_Pending_empty()
    {
        var g = Load(Hands);
        g.Ok(g.Cast(A, "spell.firebolt", Obj(g.P("creature.fire-warrior", 1))));
        Assert.False(g.Cast(A, "spell.study").Accepted); // Pending not empty
        g.Pass(A);
        Assert.False(g.Cast(B, "spell.firebolt", Obj(g.P("creature.fire-warrior"))).Accepted); // not B's main phase
    }

    [Fact]
    public void Quick_answers_a_physical_trace_but_not_a_spell_while_Reactive_answers_both()
    {
        var g = Load(Hands);
        var mine = g.P("creature.fire-warrior");
        var theirs = g.P("creature.fire-warrior", 1);

        g.Ok(g.Act(A, mine, DefaultAbilities.Move, At(1, 1)));
        g.Pass(A);
        g.Ok(g.Cast(B, "spell.flame-dart", Obj(mine))); // Quick vs. a Move: fine
        g.ResolveAll();

        g.Ok(g.Cast(A, "spell.firebolt", Obj(theirs)));
        g.Pass(A);
        Assert.False(g.Cast(B, "spell.flame-dart", Obj(mine)).Accepted); // a Spell trace is in Pending
        g.Ok(g.Cast(B, "spell.battle-surge", Obj(theirs))); // Reactive is fine
        Assert.Equal(2, g.State.Pending.Count);
    }

    [Fact]
    public void Quick_and_Reactive_are_playable_on_the_opponents_turn_when_Pending_is_empty()
    {
        var g = Load(Hands);
        g.Pass(A);
        Assert.Equal(B, g.State.PriorityHolder);
        g.Ok(g.Cast(B, "spell.flame-dart", Obj(g.P("creature.fire-warrior")))); // G3
    }

    [Fact]
    public void An_Instant_resolves_immediately_and_nothing_can_respond()
    {
        var g = Load(Hands);
        var mine = g.P("creature.fire-warrior");
        g.Ok(g.Act(A, mine, DefaultAbilities.Move, At(1, 1)));
        var apAfterMove = mine.CurrentAp;
        g.Ok(g.Cast(A, "spell.quicken", Obj(mine)));
        // The Instant already resolved; the Move below it still waits.
        Assert.Equal(apAfterMove + 1, mine.CurrentAp);
        Assert.Single(g.State.Pending);
        Assert.Equal(A, g.State.PriorityHolder);
    }

    [Fact]
    public void Pending_resolves_last_in_first_out()
    {
        var g = Load(Hands);
        var mine = g.P("creature.fire-warrior");
        var theirs = g.P("creature.fire-warrior", 1);
        g.Ok(g.Cast(A, "spell.firebolt", Obj(theirs)));
        g.Pass(A);
        g.Ok(g.Cast(B, "spell.battle-surge", Obj(theirs)));
        g.Pass(B);
        g.Pass(A);
        // Surge resolved first: B's warrior has +2 Attack, then Firebolt fells it.
        Assert.Single(g.State.Pending);
        Assert.Equal(4, g.State.Attack(theirs));
        g.ResolveAll();
        Assert.Null(g.Find("creature.fire-warrior", 1));
        _ = mine;
    }

    [Fact]
    public void Physical_traces_leave_no_Past_while_other_traces_fade_after_5_rounds()
    {
        var g = Load(Hands);
        g.Ok(g.Act(A, g.P("creature.fire-warrior"), DefaultAbilities.Move, At(1, 1)));
        g.ResolveAll();
        Assert.Empty(g.State.Past);

        g.Ok(g.Cast(A, "spell.study"));
        g.ResolveAll();
        Assert.Single(g.State.Past);
        var resolvedRound = g.State.Round;
        for (var i = 0; i < 4; i++)
            g.NextTurnOf(Seat.ChampionA);
        Assert.Equal(resolvedRound + 4, g.State.Round);
        Assert.Single(g.State.Past);
        g.NextTurnOf(Seat.ChampionA);
        Assert.Empty(g.State.Past);
    }

    [Fact]
    public void Every_activated_ability_goes_through_Pending_including_Bond_and_Draw()
    {
        var g = Load("deck A spell.mend spell.mend");
        var champion = g.Champion(A);
        g.Ok(g.Act(A, champion, DefaultAbilities.Bond, At(0, 1)));
        Assert.Single(g.State.Pending);
        Assert.Null(g.Terrain(0, 1).Parent);
        g.ResolveAll();
        Assert.Equal(champion.Id, g.Terrain(0, 1).Parent);
        Assert.Equal(2, champion.Pool!.Amounts[Element.Fire]); // credited live (D77)
    }
}
