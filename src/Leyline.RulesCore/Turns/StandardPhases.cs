using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Turns;

/// <summary>
/// M1's fixed 3-phase turn (D21). This is still ordinary data, not a hardcoded switch —
/// Slice 3 appends RefreshManaEffect/ResetChannelEffect to Beginning without touching
/// TurnEngine, which is the concrete proof that "phases are data" (pillar 5) actually holds.
/// </summary>
public static class StandardPhases
{
    public static IReadOnlyList<PhaseDefinition> Sequence =>
    [
        new PhaseDefinition("Beginning", [new RefreshApEffect(), new ResetChannelEffect(), new RefreshManaEffect()], OffersPriority: false),
        new PhaseDefinition("Action", [], OffersPriority: true),
        new PhaseDefinition("End", [new ExpireModifiersEffect()], OffersPriority: false),
    ];
}
