using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.Support;
using static Leyline.RulesCore.Tests.Support.Game;

namespace Leyline.RulesCore.Tests;

/// <summary>Combat (D13, D115–D120) and the scenarios of examples/combat-scenarios.md.</summary>
public class CombatTests
{
    private static TraceObject TopTrace(Game g) => g.State.Get<TraceObject>(g.State.Pending[^1]);

    [Fact]
    public void S1_mirror_trade_both_fall()
    {
        var g = Load("""
            place A test.mirror 0,1
            place B test.mirror 0,0
            """);
        var a1 = g.P("test.mirror");
        var b1 = g.P("test.mirror", 1);

        g.Ok(g.Act(A, a1, DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, B)));
        Assert.Equal(1, a1.CurrentAp);
        Assert.True(a1.Locked);
        g.Pass(A);
        g.Ok(g.Act(B, b1, DefaultAbilities.Defend, Obj(TopTrace(g))));
        g.ResolveAll();
        Assert.Equal(2, a1.CurrentLife);
        Assert.Equal(2, b1.CurrentLife);

        g.AdvanceTo(Seat.ChampionB);
        g.Ok(g.Act(B, b1, DefaultAbilities.Attack, AttackAt(0, 1, Slice.Ground, A)));
        g.Pass(B);
        g.Ok(g.Act(A, a1, DefaultAbilities.Defend, Obj(TopTrace(g))));
        g.ResolveAll();
        Assert.Null(g.Find("test.mirror"));
        Assert.Null(g.Find("test.mirror", 1));
        Assert.Equal(2, g.Remnants.Count);
    }

    [Fact]
    public void S3_a_creature_defends_once_against_each_opponent_turn()
    {
        var g = Load("""
            place A test.brute 0,0
            place B test.grunt 0,-1
            neutralpermanent creature.wild-boar 1,-1 behavior=Aggressive:A seat=NeutralA
            """);
        var brute = g.P("test.brute");
        var boar = g.P("creature.wild-boar");
        boar.CurrentAp = 3; // as if it had been around for a turn

        g.Pass(A);
        g.Pass(B);
        // Neutral A: the boar attacks the brute.
        Assert.Equal(Seat.NeutralA, g.State.ActiveSeat);
        var attack = TopTrace(g);
        Assert.NotNull(attack.Attack);
        Assert.Equal(B, g.State.PriorityHolder);
        g.Pass(B);
        g.Ok(g.Act(A, brute, DefaultAbilities.Defend, Obj(attack)));
        g.ResolveAll();
        Assert.Equal(5, brute.CurrentLife);
        Assert.Null(g.Find("creature.wild-boar")); // 3 damage back felled it

        g.AdvanceTo(Seat.ChampionB);
        g.Ok(g.Act(B, g.P("test.grunt"), DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, A)));
        g.Pass(B);
        Assert.Contains(RulesEngine.LegalCommands(g.State, A), c => c is ActivateCommand { Ability: DefaultAbilities.Defend });
        g.Ok(g.Act(A, brute, DefaultAbilities.Defend, Obj(TopTrace(g))));
        g.ResolveAll();
        Assert.Equal(2, brute.CurrentLife);
        Assert.Equal(2, g.P("test.grunt").CurrentLife);
    }

    [Fact]
    public void Undefended_the_attacker_hits_without_retaliation()
    {
        var g = Load("""
            place A test.grunt 0,1
            place B test.brute 0,0
            """);
        g.Ok(g.Act(A, g.P("test.grunt"), DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, B)));
        g.ResolveAll();
        Assert.Equal(4, g.P("test.brute").CurrentLife);
        Assert.Equal(5, g.P("test.grunt").CurrentLife);
    }

    [Fact]
    public void An_attack_whose_only_target_left_does_nothing_and_its_AP_stays_spent()
    {
        var g = Load("""
            place A test.grunt 0,1
            place B test.brute 0,0
            handcard B spell.recall
            """);
        g.Ok(g.Act(A, g.P("test.grunt"), DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, B)));
        g.Pass(A);
        g.Champion(B).Pool!.Add(Element.Water, 2);
        g.Ok(g.Cast(B, "spell.recall", Obj(g.P("test.brute"))));
        g.ResolveAll();
        Assert.Null(g.Find("test.brute"));
        Assert.Equal(1, g.P("test.grunt").CurrentAp);
    }

    [Fact]
    public void Two_defenders_make_the_attacker_split_its_damage_and_both_hit_back()
    {
        var g = Load("""
            place A test.brute 0,1
            place B test.grunt 0,0
            place B test.mirror 0,0
            """);
        var attacker = g.P("test.brute");
        g.Ok(g.Act(A, attacker, DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, B)));
        var attack = TopTrace(g);
        g.Pass(A);
        g.Ok(g.Act(B, g.P("test.grunt"), DefaultAbilities.Defend, Obj(attack)));
        g.Pass(B);
        g.Pass(A); // Defend 1 resolves; priority returns to the active Champion
        g.Pass(A);
        g.Ok(g.Act(B, g.P("test.mirror"), DefaultAbilities.Defend, Obj(attack)));
        g.ResolveAll();
        var decision = Assert.IsType<DamageSplitDecision>(g.State.Decision);
        Assert.Equal(A, decision.Decider);
        Assert.Equal(3, decision.Amount);

        g.Ok(g.Apply(new SplitDamageCommand(A, new Dictionary<ObjectId, int> { [g.P("test.grunt").Id] = 1, [g.P("test.mirror").Id] = 2 })));
        Assert.Equal(4, g.P("test.grunt").CurrentLife);
        Assert.Equal(3, g.P("test.mirror").CurrentLife);
        Assert.Equal(1, attacker.CurrentLife); // 7 - 3 - 3
        Assert.Null(g.State.Decision);
        Assert.Empty(g.State.Pending);
    }

    [Fact]
    public void Ranged_1_2_reaches_distance_1_and_2_but_not_3()
    {
        var g = Load("""
            place A creature.flame-archer 0,2
            place B test.grunt 0,1
            place B test.grunt 0,0
            place B test.grunt 0,-1
            """);
        var reached = Combat.AttackCandidates(g.State, g.P("creature.flame-archer"), A).Select(c => c.Hex).ToList();
        Assert.Contains(H(0, 1), reached);
        Assert.Contains(H(0, 0), reached);
        Assert.DoesNotContain(H(0, -1), reached);
    }

    [Fact]
    public void A_melee_creature_cannot_defend_against_an_attacker_beyond_its_reach()
    {
        var g = Load("""
            place A creature.flame-archer 0,2
            place B test.grunt 0,0
            """);
        var archer = g.P("creature.flame-archer");
        g.Ok(g.Act(A, archer, DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, B)));
        g.Pass(A);
        Assert.False(g.Act(B, g.P("test.grunt"), DefaultAbilities.Defend, Obj(TopTrace(g))).Accepted); // D127: distance 2 > reach 1
        g.ResolveAll();
        Assert.Equal(3, g.P("test.grunt").CurrentLife);
        Assert.Equal(2, archer.CurrentLife);
    }

    [Fact]
    public void A_Ranged_defender_in_reach_defends_and_hits_back()
    {
        var g = Load("""
            place A creature.flame-archer 0,2
            place B creature.flame-archer 0,0
            """);
        var mine = g.P("creature.flame-archer");
        var theirs = g.P("creature.flame-archer", 1);
        g.Ok(g.Act(A, mine, DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, B)));
        g.Pass(A);
        g.Ok(g.Act(B, theirs, DefaultAbilities.Defend, Obj(TopTrace(g))));
        g.ResolveAll();
        Assert.Null(g.Find("creature.flame-archer")); // both fell: every defender deals its Attack back (D127)
        Assert.Null(g.Find("creature.flame-archer", 1));
    }

    [Fact]
    public void Slice_rule_Ground_cannot_attack_Sky_but_Sky_can_attack_Ground()
    {
        var g = Load("""
            place A test.grunt 0,1
            place B creature.sky-hawk 0,0
            place A creature.sky-hawk 0,1
            place B test.brute 1,0
            """);
        Assert.DoesNotContain(Combat.AttackCandidates(g.State, g.P("test.grunt"), A), c => c.Hex == H(0, 0));
        var hawkCandidates = Combat.AttackCandidates(g.State, g.P("creature.sky-hawk", 1), A);
        Assert.Contains(hawkCandidates, c => c.Hex == H(1, 0) && c.Slice == Slice.Ground && c.Entity == B);
        Assert.Contains(hawkCandidates, c => c.Hex == H(0, 0) && c.Slice == Slice.Sky && c.Entity == B);
    }

    [Fact]
    public void Sky_defends_Ground_but_Ground_never_defends_Sky()
    {
        var g = Load("""
            place A creature.sky-hawk 0,1
            place B creature.sky-hawk 0,0
            place B test.grunt 0,0
            """);
        g.Ok(g.Act(A, g.P("creature.sky-hawk"), DefaultAbilities.Attack, AttackAt(0, 0, Slice.Sky, B)));
        var attack = TopTrace(g);
        Assert.False(Combat.CanDefend(g.State, g.P("test.grunt"), attack));
        Assert.True(Combat.CanDefend(g.State, g.P("creature.sky-hawk", 1), attack));

        var g2 = Load("""
            place A test.grunt 0,1
            place B creature.sky-hawk 0,0
            place B test.brute 0,0
            """);
        g2.Ok(g2.Act(A, g2.P("test.grunt"), DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, B)));
        Assert.True(Combat.CanDefend(g2.State, g2.P("creature.sky-hawk"), g2.State.Get<TraceObject>(g2.State.Pending[^1])));
    }

    [Fact]
    public void After_attacking_a_creature_is_done_for_the_turn_but_keeps_its_AP_for_defending()
    {
        var g = Load("""
            place A test.grunt 0,1
            place B test.brute 0,0
            """);
        var grunt = g.P("test.grunt");
        g.Ok(g.Act(A, grunt, DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, B)));
        g.ResolveAll();
        Assert.False(g.Act(A, grunt, DefaultAbilities.Move, At(1, 1)).Accepted);
        g.AdvanceTo(Seat.ChampionB);
        Assert.False(grunt.Locked);
        Assert.Equal(1, grunt.CurrentAp);
    }

    [Fact]
    public void Defender_keyword_defends_any_number_of_times_while_AP_lasts()
    {
        var g = Load("""
            place A creature.iron-golem 0,0
            place B test.grunt 0,-1
            place B test.mirror 1,-1
            """);
        var golem = g.P("creature.iron-golem");
        g.AdvanceTo(Seat.ChampionB);
        foreach (var attacker in new[] { g.P("test.grunt"), g.P("test.mirror") })
        {
            g.Ok(g.Act(B, attacker, DefaultAbilities.Attack, AttackAt(0, 0, Slice.Ground, A)));
            g.Pass(B);
            g.Ok(g.Act(A, golem, DefaultAbilities.Defend, Obj(TopTrace(g))));
            g.ResolveAll();
        }
        // It defended both attacks (3 + 3 damage fells it) and hit back both times.
        Assert.Null(g.Find("creature.iron-golem"));
        Assert.Equal(2, g.P("test.grunt").CurrentLife);
        Assert.Equal(2, g.P("test.mirror").CurrentLife);
    }

    [Fact]
    public void A_bonded_Champion_attacks_for_6_and_both_Champions_falling_together_is_a_draw()
    {
        var g = Load("""
            startap 7
            """.Replace("startap 7", "") + "\nplace A test.grunt 2,-2");
        var pyra = g.Champion(A);
        var thorn = g.Champion(B);
        // Bring the Champions next to each other with low Life.
        g.Ok(g.Act(A, pyra, DefaultAbilities.Collapse));
        g.ResolveAll();
        pyra.Hex = H(0, -1);
        pyra.CurrentLife = 2;
        thorn.CurrentLife = 2;
        pyra.CurrentAp = 7;
        g.Ok(g.Act(A, pyra, DefaultAbilities.Attack, AttackAt(0, -2, Slice.Ground, B)));
        Assert.Equal(4, pyra.CurrentAp); // disconnected: 3~AP
        g.Pass(A);
        g.Ok(g.Act(B, thorn, DefaultAbilities.Defend, Obj(TopTrace(g))));
        Assert.Equal(2, thorn.CurrentAp); // bonded Champion: 2~AP
        g.ResolveAll();
        Assert.True(g.State.IsDraw);
        Assert.Empty(RulesEngine.LegalCommands(g.State, A));
    }

    [Fact]
    public void Killing_the_enemy_Champion_wins()
    {
        var g = Load("place A test.brute 0,-1");
        g.Champion(B).CurrentLife = 3;
        g.Ok(g.Act(A, g.P("test.brute"), DefaultAbilities.Attack, AttackAt(0, -2, Slice.Ground, B)));
        g.ResolveAll();
        Assert.Equal(A, g.State.Winner);
    }
}
