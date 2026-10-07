using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>D45 (interaction-stack.md §The primitive): whether a card or ability of a given
/// Speed may be played right now, checked live against Pending.</summary>
public static class Speeds
{
    public static bool AnyInstantPending(TrueState state) =>
        state.Pending.Any(id => state.Get<TraceObject>(id).IsInstant);

    /// <summary><paramref name="actorSeat"/> is the seat the decider plays in: a Champion's own
    /// seat, or a Neutral permanent's neutral seat.</summary>
    public static bool Allows(TrueState state, Seat actorSeat, Speed speed)
    {
        if (AnyInstantPending(state) || state.Resolving is not null)
            return false;

        return speed switch
        {
            // Controller's main phase, Pending empty.
            Speed.Slow => state.ActiveSeat == actorSeat && state.Phase == Phase.Action && state.Pending.Count == 0,
            // G3: no non-physical trace in Pending (so also with Pending empty).
            Speed.Quick => state.Pending.All(id => state.Get<TraceObject>(id).Physical),
            // No Instant in Pending (checked above).
            Speed.Reactive or Speed.Instant => true,
            _ => false,
        };
    }
}
