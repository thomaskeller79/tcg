using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Aether;

/// <summary>D45/D46: casting pushes a Trace onto Pending and opens a priority window; only once
/// both players pass does it resolve, cross Now, and become a Past record (D38). D33: an illegal
/// target at resolution fizzles just that instruction.</summary>
public class PendingResolutionTests
{
    [Fact]
    public void Nothing_else_is_legal_while_a_casts_priority_window_is_open()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 5, hand: [SpellFixtures.Firebolt]);
        var enemyChampion = match.State.ActorsOwnedBy(Fixtures.P2).Single();

        RulesEngine.Apply(match, new CastSpellCommand(Fixtures.P1, SpellFixtures.Firebolt, enemyChampion.Id));

        // P2 holds priority first (opponent-first, same order Combat uses) — Pass is the only
        // legal command for anyone while the window is open.
        Assert.Equal([new PassPriorityCommand(Fixtures.P2)], RulesEngine.LegalCommands(match, Fixtures.P2));
        Assert.Empty(RulesEngine.LegalCommands(match, Fixtures.P1));
    }

    [Fact]
    public void A_creature_summon_that_becomes_illegal_before_it_resolves_fizzles_without_refunding_cost()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 5, hand: [SpellFixtures.Grunt]);
        var target = new HexCoord(1, 0);

        RulesEngine.Apply(match, new CastCreatureCommand(Fixtures.P1, SpellFixtures.Grunt, target));

        // Nothing can legally contest the target while the window is open in M1 (no instant-speed
        // content exists yet to do it for real) — mutate state directly to simulate what a future
        // Instant response would do, and confirm the resolution-time re-check (D33) catches it.
        match.State.AddActor(new CreatureState
        {
            Id = match.State.AllocateActorId(),
            Owner = Fixtures.P2,
            Definition = SpellFixtures.Grunt,
            Position = target,
            Life = 5,
            CurrentAp = 3,
        });

        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1));

        Assert.Empty(match.State.ActorsOwnedBy(Fixtures.P1).OfType<CreatureState>()); // fizzled
        var p1 = match.State.Players.Single(p => p.Id == Fixtures.P1);
        Assert.Empty(p1.Hand); // card/mana already spent at cast time — never refunded
        Assert.Equal([SpellFixtures.Grunt], p1.Discard);
        Assert.Equal(3, p1.Mana); // 5 - ManaCost:2
    }

    [Fact]
    public void A_resolved_trace_becomes_a_Past_record_and_fades_after_the_default_duration()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 5, hand: [SpellFixtures.Firebolt]);
        var enemyChampion = match.State.ActorsOwnedBy(Fixtures.P2).Single();

        RulesEngine.Apply(match, new CastSpellCommand(Fixtures.P1, SpellFixtures.Firebolt, enemyChampion.Id));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1));

        var trace = Assert.Single(match.State.Past);
        Assert.Equal(Fixtures.P1, trace.Controller);
        Assert.Equal(1, trace.CreatedAtRound);
        Assert.Equal(1 + TrueState.DefaultTraceDuration, trace.FadesAtRound);

        // Advance 5 full rounds (2 EndPhase calls per round — P1's own turn, then P2's, each
        // auto-cascading through the neutral turn between them, D60), then one more End phase
        // so round 6's own End-phase fade check (ExpireFadedTracesEffect) actually runs —
        // RoundNumber reads 6 the instant round 6's first turn begins, but the check only fires
        // when an End phase is entered, so this test needs to reach one, not just the round number.
        for (var round = 0; round < TrueState.DefaultTraceDuration; round++)
        {
            RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1));
            RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2));
        }
        Assert.Equal(1 + TrueState.DefaultTraceDuration, match.State.RoundNumber);
        Assert.Single(match.State.Past); // round 6 has begun, but nothing has re-checked fade yet

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1)); // -> round 6's own End phase
        Assert.Empty(match.State.Past); // faded
    }
}
