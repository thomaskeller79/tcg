using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.Tests.Support;
using static Leyline.RulesCore.Tests.Support.Game;

namespace Leyline.RulesCore.Tests;

/// <summary>resources-terrain.md §Severing behavior: Knotting (D121), pause vs. cut and theft
/// (D122), the realm lock (D9), Companions as second roots (D22).</summary>
public class NetworkTests
{
    [Fact]
    public void A_bonded_Champion_can_bond_only_terrain_adjacent_to_its_flowing_network()
    {
        var g = Load();
        var candidates = Network.BondCandidates(g.State, g.Champion(A));
        Assert.Equal(g.State.Neighbors(H(0, 2)).OrderBy(h => h), candidates);
    }

    [Fact]
    public void An_enemy_with_Knotting_pauses_everything_behind_it_and_a_plain_enemy_does_not()
    {
        var g = Load("""
            bond A 0,1 0,0
            place B test.knotter 0,1
            """);
        Assert.True(Network.IsPaused(g.State, g.Terrain(0, 1)));
        Assert.True(Network.IsPaused(g.State, g.Terrain(0, 0)));
        Assert.True(Network.IsFlowing(g.State, g.Terrain(0, 2)));
        Assert.Null(g.State.Controller(g.Terrain(0, 0)));
        Assert.Equal(1, g.Champion(A).Pool!.Amounts[ManaUnit.Of(Element.Fire)]); // the paused ones didn't credit

        var plain = Load("""
            bond A 0,1 0,0
            place B creature.fire-warrior 0,1
            """);
        Assert.True(Network.IsFlowing(plain.State, plain.Terrain(0, 0)));
        Assert.Equal(3, plain.Champion(A).Pool!.Amounts[ManaUnit.Of(Element.Fire)]);
    }

    [Fact]
    public void A_knot_is_reversible_the_terrain_keeps_its_bond_record()
    {
        var g = Load("""
            bond A 0,1 0,0
            place B test.knotter 0,1
            mana B Fire 3
            handcard A spell.firebolt
            mana A Fire 2
            """);
        var knotter = g.P("test.knotter");
        Assert.Equal(g.Champion(A).Id, g.Terrain(0, 0).Parent);
        g.Ok(g.Cast(A, "spell.firebolt", Obj(knotter)));
        g.ResolveAll();
        Assert.True(Network.IsFlowing(g.State, g.Terrain(0, 0)));
        // D77: credited live the instant the path cleared.
        Assert.True(g.Terrain(0, 0).Drawn);
    }

    [Fact]
    public void Unbonding_a_terrain_in_the_middle_cuts_everything_behind_it()
    {
        var g = Load("""
            bond A 0,1 0,0 1,-1
            handcard B spell.sever
            """);
        g.AdvanceTo(Seat.ChampionB);
        g.Champion(B).Pool!.Add(Element.Darkness, 3);
        g.Ok(g.Cast(B, "spell.sever", Obj(g.Terrain(0, 1))));
        g.ResolveAll();
        Assert.Null(g.Terrain(0, 1).Parent);
        Assert.Null(g.Terrain(0, 0).Parent);
        Assert.Null(g.Terrain(1, -1).Parent);
        Assert.Equal(g.Champion(A).Id, g.Terrain(0, 2).Parent);
    }

    [Fact]
    public void Knotted_terrain_can_be_stolen_and_the_theft_cuts_the_branch_behind_it()
    {
        var g = Load("""
            bond A 0,1 0,0 1,-1
            bond B 0,-1
            place B test.knotter 0,1
            """);
        g.AdvanceTo(Seat.ChampionB);
        var thorn = g.Champion(B);
        Assert.Contains(H(0, 0), Network.BondCandidates(g.State, thorn));
        g.Ok(g.Act(B, thorn, DefaultAbilities.Bond, At(0, 0)));
        g.ResolveAll();
        Assert.Equal(thorn.Id, g.Terrain(0, 0).Parent);
        Assert.Null(g.Terrain(1, -1).Parent); // cut from A: no path left
        Assert.Equal(g.Champion(A).Id, g.Terrain(0, 1).Parent); // still A's, still knotted
        Assert.True(Network.IsFlowing(g.State, g.Terrain(0, 0)));
    }

    [Fact]
    public void Terrain_that_is_merely_bonded_by_the_enemy_cannot_be_bonded()
    {
        var g = Load("bond A 0,1 0,0\nbond B 0,-1");
        g.AdvanceTo(Seat.ChampionB);
        Assert.DoesNotContain(H(0, 0), Network.BondCandidates(g.State, g.Champion(B)));
    }

    [Fact]
    public void A_connected_Champion_is_confined_to_its_realm_and_pays_double_until_it_collapses()
    {
        var g = Load("bond A 0,1");
        var pyra = g.Champion(A);
        Assert.False(g.Act(A, pyra, DefaultAbilities.Move, At(1, 1)).Accepted);
        g.Ok(g.Act(A, pyra, DefaultAbilities.Move, At(0, 1)));
        g.ResolveAll();
        Assert.Equal(2, pyra.CurrentAp);

        g.Ok(g.Act(A, pyra, DefaultAbilities.Collapse));
        g.ResolveAll();
        Assert.Null(g.Terrain(0, 1).Parent);
        Assert.Null(g.Terrain(0, 2).Parent);
        g.Ok(g.Act(A, pyra, DefaultAbilities.Move, At(1, 0)));
        g.ResolveAll();
        Assert.Equal(1, pyra.CurrentAp);
    }

    [Fact]
    public void A_fully_knotted_Champion_stays_confined()
    {
        var g = Load("place B test.knotter 0,2 slice=Sky");
        var pyra = g.Champion(A);
        Assert.False(Network.IsFlowing(g.State, g.Terrain(0, 2)));
        Assert.True(Network.IsRootConnected(g.State, pyra));
        Assert.False(g.Act(A, pyra, DefaultAbilities.Move, At(0, 1)).Accepted);
    }

    [Fact]
    public void A_Companion_bonds_into_its_own_pool()
    {
        var g = Load("place A companion.ash 0,1");
        var ash = g.P("companion.ash");
        g.Ok(g.Act(A, ash, DefaultAbilities.Bond, At(0, 1)));
        g.ResolveAll();
        Assert.Equal(ash.Id, g.Terrain(0, 1).Parent);
        Assert.Equal(1, ash.Pool!.Amounts[ManaUnit.Of(Element.Fire)]);
        Assert.Equal(1, g.Champion(A).Pool!.Amounts[ManaUnit.Of(Element.Fire)]); // the Champion's pool is untouched
        Assert.Equal(A, g.State.Controller(g.Terrain(0, 1))); // controller: the Companion's Champion
        Assert.Equal(2, ash.CurrentAp);
    }

    [Fact]
    public void A_Companion_leaving_its_bonded_tile_cuts_its_network()
    {
        var g = Load("place A companion.ash 0,1");
        var ash = g.P("companion.ash");
        g.Ok(g.Act(A, ash, DefaultAbilities.Bond, At(0, 1)));
        g.ResolveAll();
        g.Ok(g.Act(A, ash, DefaultAbilities.Move, At(1, 0)));
        g.ResolveAll();
        Assert.Null(g.Terrain(0, 1).Parent);
        Assert.Equal(1, ash.CurrentAp); // 2 - 1: an unbonded destination costs the plain price (G10)
    }

    [Fact]
    public void A_Structure_on_paused_terrain_loses_its_controller()
    {
        var g = Load("""
            bond A 0,1
            place A structure.watchtower 0,1
            place B test.knotter 0,1
            """);
        var tower = g.P("structure.watchtower");
        Assert.Null(g.State.Controller(tower));
        Assert.True(g.State.IsDormant(tower));
    }
}
