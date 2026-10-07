using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// Combat (D13, D115–D120; interaction-stack.md §Combat integration). An Attack is a Physical
/// Trace naming a terrain, a Slice and an entity. Defend is a Quick ability whose trace adds its
/// creature to the Attack. When the Attack resolves it checks whether it still has a defender:
/// defended → the attacker splits its Attack among the defenders and each deals its Attack back;
/// undefended → the attacker splits among the entity's permanents there, no retaliation.
/// </summary>
public static class Combat
{
    /// <summary>D115 Slice rule: an attacker attacks its own Slice, plus Sky → Ground.</summary>
    public static IEnumerable<Slice> AttackableSlices(Slice attacker) =>
        attacker == Slice.Sky ? [Slice.Sky, Slice.Ground] : [attacker];

    /// <summary>The entity's permanents in an attacked Slice of a terrain: creature-type
    /// permanents in that Slice, plus a Structure in the matching slot (G8 — the Ground slot
    /// counts as Ground, the Root slot as Root).</summary>
    public static IReadOnlyList<Permanent> TargetsIn(TrueState state, HexCoord hex, Slice slice, PlayerId? entity, ObjectId? exclude = null)
    {
        var list = state.CreaturesIn(hex, slice).Where(p => p.Id != exclude && state.Controller(p) == entity).ToList();
        if (slice != Slice.Sky && state.StructureIn(hex, slice) is { } structure && state.Controller(structure) == entity && structure.Id != exclude)
            list.Add(structure);
        return list;
    }

    private static readonly PlayerId?[] Entities = [PlayerId.A, PlayerId.B, null];

    /// <summary>Legal attack targets for <paramref name="attacker"/>, in the view of
    /// <paramref name="observer"/> (D115: at least one permanent of the attacked entity must be
    /// visible in the attacked Slice of that terrain — so an attack can't probe for hidden ones).
    /// Melee reaches distance ≤ 1 (D120), Ranged N within N (G7).</summary>
    public static IReadOnlyList<TargetChoice> AttackCandidates(TrueState state, Permanent attacker, PlayerId? observer)
    {
        if (!attacker.Kind.IsCreatureType() || attacker.Carrier is not null)
            return [];
        var own = state.Controller(attacker);
        var result = new List<TargetChoice>();
        foreach (var hex in state.WithinDistance(attacker.Hex, state.Range(attacker)))
        {
            foreach (var slice in AttackableSlices(attacker.Slice))
            {
                foreach (var entity in Entities)
                {
                    if (entity == own)
                        continue;
                    if (TargetsIn(state, hex, slice, entity, attacker.Id).Any(p => state.CanSee(observer, p)))
                        result.Add(TargetChoice.ForAttack(hex, slice, entity));
                }
            }
        }
        return result;
    }

    /// <summary>D115/D117: who may defend — a creature-type permanent of the attacked entity on
    /// the attacked terrain, in the attacked Slice (Sky creatures also defend Ground).</summary>
    public static bool IsEligibleDefender(TrueState state, Permanent defender, AttackInfo attack, ObjectId attacker)
    {
        if (!defender.Kind.IsCreatureType() || defender.Carrier is not null || defender.Id == attacker)
            return false;
        if (defender.Hex != attack.Hex || state.Controller(defender) != attack.Entity)
            return false;
        return defender.Slice == attack.Slice || (attack.Slice == Slice.Ground && defender.Slice == Slice.Sky);
    }

    public static bool CanDefend(TrueState state, Permanent defender, TraceObject attackTrace) =>
        attackTrace.Attack is { } attack
        && attackTrace.Zone == Zone.Pending
        && attackTrace.ActingPermanent is { } attacker
        && !attack.Defenders.Contains(defender.Id)
        && IsEligibleDefender(state, defender, attack, attacker);

    public static IReadOnlyList<TraceObject> DefendableAttacks(TrueState state, Permanent defender) =>
        state.Pending.Select(state.Get<TraceObject>).Where(t => CanDefend(state, defender, t)).ToList();

    /// <summary>The Defend trace resolves: its creature is added to the Attack trace as a
    /// defender, if it and the Attack are still there and it is still eligible.</summary>
    public static void ResolveDefend(TrueState state, TraceObject defend)
    {
        var target = defend.Targets.Values.SelectMany(v => v).FirstOrDefault()?.Object;
        if (defend.ActingPermanent is not { } defenderId || state.Find<Permanent>(defenderId) is not { } defender)
        {
            defend.Notes.Add("fizzled: the defender is gone");
            return;
        }
        if (target is not { } attackId || state.Find<TraceObject>(attackId) is not { } attackTrace || !CanDefend(state, defender, attackTrace))
        {
            defend.Notes.Add("fizzled: the attack can't be defended any more");
            return;
        }
        attackTrace.Attack!.Defenders.Add(defender.Id);
        defend.Notes.Add($"{state.NameOf(defender.Id)} defends");
        state.Note($"{state.NameOf(defender.Id)} defends against {attackTrace.Text}.", defender.Id, attackTrace.Id);
    }

    /// <summary>Resolves an Attack trace. Returns true if it is suspended for the attacker's
    /// damage split (G16).</summary>
    public static bool ResolveAttack(TrueState state, TraceObject trace)
    {
        var attack = trace.Attack!;
        if (trace.ActingPermanent is not { } attackerId || state.Find<Permanent>(attackerId) is not { } attacker)
        {
            trace.Notes.Add("fizzled: the attacker is gone");
            return false;
        }

        var defenders = attack.Defenders
            .Select(state.Find<Permanent>)
            .Where(d => d is not null && IsEligibleDefender(state, d, attack, attackerId))
            .Select(d => d!)
            .ToList();
        var defended = defenders.Count > 0;
        var attackerSide = state.Controller(attacker);
        var candidates = defended
            ? defenders
            : TargetsIn(state, attack.Hex, attack.Slice, attack.Entity, attackerId).Where(p => state.CanSee(attackerSide, p)).ToList();

        if (candidates.Count == 0)
        {
            trace.Notes.Add("does nothing: no target is left");
            state.Note($"{trace.Text} finds nothing to hit.", trace.Id);
            return false;
        }

        var amount = state.Attack(attacker);
        if (amount == 0 || candidates.Count == 1 || attackerSide is null)
        {
            // G16: one recipient takes it all; a Neutral attacker's Behavior picks the
            // lowest-Life candidate, ties by ID.
            var recipient = candidates.OrderBy(c => c.CurrentLife).ThenBy(c => c.Id).First();
            Apply(state, trace, new Dictionary<ObjectId, int> { [recipient.Id] = amount });
            return false;
        }

        state.Decision = new DamageSplitDecision(attackerSide.Value, trace.Id, amount, candidates.Select(c => c.Id).ToList(), defended);
        return true;
    }

    /// <summary>D13: all of a Combat's damage is computed from the pre-combat state and dealt in
    /// one instruction, so both sides can fall. Defenders retaliate unless the attacker is Ranged.</summary>
    public static void Apply(TrueState state, TraceObject trace, IReadOnlyDictionary<ObjectId, int> split)
    {
        var attack = trace.Attack!;
        var attacker = state.Get<Permanent>(trace.ActingPermanent!.Value);
        var damage = new Dictionary<ObjectId, int>();
        foreach (var (id, amount) in split)
        {
            if (amount > 0 && state.Exists(id))
                damage[id] = damage.GetValueOrDefault(id) + amount;
        }

        var defenders = attack.Defenders
            .Select(state.Find<Permanent>)
            .Where(d => d is not null && IsEligibleDefender(state, d, attack, attacker.Id))
            .Select(d => d!)
            .ToList();
        if (defenders.Count > 0 && !state.HasKeyword(attacker, Keyword.Ranged))
        {
            foreach (var d in defenders)
                damage[attacker.Id] = damage.GetValueOrDefault(attacker.Id) + state.Attack(d);
        }

        foreach (var (id, amount) in damage)
        {
            var target = state.Get<Permanent>(id);
            target.CurrentLife -= amount;
            trace.Notes.Add($"{state.NameOf(id)} takes {amount}");
            state.Note($"{state.NameOf(id)} takes {amount} damage.", id);
        }
        state.InstructionIndex++;
        Consequences.Run(state);
    }
}
