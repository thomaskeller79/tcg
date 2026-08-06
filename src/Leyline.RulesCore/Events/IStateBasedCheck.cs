using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Events;

/// <summary>An always-evaluated consequence of the current state (pillar 5, mutation kind
/// #7 — win/loss is one of these, never a hardcoded `if`). Run to a fixed point after every
/// batch of events, between stack resolutions.</summary>
public interface IStateBasedCheck
{
    IEnumerable<IEvent> Evaluate(TrueState state);
}
