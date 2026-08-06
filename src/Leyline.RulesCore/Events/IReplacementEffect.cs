using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Events;

/// <summary>"Instead of" effects (pillar 5, mutation kind #4): run every intent past active
/// replacement effects before it becomes a concrete event. Nothing registers one in M1 —
/// no card content exists yet — but the fold happens on every intent regardless.</summary>
public interface IReplacementEffect
{
    int Priority { get; }
    bool AppliesTo(EventIntent intent, TrueState state);
    EventIntent Replace(EventIntent intent, TrueState state);
}
