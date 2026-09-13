using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Turns;

/// <summary>D21: mana refreshes to the sum of connected/producing terrain each Beginning
/// phase — no banking. Harmless no-op for a player with no Champion (0 production). A neutral
/// turn (D60, ActivePlayer null) is a no-op here too — a Neutral Companion's own private-pool
/// refresh (`neutral-permanents.md`) isn't implemented yet.</summary>
public sealed class RefreshManaEffect : IPhaseEffect
{
    public IEnumerable<EventIntent> Apply(TrueState state)
    {
        if (state.ActivePlayer is not PlayerId activePlayer)
            yield break;
        yield return new ManaChangeIntent(activePlayer, Query.ResolveManaProduction(activePlayer, state));
    }
}
