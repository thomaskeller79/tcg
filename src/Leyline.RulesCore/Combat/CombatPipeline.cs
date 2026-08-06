using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Combat;

/// <summary>
/// The D3/D4/D13/D19 combat sequence: DeclareCombat → DeclareDefenders → (damage-assignment
/// decision point) → the one Combat-declare priority window (locked M1 scope) → Resolve.
/// This is the "first slice" the whole M1 sandbox exists to exercise.
/// </summary>
public static class CombatPipeline
{
    public static CommandResult DeclareCombat(TrueState state, EventPipeline pipeline, DeclareCombatCommand cmd)
    {
        var attacker = state.GetActor(cmd.Attacker);
        if (attacker.Owner != cmd.Actor)
            return CommandResult.Reject("Not your creature.");
        if (!Query.ResolveLegalAttackTargets(cmd.Attacker, state).Contains(cmd.TargetHex))
            return CommandResult.Reject("Illegal attack target.");

        var cost = Query.ResolveAttackCost(cmd.Attacker, state);
        var combatId = state.AllocateCombatId();

        var events = new List<IEvent>();
        events.AddRange(pipeline.Process(new ApChangeIntent(cmd.Attacker, cost.Apply(attacker.CurrentAp)), state));
        events.AddRange(pipeline.Process(new CombatDeclaredIntent(combatId, cmd.Attacker, cmd.TargetHex), state));

        // D19 (provisional): declaring an attack from Below surfaces the attacker — it stays
        // located on this hex until it moves away (RulesEngine.ApplyMove re-conceals it).
        if (attacker.Layer == Layer.Below)
            events.AddRange(pipeline.Process(new ActorRevealedIntent(cmd.Attacker), state));

        return CommandResult.Accept(events);
    }

    public static CommandResult DeclareDefenders(TrueState state, EventPipeline pipeline, DeclareDefendersCommand cmd)
    {
        var combat = state.GetCombat(cmd.Combat);
        if (combat.Phase != CombatPhase.AwaitingDefenders)
            return CommandResult.Reject("Combat is not awaiting defenders.");

        var attackerOwner = state.GetActor(combat.Attacker).Owner;
        if (cmd.Actor == attackerOwner)
            return CommandResult.Reject("The attacker does not declare defenders.");
        if (cmd.Defenders.Distinct().Count() != cmd.Defenders.Count)
            return CommandResult.Reject("Duplicate defender.");

        // No visibility gate here: a player always knows about their own creatures, hidden or
        // not — concealment (D19) limits what the OPPONENT can target, not the owner's choices.
        var hexOccupants = state.Board.GetCell(combat.TargetHex).GroundAndBelowOccupants.ToList();
        foreach (var defenderId in cmd.Defenders)
        {
            var defender = state.GetActor(defenderId);
            if (defender.Owner != cmd.Actor || !hexOccupants.Contains(defenderId))
                return CommandResult.Reject($"{defenderId} cannot defend this hex.");
            if (!Query.CanDefend(defenderId, state))
                return CommandResult.Reject($"{defenderId} cannot defend right now.");
        }

        var events = new List<IEvent>();
        foreach (var defenderId in cmd.Defenders)
        {
            var defender = state.GetActor(defenderId);
            var cost = Query.ResolveDefendCost(defenderId, state);
            events.AddRange(pipeline.Process(new ApChangeIntent(defenderId, cost.Apply(defender.CurrentAp)), state));
        }
        events.AddRange(pipeline.Process(new DefendersDeclaredIntent(cmd.Combat, cmd.Defenders), state));

        if (cmd.Defenders.Count == 1)
        {
            var attack = Query.ResolveAttack(combat.Attacker, state);
            var assignment = new Dictionary<ActorId, int> { [cmd.Defenders[0]] = attack };
            events.AddRange(pipeline.Process(new DamageAssignedIntent(cmd.Combat, assignment), state));
            OpenPriorityWindow(state, combat, attackerOwner);
        }
        // 0 defenders: attacker still needs to choose an undefended target.
        // >1 defenders: attacker still needs to submit a damage split.

        return CommandResult.Accept(events);
    }

    public static CommandResult AssignDamage(TrueState state, EventPipeline pipeline, AssignDamageCommand cmd)
    {
        var combat = state.GetCombat(cmd.Combat);
        if (combat.Phase != CombatPhase.AwaitingAssignment)
            return CommandResult.Reject("Combat is not awaiting a damage assignment.");

        var attackerOwner = state.GetActor(combat.Attacker).Owner;
        if (cmd.Actor != attackerOwner)
            return CommandResult.Reject("Only the attacker assigns damage.");

        var attack = Query.ResolveAttack(combat.Attacker, state);
        if (!cmd.Assignment.Keys.OrderBy(k => k).SequenceEqual(combat.Defenders.OrderBy(k => k)))
            return CommandResult.Reject("Assignment must cover exactly the declared defenders.");
        if (cmd.Assignment.Values.Any(v => v < 0) || cmd.Assignment.Values.Sum() != attack)
            return CommandResult.Reject($"Assignment must be non-negative and sum to {attack}.");

        var events = pipeline.Process(new DamageAssignedIntent(cmd.Combat, cmd.Assignment), state).ToList();
        OpenPriorityWindow(state, combat, attackerOwner);
        return CommandResult.Accept(events);
    }

    public static CommandResult ChooseUndefendedTarget(TrueState state, EventPipeline pipeline, ChooseUndefendedTargetCommand cmd)
    {
        var combat = state.GetCombat(cmd.Combat);
        if (combat.Phase != CombatPhase.AwaitingUndefendedChoice)
            return CommandResult.Reject("Combat is not awaiting an undefended-target choice.");

        var attackerOwner = state.GetActor(combat.Attacker).Owner;
        if (cmd.Actor != attackerOwner)
            return CommandResult.Reject("Only the attacker chooses the undefended target.");

        var hexOccupants = state.Board.GetCell(combat.TargetHex).GroundAndBelowOccupants.ToList();
        if (!hexOccupants.Contains(cmd.Target))
            return CommandResult.Reject("Target must occupy the attacked hex.");
        if (!Query.IsVisibleTo(cmd.Target, attackerOwner, state))
            return CommandResult.Reject("You cannot target a concealed unit you cannot see.");

        var events = pipeline.Process(new UndefendedTargetChosenIntent(cmd.Combat, cmd.Target), state).ToList();
        OpenPriorityWindow(state, combat, attackerOwner);
        return CommandResult.Accept(events);
    }

    public static CommandResult Pass(TrueState state, EventPipeline pipeline, PassPriorityCommand cmd)
    {
        var window = state.ActiveWindow;
        if (window is null)
            return CommandResult.Reject("No active priority window.");
        if (window.CurrentPriority != cmd.Actor)
            return CommandResult.Reject("Not your priority.");

        window.ConsecutivePasses++;
        window.CurrentIndex = (window.CurrentIndex + 1) % window.Order.Count;

        if (window.ConsecutivePasses < window.Order.Count)
            return CommandResult.Accept([]);

        if (!state.Stack.IsEmpty)
        {
            // Everyone passed with something on the stack: resolve top-of-stack, then
            // priority reopens from the top of the order. M1 ships no stack content (no
            // instant-speed abilities exist yet), so this path is real plumbing that stays
            // unexercised until such content exists.
            state.Stack.Pop();
            window.ConsecutivePasses = 0;
            window.CurrentIndex = 0;
            return CommandResult.Accept([]);
        }

        var combat = state.GetCombat(window.Context);
        state.ActiveWindow = null;
        return CommandResult.Accept(Resolve(state, pipeline, combat));
    }

    private static void OpenPriorityWindow(TrueState state, CombatState combat, PlayerId attackerOwner)
    {
        var defenderOwner = state.Players.Select(p => p.Id).First(id => id != attackerOwner);
        state.ActiveWindow = new PriorityWindow
        {
            Kind = PriorityWindowKind.CombatDeclare,
            Context = combat.Id,
            Order = [defenderOwner, attackerOwner],
        };
    }

    private static IReadOnlyList<IEvent> Resolve(TrueState state, EventPipeline pipeline, CombatState combat)
    {
        var intents = new List<EventIntent>();

        if (combat.Defenders.Count == 0)
        {
            var target = combat.UndefendedTarget!.Value;
            intents.Add(new DamageIntent(combat.Attacker, target, Query.ResolveAttack(combat.Attacker, state)));
        }
        else
        {
            foreach (var (defenderId, amount) in combat.DamageAssignment!)
                intents.Add(new DamageIntent(combat.Attacker, defenderId, amount));

            // Universal retaliation (D19): every declared defender damages the attacker.
            // Ranged's "no retaliation" exception doesn't exist as a keyword yet.
            foreach (var defenderId in combat.Defenders)
                intents.Add(new DamageIntent(defenderId, combat.Attacker, Query.ResolveAttack(defenderId, state)));
        }

        intents.Add(new CombatResolvedIntent(combat.Id));

        // D13: every damage intent in this combat is computed from pre-combat state and
        // applied together — one ProcessBatch call, not one Process per intent (which would
        // run the state-based-check checkpoint between each and could prevent "both die").
        return pipeline.ProcessBatch(intents, state);
    }

    public static IReadOnlyList<DeclareDefendersCommand> LegalDefenderDeclarations(TrueState state, CombatState combat, PlayerId defendingPlayer)
    {
        var eligible = state.Board.GetCell(combat.TargetHex).GroundAndBelowOccupants
            .Where(id => state.GetActor(id).Owner == defendingPlayer && Query.CanDefend(id, state))
            .OrderBy(id => id)
            .ToList();

        var results = new List<DeclareDefendersCommand>();
        for (var mask = 0; mask < (1 << eligible.Count); mask++)
        {
            var subset = new List<ActorId>();
            for (var i = 0; i < eligible.Count; i++)
                if ((mask & (1 << i)) != 0)
                    subset.Add(eligible[i]);
            results.Add(new DeclareDefendersCommand(defendingPlayer, combat.Id, subset));
        }
        return results;
    }

    public static IReadOnlyList<AssignDamageCommand> LegalDamageAssignments(TrueState state, CombatState combat, PlayerId attackingPlayer)
    {
        var attack = Query.ResolveAttack(combat.Attacker, state);
        return EnumerateAssignments(attack, combat.Defenders)
            .Select(assignment => new AssignDamageCommand(attackingPlayer, combat.Id, assignment))
            .ToList();
    }

    public static IReadOnlyList<ChooseUndefendedTargetCommand> LegalUndefendedChoices(TrueState state, CombatState combat, PlayerId attackingPlayer) =>
        state.Board.GetCell(combat.TargetHex).GroundAndBelowOccupants
            .Where(id => Query.IsVisibleTo(id, attackingPlayer, state))
            .OrderBy(id => id)
            .Select(id => new ChooseUndefendedTargetCommand(attackingPlayer, combat.Id, id))
            .ToList();

    private static IEnumerable<Dictionary<ActorId, int>> EnumerateAssignments(int total, IReadOnlyList<ActorId> defenders)
    {
        if (defenders.Count == 1)
        {
            yield return new Dictionary<ActorId, int> { [defenders[0]] = total };
            yield break;
        }

        for (var toFirst = 0; toFirst <= total; toFirst++)
        {
            foreach (var rest in EnumerateAssignments(total - toFirst, defenders.Skip(1).ToList()))
            {
                rest[defenders[0]] = toFirst;
                yield return rest;
            }
        }
    }
}
