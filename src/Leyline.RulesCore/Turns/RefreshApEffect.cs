using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Turns;

/// <summary>D21 Beginning-phase refresh: the active player's creatures refill to max AP.
/// A defender's AP outside their own turn carries over from their last refresh — no
/// special-casing needed, since this only touches the active player's actors.</summary>
public sealed class RefreshApEffect : IPhaseEffect
{
    public IEnumerable<EventIntent> Apply(TrueState state)
    {
        foreach (var actor in state.ActorsOwnedBy(state.ActivePlayer))
        {
            if (actor is CreatureState creature)
                yield return new ApChangeIntent(creature.Id, Query.ResolveMaxAp(creature.Id, state));
        }
    }
}
