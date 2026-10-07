using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Model;
using Leyline.RulesCore.Perception;
using Leyline.RulesCore.Rng;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.Support;
using Xunit.Abstractions;

namespace Leyline.RulesCore.Tests;

/// <summary>Plays random legal commands for both Champions on a full-size match and checks that
/// every offered command is accepted and the rules' invariants hold after each one.</summary>
public class FuzzTests(ITestOutputHelper output)
{
    public const string Match = """
        name Fuzz
        map hexagon 4
        void 2,-1 -2,1
        start A 0,3
        start B 0,-3
        home A radius=1
        home B radius=1
        neutral random terrain.light terrain.fire terrain.metal terrain.earth terrain.darkness terrain.ice terrain.water terrain.air terrain.mire terrain.forge
        champion A champion.pyra
        champion B champion.thorn
        terraindeck A terrain.fire*3 terrain.metal*2 terrain.air terrain.light
        terraindeck B terrain.earth*3 terrain.water*2 terrain.darkness*2
        deck A creature.fire-warrior*2 creature.ember-imp*2 creature.flame-archer creature.iron-golem creature.sky-hawk creature.herald creature.cinder-spirit companion.ash structure.watchtower structure.barrack item.flame-blade item.iron-shield item.spark-amulet spell.firebolt*2 spell.flame-dart spell.battle-surge spell.blink spell.smite spell.study
        deck B creature.stone-brute*2 creature.burrower*2 creature.marsh-lurker creature.night-shade creature.tide-caller companion.moss structure.thorn-wall structure.root-cellar spell.mend spell.quicken spell.raise spell.recall spell.sever spell.stone-shards spell.study creature.wild-boar
        neutralpermanent creature.wild-boar 0,0 behavior=Aggressive:A seat=NeutralA
        neutralpermanent creature.wild-boar 1,-1 behavior=Aggressive:B seat=NeutralB
        """;

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void Random_legal_play_is_always_accepted_and_keeps_the_invariants(int seed)
    {
        var g = Game.Load($"seed {seed}\n" + Match, withBase: false);
        var rng = RngState.FromSeed((ulong)seed * 7919);
        var steps = 0;
        for (; steps < 1500 && !g.State.IsOver; steps++)
        {
            var actor = g.State.Decision?.Decider ?? g.State.PriorityHolder;
            Assert.NotNull(actor);
            var legal = RulesEngine.LegalCommands(g.State, actor!.Value);
            Assert.NotEmpty(legal);
            // Prefer acting over passing so the board fills up.
            var pick = legal.Count > 1 && legal[0] is PassCommand ? PickNonPass(legal, ref rng) : PickAny(legal, ref rng);
            var result = RulesEngine.Apply(g.State, pick);
            Assert.True(result.Accepted, $"step {steps}: {pick} rejected: {result.Error}");
            CheckInvariants(g.State);
            _ = ViewProjector.Project(g.State, PlayerId.A);
            _ = ViewProjector.Project(g.State, PlayerId.B);
        }
        output.WriteLine($"seed {seed}: {steps} steps, round {g.State.Round}, winner {g.State.Winner?.ToString() ?? (g.State.IsDraw ? "draw" : "none")}, permanents {g.State.Permanents.Count}");
    }

    private static Command PickNonPass(IReadOnlyList<Command> legal, ref RngState rng)
    {
        var (roll, next) = rng.NextInt(4);
        rng = next;
        if (roll == 0)
            return legal[0]; // pass sometimes, so turns advance
        var (i, next2) = rng.NextInt(legal.Count - 1);
        rng = next2;
        return legal[i + 1];
    }

    private static Command PickAny(IReadOnlyList<Command> legal, ref RngState rng)
    {
        var (i, next) = rng.NextInt(legal.Count);
        rng = next;
        return legal[i];
    }

    internal static void CheckInvariants(TrueState state)
    {
        // Every hex holds exactly one terrain (D100).
        Assert.Equal(state.TerrainAt.Count, state.Permanents.Count(p => p.Kind == PermanentKind.Terrain));
        foreach (var hex in state.Hexes)
        {
            foreach (var slice in new[] { Slice.Root, Slice.Ground, Slice.Sky })
                Assert.True(state.CreaturesIn(hex, slice).Count <= Island.SliceCapacity, $"{hex} {slice} over capacity");
            Assert.True(state.Permanents.Count(p => p.Kind == PermanentKind.Structure && p.Hex == hex && p.Slice == Slice.Ground) <= 1);
            Assert.True(state.Permanents.Count(p => p.Kind == PermanentKind.Structure && p.Hex == hex && p.Slice == Slice.Root) <= 1);
        }

        // Every object is in exactly one zone (D16).
        var seen = new HashSet<ObjectId>();
        foreach (var ps in state.Players)
            foreach (var id in ps.Hand.Concat(ps.Library).Concat(ps.Discard))
                Assert.True(seen.Add(id), $"{id} in two Mind zones");
        foreach (var id in state.Pending.Concat(state.Past).Concat(state.Future))
            Assert.True(seen.Add(id), $"{id} in two Aether zones");
        foreach (var p in state.Permanents)
        {
            Assert.True(seen.Add(p.Id), $"{p.Id} is a permanent and in another zone");
            Assert.False(p.Kind == PermanentKind.Creature && p.Slice == Slice.Root && !state.Def(p).HasKeyword(Keyword.Subterranean) && state.Def(p).EntersSlice != Slice.Root,
                $"{p.Id} in Root without Subterranean");
            if (p.Kind == PermanentKind.Item && p.Carrier is { } c)
                Assert.True(state.Exists(c), $"{p.Id} carried by a gone carrier");
            if (p.Kind.IsActor())
                Assert.True(p.CurrentAp >= 0, $"{p.Id} has negative AP");
            Assert.True(p.Kind == PermanentKind.Terrain || state.IsOnBoard(p.Hex));
        }

        // A bonded terrain always has a path to its root (D122: cuts are immediate).
        foreach (var t in state.Permanents.Where(p => p.Kind == PermanentKind.Terrain && p.Parent is not null))
        {
            var root = state.Get<Permanent>(t.Parent!.Value);
            Assert.Contains(t.Hex, Network.Connected(state, root));
        }

        // Mana pools never go negative.
        foreach (var root in state.Permanents.Where(p => p.Pool is not null))
            Assert.All(root.Pool!.Amounts.Values, v => Assert.True(v > 0));
    }
}
