using Leyline.RulesCore.Events;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Turns;

/// <summary>D21 Beginning-phase refresh: the active player's `*`-flavored once-per-turn
/// actions (e.g. the Champion's Bond) become usable again. Harmless no-op for actors with
/// nothing recorded.</summary>
public sealed class ResetOncePerTurnActionsEffect : IPhaseEffect
{
    public IEnumerable<EventIntent> Apply(TrueState state)
    {
        foreach (var actor in state.ActorsOwnedBy(state.ActivePlayer))
            yield return new OncePerTurnActionsResetIntent(actor.Id);
    }
}
