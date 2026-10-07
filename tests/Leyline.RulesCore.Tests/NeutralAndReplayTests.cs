using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Model;
using Leyline.RulesCore.Perception;
using Leyline.RulesCore.Rng;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.Tests.Support;
using static Leyline.RulesCore.Tests.Support.Game;

namespace Leyline.RulesCore.Tests;

/// <summary>neutral-permanents.md: neutral turns, the Aggressive Behavior (G19).</summary>
public class NeutralTests
{
    [Fact]
    public void An_Aggressive_Neutral_walks_toward_its_enemy_in_its_own_neutral_turn_only()
    {
        var g = Load("neutralpermanent creature.wild-boar 2,-2 behavior=Aggressive:A seat=NeutralB");
        var boar = g.P("creature.wild-boar");
        g.AdvanceTo(Seat.NeutralA);
        Assert.Equal(H(2, -2), boar.Hex); // not its seat: no refresh, no action
        g.AdvanceTo(Seat.NeutralB);
        // It refreshed and is walking toward Champion A at (0,2).
        g.ResolveAll();
        Assert.True(boar.Hex.DistanceTo(H(0, 2)) < H(2, -2).DistanceTo(H(0, 2)));
    }

    [Fact]
    public void An_Aggressive_Neutral_attacks_when_it_can()
    {
        var g = Load("""
            place A test.grunt 0,0
            neutralpermanent creature.wild-boar 0,-1 behavior=Aggressive:A seat=NeutralA
            """);
        g.P("creature.wild-boar").CurrentAp = 3;
        g.AdvanceTo(Seat.NeutralA);
        g.ResolveAll();
        Assert.Equal(3, g.P("test.grunt").CurrentLife);
    }

    [Fact]
    public void A_Neutral_defends_an_attack_on_it_through_its_Behavior()
    {
        var g = Load("""
            place A test.grunt 0,0
            neutralpermanent creature.wild-boar 0,-1 behavior=Aggressive:A seat=NeutralA
            """);
        var boar = g.P("creature.wild-boar");
        boar.CurrentAp = 3;
        g.Ok(g.Act(A, g.P("test.grunt"), DefaultAbilities.Attack, AttackAt(0, -1, Slice.Ground, null)));
        g.Pass(A);
        g.Pass(B);
        // Before the Attack resolves, the boar adds itself as a defender.
        Assert.Equal(2, g.State.Pending.Count);
        g.ResolveAll();
        Assert.Equal(3, g.P("test.grunt").CurrentLife); // it hit back
        Assert.Null(g.Find("creature.wild-boar"));
    }
}

/// <summary>Determinism (overview.md §8): seed + commands → the same match.</summary>
public class ReplayTests
{
    [Fact]
    public void Replaying_the_same_commands_on_the_same_seed_gives_the_same_state()
    {
        var first = Load("seed 11\n" + FuzzTests.Match, withBase: false);
        var commands = new List<Command>();
        var rng = RngState.FromSeed(3);
        for (var i = 0; i < 400 && !first.State.IsOver; i++)
        {
            var actor = first.State.Decision?.Decider ?? first.State.PriorityHolder!.Value;
            var legal = RulesEngine.LegalCommands(first.State, actor);
            var (pick, next) = rng.NextInt(legal.Count);
            rng = next;
            commands.Add(legal[pick]);
            first.Ok(first.Apply(legal[pick]));
        }

        var second = Load("seed 11\n" + FuzzTests.Match, withBase: false);
        foreach (var c in commands)
            second.Ok(second.Apply(c));

        Assert.Equal(first.Log, second.Log);
        var v1 = ViewProjector.Project(first.State, null, omniscient: true);
        var v2 = ViewProjector.Project(second.State, null, omniscient: true);
        Assert.Equal(v1.Permanents.Select(p => (p.Id, p.Hex, p.Life, p.Ap)), v2.Permanents.Select(p => (p.Id, p.Hex, p.Life, p.Ap)));
    }
}
