using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Events;

/// <summary>D9: killing the enemy Champion wins. Runs alongside ZeroLifeDestructionCheck in
/// the same fixed-point pass — that check removes the Champion from the board, this one
/// marks the match over. Guarded by state.Winner so it fires exactly once.</summary>
public sealed class ChampionDeathCheck : IStateBasedCheck
{
    public IEnumerable<IEvent> Evaluate(TrueState state)
    {
        if (state.Winner is not null)
            yield break;

        foreach (var champion in state.AllActors.OfType<ChampionState>().Where(c => c.Life <= 0))
            yield return new MatchEndedEvent(champion.Owner);
    }
}
