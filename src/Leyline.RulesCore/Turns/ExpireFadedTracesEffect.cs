using Leyline.RulesCore.Events;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Turns;

/// <summary>
/// D50 End-phase cleanup: any Past trace whose fade window has elapsed leaves the Aether
/// entirely. Global rather than active-player-scoped (Past isn't a per-player zone) — safe to
/// run on every End phase, including a neutral turn's (D60), since removing an already-expired
/// trace twice is a no-op.
/// </summary>
public sealed class ExpireFadedTracesEffect : IPhaseEffect
{
    public IEnumerable<EventIntent> Apply(TrueState state) =>
        state.Past
            .Where(t => state.RoundNumber >= t.FadesAtRound)
            .Select(t => (EventIntent)new TraceFadedIntent(t.Id))
            .ToList();
}
