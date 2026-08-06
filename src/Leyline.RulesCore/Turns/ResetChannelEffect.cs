using Leyline.RulesCore.Events;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Turns;

/// <summary>
/// D21 Beginning-phase refresh: the active player's Channel becomes available again, and
/// (unlike a Creature, which refills to max via RefreshApEffect) the Champion's AP resets to
/// zero — it only has spendable AP on a turn it Channel-acts (D9).
/// </summary>
public sealed class ResetChannelEffect : IPhaseEffect
{
    public IEnumerable<EventIntent> Apply(TrueState state)
    {
        var champion = state.ActorsOwnedBy(state.ActivePlayer).OfType<ChampionState>().FirstOrDefault();
        if (champion is null)
            yield break;

        yield return new ChannelResetIntent(champion.Id);
        yield return new ApChangeIntent(champion.Id, 0);
    }
}
