using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Model;
using Leyline.RulesCore.Perception;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.Support;
using static Leyline.RulesCore.Tests.Support.Game;

namespace Leyline.RulesCore.Tests;

/// <summary>Moving and Slices (overview.md §2/§4): the entry rule (D86, D119), capacity,
/// Ascend/Descend (D42), Root concealment and D67 proximity, the D68 redirect.</summary>
public class MovementTests
{
    [Fact]
    public void Move_is_a_physical_trace_and_costs_the_destination_terrains_move_cost()
    {
        var g = Load("place A test.grunt 0,1\nneutral fixed 1,0=terrain.mire");
        var grunt = g.P("test.grunt");
        g.Ok(g.Act(A, grunt, DefaultAbilities.Move, At(1, 0)));
        Assert.Equal(H(0, 1), grunt.Hex); // still in Pending
        Assert.Equal(2, grunt.CurrentAp); // the Mire costs 2
        g.ResolveAll();
        Assert.Equal(H(1, 0), grunt.Hex);
        Assert.Empty(g.State.Past);
    }

    [Fact]
    public void Entering_needs_every_occupant_of_that_Slice_to_share_your_controller_and_a_free_place()
    {
        var g = Load("""
            place A test.grunt 0,1
            place B test.grunt 0,0
            place A creature.sky-hawk 1,-1
            place A test.mirror 1,0
            place A test.mirror 1,0
            place A test.mirror 1,0
            """);
        Assert.False(g.Act(A, g.P("test.grunt"), DefaultAbilities.Move, At(0, 0)).Accepted); // enemy in Ground
        Assert.False(g.Act(A, g.P("test.grunt"), DefaultAbilities.Move, At(1, 0)).Accepted); // full
        g.Ok(g.Act(A, g.P("creature.sky-hawk"), DefaultAbilities.Move, At(0, 0, Slice.Sky))); // flyers enter over enemy Ground (D119)
    }

    [Fact]
    public void Enemy_Structures_Remnants_and_Items_never_block_entry()
    {
        var g = Load("""
            bond B 0,-1 0,0
            place B structure.watchtower 0,0
            place A test.grunt 0,1
            """);
        g.Ok(g.Act(A, g.P("test.grunt"), DefaultAbilities.Move, At(0, 0)));
    }

    [Fact]
    public void A_Subterranean_creature_descends_into_the_hidden_Root_and_ascends_again()
    {
        var g = Load("place A test.mole 0,0");
        var mole = g.P("test.mole");
        g.Ok(g.Act(A, mole, DefaultAbilities.Descend));
        g.ResolveAll();
        Assert.Equal(Slice.Root, mole.Slice);
        Assert.False(g.State.CanSee(B, mole));
        Assert.True(g.State.CanSee(A, mole));
        Assert.DoesNotContain(ViewProjector.Project(g.State, B).Permanents, p => p.Id == mole.Id.Value);

        g.Ok(g.Act(A, mole, DefaultAbilities.Ascend));
        Assert.Contains(ViewProjector.Project(g.State, B).Pending, t => t.Hidden); // the Ascend trace of a hidden mole
        g.ResolveAll();
        Assert.Equal(Slice.Ground, mole.Slice);
        Assert.True(g.State.CanSee(B, mole));
    }

    [Fact]
    public void Root_vision_belongs_to_any_permanent_you_control_in_Root_on_the_hex_or_next_to_it()
    {
        var g = Load("""
            place A test.mole 0,0 slice=Root
            place B test.mole 1,-1 slice=Root
            place B test.mole 3,-3 slice=Root
            """);
        Assert.True(g.State.CanSee(A, g.P("test.mole", 1)));  // adjacent Root presence
        Assert.False(g.State.CanSee(A, g.P("test.mole", 2))); // far away
        Assert.True(g.State.CanSee(B, g.P("test.mole")));
    }

    [Fact]
    public void Descending_blind_into_a_hidden_enemy_fails_and_the_cost_stays_paid()
    {
        var g = Load("""
            place A test.mole 0,0
            place B test.mole 0,0 slice=Root
            """);
        var mole = g.P("test.mole");
        Assert.False(g.State.CanSee(A, g.P("test.mole", 1))); // a Ground mover gives no Root vision
        g.Ok(g.Act(A, mole, DefaultAbilities.Descend));
        // Descend has no other location: a clean hard fail with the cost sunk (D68).
        Assert.Empty(g.State.Pending);
        Assert.Null(g.State.Decision);
        Assert.Equal(3, mole.CurrentAp);
        Assert.Equal(Slice.Ground, mole.Slice);
    }

    [Fact]
    public void Descending_with_a_scout_already_below_is_not_blind()
    {
        var g = Load("""
            place A test.mole 0,0
            place A test.mole 1,0 slice=Root
            place B test.mole 0,0 slice=Root
            """);
        Assert.True(g.State.CanSee(A, g.P("test.mole", 2)));
        Assert.False(g.Act(A, g.P("test.mole"), DefaultAbilities.Descend).Accepted); // known to be illegal
    }

    private const string RedirectSetup = """
        bond A 0,1 1,1
        place B test.mole 0,1 slice=Root
        handcard A test.digger
        mana A Fire 2
        """;

    [Fact]
    public void A_cast_into_a_hidden_occupant_offers_the_other_locations()
    {
        var g = Load(RedirectSetup);
        g.P("test.mole").Parent = g.Champion(B).Id;
        var card = g.HandCard(A, "test.digger");
        g.Ok(g.Cast(A, "test.digger", At(0, 1, Slice.Root)));

        var decision = Assert.IsType<RedirectDecision>(g.State.Decision);
        Assert.DoesNotContain(decision.Remaining, c => c.Hex == H(0, 1));
        Assert.Contains(decision.Remaining, c => c.Hex == H(1, 1));
        Assert.Equal(Zone.Discard, card.Zone); // the failure revealed information: cost committed
        Assert.Empty(g.State.Pending);

        g.Ok(g.Apply(new RedirectCommand(A, decision.Remaining.First(c => c.Hex == H(1, 1)))));
        Assert.Null(g.State.Decision);
        g.ResolveAll();
        Assert.Equal(H(1, 1), g.P("test.digger").Hex);
        Assert.Equal(Slice.Root, g.P("test.digger").Slice);
    }

    [Fact]
    public void Cancelling_a_redirect_keeps_the_cost_paid()
    {
        var g = Load(RedirectSetup);
        g.P("test.mole").Parent = g.Champion(B).Id;
        var card = g.HandCard(A, "test.digger");
        g.Ok(g.Cast(A, "test.digger", At(0, 1, Slice.Root)));
        g.Ok(g.Apply(new CancelRedirectCommand(A)));
        Assert.Equal(Zone.Discard, card.Zone);
        Assert.Empty(g.State.Pending);
        Assert.Equal(4, g.Champion(A).Pool!.Total); // 3 bonded + 2 added, minus the 1 paid
    }
}
