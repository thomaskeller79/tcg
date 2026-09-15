using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Turns;

/// <summary>D21: mana refreshes to the sum of connected/producing terrain each Beginning
/// phase — no banking. Harmless no-op for a player with no Champion (0 production). A neutral
/// turn (D60, ActivePlayer null) is a no-op here too — a Neutral Companion's own private-pool
/// refresh (`neutral-permanents.md`) isn't implemented yet.
/// Not the only trigger: TerrainPipeline.Bond does this same recompute immediately when a Bond
/// resolves (D57 — Terrain is placed, not cast, so it's never summoning-sick; an already-
/// producing tile counts the instant it's bonded, not at the next Beginning phase). This effect
/// still matters for anything that changes production *without* a fresh bond (e.g. a blockade
/// clearing on its own).</summary>
public sealed class RefreshManaEffect : IPhaseEffect
{
    public IEnumerable<EventIntent> Apply(TrueState state)
    {
        if (state.ActivePlayer is not PlayerId activePlayer)
            yield break;
        yield return new ManaChangeIntent(activePlayer, Query.ResolveManaProduction(activePlayer, state));
    }
}
