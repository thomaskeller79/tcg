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

        Activation.Commit(state, draft);
        Turns.AfterActivation(state, cmd.Actor);
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
