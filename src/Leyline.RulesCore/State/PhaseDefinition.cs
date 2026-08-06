using Leyline.RulesCore.Events;

namespace Leyline.RulesCore.State;

public interface IPhaseEffect
{
    IEnumerable<EventIntent> Apply(TrueState state);
}

/// <summary>
/// Phases are data, walked by index (pillar 5: a future card effect must be able to
/// add/remove a phase without the turn engine special-casing it).
/// </summary>
public sealed record PhaseDefinition(string Id, IReadOnlyList<IPhaseEffect> OnEnterEffects, bool OffersPriority);
