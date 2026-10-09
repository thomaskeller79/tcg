using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore;

/// <summary>The engine's two first-class queries (PLAN.md M1): legal-command enumeration and
/// command application. Hosts and the harnesses go through here.</summary>
public static class RulesEngine
{
    public static IReadOnlyList<Command> LegalCommands(TrueState state, PlayerId actor)
    {
        if (state.IsOver)
            return [];

        if (state.Decision is { } decision)
            return decision.Decider == actor ? DecisionCommands(state, decision) : [];

        if (state.PriorityHolder != actor)
            return [];

        var commands = new List<Command> { new PassCommand(actor) };
        commands.AddRange(Activation.LegalFor(state, actor).Select(r => new ActivateCommand(actor, r.Source, r.AbilityId, r.Targets, r.Mana)));
        return commands;
    }

    public static CommandResult Apply(TrueState state, Command command)
    {
        if (state.IsOver)
            return CommandResult.Reject("The match is over.");

        if (state.Decision is { } decision)
        {
            if (decision.Decider != command.Actor)
                return CommandResult.Reject("Waiting for another Champion's decision.");
            return (decision, command) switch
            {
                (DamageSplitDecision d, SplitDamageCommand s) => ApplySplit(state, d, s),
                (RedirectDecision r, RedirectCommand c) => ApplyRedirect(state, r, c),
                (RedirectDecision r, CancelRedirectCommand) => CancelRedirect(state, r),
                _ => CommandResult.Reject("A decision is pending."),
            };
        }

        if (state.PriorityHolder != command.Actor)
            return CommandResult.Reject("You don't have priority.");

        switch (command)
        {
            case PassCommand:
                Turns.Pass(state, command.Actor);
                return CommandResult.Ok;
            case ActivateCommand a:
                return ApplyActivate(state, a);
            default:
                return CommandResult.Reject($"Can't do {command.GetType().Name} now.");
        }
    }

    private static CommandResult ApplyActivate(TrueState state, ActivateCommand cmd)
    {
        var result = Activation.Build(state, new ActivationRequest(cmd.Actor, cmd.Source, cmd.Ability, cmd.Targets, cmd.Mana));
        if (result.Error is { } error)
            return CommandResult.Reject(error);
        var draft = result.Draft!;

        if (result.RedirectIndex is { } index)
        {
            // D68: the location was illegal against true state. The attempt revealed
            // information, so the cost is committed; the player redirects or cancels.
            Activation.PayCost(state, draft);
            Consequences.Run(state);
            var failed = index < draft.Targets.Count ? draft.Targets[index][0] : null;
            state.Note($"Champion {cmd.Actor}'s destination turns out to be blocked by something hidden.");
            var remaining = failed is null ? [] : RedirectCandidates(state, draft, index).Where(c => c != failed).ToList();
            if (remaining.Count == 0)
            {
                state.Note("No other destination — the action fails; its cost stays paid.");
                Triggers.Flush(state);
                return CommandResult.Ok;
            }
            state.Decision = new RedirectDecision(cmd.Actor, draft, index, remaining);
            return CommandResult.Ok;
        }

        Activation.Commit(state, draft);
        Turns.AfterActivation(state, cmd.Actor);
        return CommandResult.Ok;
    }

    /// <summary>The other locations still legal in the actor's view for the same choice.</summary>
    private static IReadOnlyList<TargetChoice> RedirectCandidates(TrueState state, ActivationDraft draft, int index)
    {
        var result = new List<TargetChoice>();
        if (draft.IsCast)
        {
            foreach (var hex in state.Hexes)
            {
                var candidate = TargetChoice.ForLocation(hex, draft.Targets[index][0].Slice);
                if (CheckIgnoringCost(state, draft, index, candidate))
                    result.Add(candidate);
            }
            return result;
        }

        var p = state.Get<Permanent>(draft.Source);
        if (draft.Ability!.Builtin != BuiltinAbility.Move)
            return result; // Ascend/Descend have only one location (D68)
        foreach (var hex in state.Neighbors(p.Hex))
        {
            var candidate = TargetChoice.ForLocation(hex, p.Slice);
            if (CheckIgnoringCost(state, draft, index, candidate))
                result.Add(candidate);
        }
        return result;
    }

    /// <summary>Re-checks a redirected choice's legality in the actor's view. The cost is already
    /// paid, so for a permanent's ability only the target part is checked (a card has already
    /// left the hand, so casts are checked by location alone).</summary>
    private static bool CheckIgnoringCost(TrueState state, ActivationDraft draft, int index, TargetChoice choice)
    {
        if (draft.IsCast)
        {
            var def = draft.Card!;
            var hex = choice.Hex!.Value;
            var slice = choice.Slice ?? Activation.ExpectedSlice(def);
            return state.IsOnBoard(hex) && !state.IsVoid(hex)
                && Network.TerrainController(state, state.TerrainOf(hex)) == draft.Actor
                && Entry.CanPermanentEnter(state, def, draft.Actor, hex, slice, trueState: false, observer: draft.Actor);
        }
        var p = state.Get<Permanent>(draft.Source);
        var target = choice.Hex!.Value;
        if (!state.IsOnBoard(target) || target.DistanceTo(p.Hex) != 1 || state.IsVoid(target))
            return false;
        if (p.Kind.IsRoot() && Network.IsRootConnected(state, p) && state.TerrainOf(target).Parent != p.Id)
            return false;
        return Entry.CanCreatureEnter(state, state.Controller(p), target, p.Slice, p.Id, trueState: false, draft.Actor);
    }

    private static CommandResult ApplyRedirect(TrueState state, RedirectDecision decision, RedirectCommand cmd)
    {
        if (!decision.Remaining.Contains(cmd.Choice))
            return CommandResult.Reject("Not one of the remaining destinations.");

        var draft = decision.Draft;
        var hex = cmd.Choice.Hex!.Value;
        bool legalNow;
        if (draft.IsCast)
        {
            var slice = cmd.Choice.Slice ?? Activation.ExpectedSlice(draft.Card!);
            legalNow = Entry.CanPermanentEnter(state, draft.Card!, draft.Actor, hex, slice, trueState: true);
        }
        else
        {
            var p = state.Get<Permanent>(draft.Source);
            legalNow = Entry.CanCreatureEnter(state, state.Controller(p), hex, p.Slice, p.Id, trueState: true);
        }

        if (!legalNow)
        {
            var remaining = decision.Remaining.Where(c => c != cmd.Choice).ToList();
            state.Note($"{cmd.Choice} is blocked too.");
            if (remaining.Count == 0)
            {
                state.Decision = null;
                state.Note("No destination left — the action fails; its cost stays paid.");
                return CommandResult.Ok;
            }
            state.Decision = decision with { Remaining = remaining };
            return CommandResult.Ok;
        }

        draft.Targets[decision.TargetIndex] = [cmd.Choice];
        state.Decision = null;
        Activation.Commit(state, draft);
        Turns.AfterActivation(state, cmd.Actor);
        return CommandResult.Ok;
    }

    private static CommandResult CancelRedirect(TrueState state, RedirectDecision decision)
    {
        state.Decision = null;
        state.Note($"Champion {decision.Decider} cancels; the cost stays paid (D68).");
        return CommandResult.Ok;
    }

    private static CommandResult ApplySplit(TrueState state, DamageSplitDecision decision, SplitDamageCommand cmd)
    {
        if (cmd.Split.Keys.Any(k => !decision.Candidates.Contains(k)))
            return CommandResult.Reject("Damage can only go to the listed candidates.");
        if (cmd.Split.Values.Any(v => v < 0) || cmd.Split.Values.Sum() != decision.Amount)
            return CommandResult.Reject($"Split exactly {decision.Amount} damage.");

        state.Decision = null;
        var trace = state.Get<TraceObject>(decision.AttackTrace);
        Combat.Apply(state, trace, cmd.Split);
        Resolution.Finish(state, trace);
        Turns.AfterResolution(state);
        return CommandResult.Ok;
    }

    private static IReadOnlyList<Command> DecisionCommands(TrueState state, PendingDecision decision)
    {
        switch (decision)
        {
            case DamageSplitDecision d:
                return Splits(d.Amount, d.Candidates).Select(s => (Command)new SplitDamageCommand(d.Decider, s)).ToList();
            case RedirectDecision r:
                var list = r.Remaining.Select(c => (Command)new RedirectCommand(r.Decider, c)).ToList();
                list.Add(new CancelRedirectCommand(r.Decider));
                return list;
            default:
                return [];
        }
    }

    /// <summary>Every way to split <paramref name="amount"/> among the candidates.</summary>
    private static IEnumerable<IReadOnlyDictionary<ObjectId, int>> Splits(int amount, IReadOnlyList<ObjectId> candidates)
    {
        if (candidates.Count == 1)
        {
            yield return new Dictionary<ObjectId, int> { [candidates[0]] = amount };
            yield break;
        }
        for (var first = amount; first >= 0; first--)
        {
            foreach (var rest in Splits(amount - first, candidates.Skip(1).ToList()))
            {
                var d = new Dictionary<ObjectId, int>(rest) { [candidates[0]] = first };
                yield return d;
            }
        }
    }
}
