using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Events;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Turns;

public static class TurnEngine
{
    /// <summary>Runs the very first Beginning phase's effects and auto-advances to Action.</summary>
    public static IReadOnlyList<IEvent> BeginMatch(TrueState state, EventPipeline pipeline) =>
        RunPhaseEnter(state, pipeline);

    public static CommandResult EndPhase(TrueState state, EventPipeline pipeline, EndPhaseCommand cmd)
    {
        if (cmd.Actor != state.ActivePlayer)
            return CommandResult.Reject("Not your turn.");
        if (!state.CurrentPhase.OffersPriority)
            return CommandResult.Reject("This phase advances automatically.");

        return CommandResult.Accept(Advance(state, pipeline));
    }

    private static IReadOnlyList<IEvent> Advance(TrueState state, EventPipeline pipeline)
    {
        var events = new List<IEvent>();
        var nextIndex = state.CurrentPhaseIndex + 1;

        if (nextIndex >= state.PhaseSequence.Count)
        {
            var nextPlayer = state.Players.Select(p => p.Id).First(id => id != state.ActivePlayer);
            events.AddRange(pipeline.Process(new TurnAdvancedIntent(state.TurnNumber + 1, nextPlayer), state));
            nextIndex = 0;
        }

        events.AddRange(pipeline.Process(new PhaseChangedIntent(nextIndex), state));
        events.AddRange(RunPhaseEnter(state, pipeline));
        return events;
    }

    private static IReadOnlyList<IEvent> RunPhaseEnter(TrueState state, EventPipeline pipeline)
    {
        var events = new List<IEvent>();
        foreach (var effect in state.CurrentPhase.OnEnterEffects)
        {
            foreach (var intent in effect.Apply(state))
                events.AddRange(pipeline.Process(intent, state));
        }

        // Phases nobody acts in (Beginning/End in M1) advance themselves immediately.
        if (!state.CurrentPhase.OffersPriority)
            events.AddRange(Advance(state, pipeline));

        return events;
    }
}
