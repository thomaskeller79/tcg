using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Turns;

/// <summary>D21 Beginning-phase refresh: the active player's actors refill to max AP —
/// uniform across Creature and Champion (both IHasCardDefinition; ResolveMaxAp already
/// resolves to 0 for anything without a CardDefinition, so this needs no type-check). A
/// defender's AP outside their own turn carries over from their last refresh — no
/// special-casing needed, since this only touches the active player's actors.</summary>
public sealed class RefreshApEffect : IPhaseEffect
{
    public IEnumerable<EventIntent> Apply(TrueState state)
    {
        foreach (var actor in state.ActorsOwnedBy(state.ActivePlayer))
            yield return new ApChangeIntent(actor.Id, Query.ResolveMaxAp(actor.Id, state));
    }
}
