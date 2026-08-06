using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Turns;

/// <summary>D21: mana refreshes to the sum of connected/producing terrain each Beginning
/// phase — no banking. Harmless no-op for a player with no Champion (0 production).</summary>
public sealed class RefreshManaEffect : IPhaseEffect
{
    public IEnumerable<EventIntent> Apply(TrueState state)
    {
        yield return new ManaChangeIntent(state.ActivePlayer, Query.ResolveManaProduction(state.ActivePlayer, state));
    }
}
