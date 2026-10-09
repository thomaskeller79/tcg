using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.Tests.Support;
using static Leyline.RulesCore.Tests.Support.Game;

namespace Leyline.RulesCore.Tests;

/// <summary>D21/D60: four seats per round; G4: an Action phase ends when both Champions pass
/// in succession with Pending empty.</summary>
public class TurnTests
{
    [Fact]
    public void The_Action_phase_ends_after_both_Champions_pass_with_Pending_empty()
    {
        var g = Load();
        g.Pass(A);
        Assert.Equal(Seat.ChampionA, g.State.ActiveSeat);
        Assert.Equal(B, g.State.PriorityHolder);
        g.Pass(B);
        // Neutral A's turn: nothing assigned, so the Champions' pass round opens, B first.
        Assert.Equal(Seat.NeutralA, g.State.ActiveSeat);
        Assert.Equal(B, g.State.PriorityHolder);
    }

    [Fact]
    public void A_Reactive_Beginning_trigger_opens_priority_and_the_phase_ends_once_Pending_is_empty()
    {
        var g = Load("""
            place B test.dawn-seer-reactive 0,-1
            deck B spell.mend
            """);
        g.Pass(A);
        g.Pass(B);
        while (g.State.ActiveSeat != Seat.ChampionB)
            g.Pass(g.State.PriorityHolder!.Value);
        // D128: the trigger is respondable, so Beginning holds priority while it is in Pending.
        Assert.Equal(Phase.Beginning, g.State.Phase);
        Assert.Single(g.State.Pending);
        Assert.Equal(B, g.State.PriorityHolder);
        g.Pass(B);
        g.Pass(A);
        Assert.Equal(Phase.Action, g.State.Phase);
        Assert.Single(g.State.Player(B).Hand);
    }

    [Fact]
    public void An_Instant_Beginning_trigger_resolves_without_priority()
    {
        var g = Load("""
            place B test.dawn-seer-instant 0,-1
            deck B spell.mend
            """);
        g.AdvanceTo(Seat.ChampionB);
        Assert.Single(g.State.Player(B).Hand);
    }

    [Fact]
    public void A_trigger_without_a_stated_Speed_is_Reactive()
    {
        var herald = Game.Content.Get("creature.herald");
        Assert.Equal(Speed.Reactive, herald.Abilities.Single(a => a.IsTriggered).Speed);
    }

    [Fact]
    public void A_round_is_four_turns_and_each_Champion_refreshes_only_in_its_own_Beginning()
    {
        var g = Load();
        g.Ok(g.Act(A, g.Champion(A), DefaultAbilities.Collapse));
        g.ResolveAll();
        g.Ok(g.Act(A, g.Champion(A), DefaultAbilities.Move, At(0, 1)));
        g.ResolveAll();
        Assert.Equal(3, g.Champion(A).CurrentAp);

        g.AdvanceTo(Seat.ChampionB);
        Assert.Equal(7, g.Champion(B).CurrentAp);
        Assert.Equal(3, g.Champion(A).CurrentAp);

        g.NextTurnOf(Seat.ChampionA);
        Assert.Equal(2, g.State.Round);
        Assert.Equal(7, g.Champion(A).CurrentAp);
    }

    [Fact]
    public void Neutral_seats_never_hold_priority()
    {
        var g = Load();
        for (var i = 0; i < 12; i++)
        {
            Assert.NotNull(g.State.PriorityHolder);
            g.Pass(g.State.PriorityHolder!.Value);
        }
    }

    [Fact]
    public void Mana_pool_resets_at_the_owners_Beginning_and_credits_again_live()
    {
        var g = Load("mana A Fire 3");
        Assert.Equal(4, g.Champion(A).Pool!.Amounts[ManaUnit.Of(Element.Fire)]);
        g.NextTurnOf(Seat.ChampionA);
        Assert.Equal(1, g.Champion(A).Pool!.Amounts[ManaUnit.Of(Element.Fire)]);
    }
}
