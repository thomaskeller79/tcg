using Leyline.RulesCore.Events;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Turns;

/// <summary>D21 Beginning-phase refresh: the active player's `*`-flavored once-per-turn
/// actions (e.g. the Champion's Bond) become usable again. Harmless no-op for actors with
/// nothing recorded. This is also what makes Defend's cap (D65) land as "once per round"
/// rather than "once per engine turn": it only ever resets on the *player's own* turn, and
/// with Neutral turns (D60) in the cycle a player's own turn now comes around once per round —
/// no special-casing needed here, CanUseOncePerTurnAction stays generic. A neutral turn
/// (ActivePlayer null) is a no-op until Neutral permanents exist.</summary>
public sealed class ResetOncePerTurnActionsEffect : IPhaseEffect
{
    public IEnumerable<EventIntent> Apply(TrueState state)
    {
        if (state.ActivePlayer is not PlayerId activePlayer)
            yield break;
        foreach (var actor in state.ActorsOwnedBy(activePlayer))
            yield return new OncePerTurnActionsResetIntent(actor.Id);
    }
}
