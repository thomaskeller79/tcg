using System.Text;
using Leyline.Host;
using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Modifiers;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests;

/// <summary>
/// A4/pillar 4: seed + config + ordered command log must deterministically reproduce the
/// same final state. Two comparisons: (a) the same log run twice directly against
/// RulesEngine, and (b) once direct vs. once through LocalHost — the second is what would
/// catch Perception/Host-layer nondeterminism that pure-engine replay alone would miss.
/// </summary>
public class DeterminismReplayTests
{
    [Fact]
    public void Replaying_the_same_command_log_twice_produces_identical_state()
    {
        var matchA = Fixtures.Adjacent1v1();
        var matchB = Fixtures.Adjacent1v1(); // identical construction -> identical ids, deterministically

        var attacker = matchA.State.ActorsOwnedBy(Fixtures.P1).Single().Id;
        var defender = matchA.State.ActorsOwnedBy(Fixtures.P2).Single().Id;

        RunScript(matchA, attacker, defender);
        RunScript(matchB, attacker, defender);

        Assert.Equal(Snapshot(matchA.State), Snapshot(matchB.State));
    }

    [Fact]
    public void Adding_a_modifier_and_expiring_it_replays_identically_across_two_matches()
    {
        var matchA = Fixtures.Adjacent1v1();
        var matchB = Fixtures.Adjacent1v1(); // identical construction -> identical ids, deterministically

        var attackerA = matchA.State.ActorsOwnedBy(Fixtures.P1).Single().Id;
        var defenderA = matchA.State.ActorsOwnedBy(Fixtures.P2).Single().Id;
        var attackerB = matchB.State.ActorsOwnedBy(Fixtures.P1).Single().Id;
        var defenderB = matchB.State.ActorsOwnedBy(Fixtures.P2).Single().Id;

        var beforeExpiryA = RunScriptWithModifier(matchA, attackerA, defenderA);
        var beforeExpiryB = RunScriptWithModifier(matchB, attackerB, defenderB);

        // Same deterministic construction call (same AllocateModifierId sequence, value-equal
        // modifier record) -> the modifier itself, not just downstream behavior, replays
        // identically while still active.
        Assert.Equal(beforeExpiryA, beforeExpiryB);
        Assert.Contains("Modifier IntDeltaModifier", beforeExpiryA);

        Assert.Equal(Snapshot(matchA.State), Snapshot(matchB.State));
        Assert.Empty(matchA.State.ActiveModifiers); // expired by the End phase in both

        return;

        /// <summary>Same script as RunScript, plus a mid-script modifier add that expires when
        /// the End phase is reached. Returns a snapshot taken right after the add, before
        /// expiry, so the caller can compare the still-active modifier too.</summary>
        static string RunScriptWithModifier(Match match, ActorId attacker, ActorId defender)
        {
            RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker, match.State.GetActor(defender).Position));
            var combatId = match.State.ActiveCombats.Single().Id;
            RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, [defender]));
            ModifierPipeline.AddModifier(match.State, match.Pipeline,
                id => new IntDeltaModifier(id, "Attack", attacker, Delta: 1, ModifierDuration.UntilEndOfTurn));
            var beforeExpiry = Snapshot(match.State);
            RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));
            RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1));
            RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1));
            RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2));
            return beforeExpiry;
        }
    }

    [Fact]
    public void Replaying_through_LocalHost_matches_a_direct_RulesEngine_replay()
    {
        var matchDirect = Fixtures.Adjacent1v1();
        var matchHosted = Fixtures.Adjacent1v1();

        var attacker = matchDirect.State.ActorsOwnedBy(Fixtures.P1).Single().Id;
        var defender = matchDirect.State.ActorsOwnedBy(Fixtures.P2).Single().Id;

        RunScript(matchDirect, attacker, defender);

        var p1Seat = new SeatId(1);
        var p2Seat = new SeatId(2);
        var host = new LocalHost(matchHosted, new Dictionary<SeatId, PlayerId> { [p1Seat] = Fixtures.P1, [p2Seat] = Fixtures.P2 });
        RunScriptViaHost(host, p1Seat, p2Seat, attacker, defender);

        Assert.Equal(Snapshot(matchDirect.State), Snapshot(matchHosted.State));
    }

    /// <summary>Declare combat, resolve it, then cycle one full turn — exercises Combat, the
    /// priority window, and the turn/phase machine together in one deterministic script.</summary>
    private static void RunScript(Match match, ActorId attacker, ActorId defender)
    {
        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, attacker, match.State.GetActor(defender).Position));
        var combatId = match.State.ActiveCombats.Single().Id;
        RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, [defender]));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1));
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1));
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2));
    }

    private static void RunScriptViaHost(IHost host, SeatId p1Seat, SeatId p2Seat, ActorId attacker, ActorId defender)
    {
        var defenderPosition = host.LegalCommands(p1Seat)
            .OfType<DeclareCombatCommand>().Single(c => c.Attacker == attacker).TargetHex;

        host.Submit(p1Seat, new DeclareCombatCommand(Fixtures.P1, attacker, defenderPosition));
        var combatId = host.LegalCommands(p2Seat).OfType<DeclareDefendersCommand>().First().Combat;
        host.Submit(p2Seat, new DeclareDefendersCommand(Fixtures.P2, combatId, [defender]));
        host.Submit(p2Seat, new PassPriorityCommand(Fixtures.P2));
        host.Submit(p1Seat, new PassPriorityCommand(Fixtures.P1));
        host.Submit(p1Seat, new EndPhaseCommand(Fixtures.P1));
        host.Submit(p2Seat, new EndPhaseCommand(Fixtures.P2));
    }

    private static string Snapshot(TrueState state)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Turn={state.TurnNumber} Active={state.ActivePlayer} Phase={state.CurrentPhaseIndex} Winner={state.Winner}");
        foreach (var p in state.Players.OrderBy(p => p.Id.Value))
            sb.AppendLine($"Player {p.Id} Mana={p.Mana}");
        foreach (var a in state.AllActors) // AllActors is already in canonical Id order
            sb.AppendLine($"Actor {a.Id} Owner={a.Owner} Life={a.Life} AP={a.CurrentAp} Pos={a.Position} Layer={a.Layer} Located={a.Located}");
        foreach (var m in state.ActiveModifiers) // record ToString() is deterministic/value-based
            sb.AppendLine($"Modifier {m}");
        return sb.ToString();
    }
}
