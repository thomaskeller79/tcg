using Leyline.RulesCore.Combat;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Events;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Aether;

/// <summary>
/// The one shared priority/Pending mechanism (D45/D46) every response window resolves through —
/// Combat-declare and Cast-resolution alike. Combat keeps its own ActiveCombats-keyed Resolve()
/// for what happens when ITS window closes; a cast's resolution is already fully described by
/// its own Trace (IPendingResolution), consumed the moment that Trace is popped off Pending, so
/// closing a CastResolution window is a no-op here.
/// </summary>
public static class AetherPipeline
{
    public static void OpenPriorityWindow(TrueState state, PriorityWindowKind kind, object context, IReadOnlyList<PlayerId> order)
    {
        state.ActiveWindow = new PriorityWindow { Kind = kind, Context = context, Order = order };
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

        var events = new List<IEvent>();
        if (!state.Pending.IsEmpty)
            events.AddRange(ResolveTopOfPending(state, pipeline));

        if (!state.Pending.IsEmpty)
        {
            // Something is still queued (another trace, or a real response the resolution above
            // just triggered) — reopen for a fresh round rather than draining everything in one
            // pass-cycle. Nothing in M1 can actually add anything here yet (no instant-speed
            // content), but the loop is correct for when that exists.
            window.ConsecutivePasses = 0;
            window.CurrentIndex = 0;
            return CommandResult.Accept(events);
        }

        // Pending is empty — either it already was, or the resolution above just emptied it.
        // Close now rather than demanding one more all-pass round to "confirm" nothing's left:
        // with zero instant-speed content in M1, that round could never produce a different
        // command anyway (RulesEngine.LegalCommands offers only Pass while any window is open).
        state.ActiveWindow = null;
        return window.Kind switch
        {
            PriorityWindowKind.CombatDeclare => CommandResult.Accept(events.Concat(CombatPipeline.Resolve(state, pipeline, state.GetCombat((CombatId)window.Context))).ToList()),
            PriorityWindowKind.CastResolution => CommandResult.Accept(events),
            _ => throw new NotSupportedException($"Unhandled priority window kind {window.Kind}."),
        };
    }

    /// <summary>Pops the top Trace, runs its own resolution (re-validating targets per-instruction,
    /// D33), and files the result into Past with a fresh fade timer (D50).</summary>
    private static IReadOnlyList<IEvent> ResolveTopOfPending(TrueState state, EventPipeline pipeline)
    {
        var trace = state.Pending.Pop();
        var intents = trace.Resolution.Resolve(state).ToList();
        var events = pipeline.ProcessBatch(intents, state).ToList();

        state.Past.Add(new PastTrace(
            trace.Id,
            trace.Controller,
            trace.Resolution.Describe(state),
            state.RoundNumber,
            state.RoundNumber + TrueState.DefaultTraceDuration));

        return events;
    }
}
