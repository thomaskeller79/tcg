using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Turns;

/// <summary>
/// Generic End-phase cleanup (pillar 5, "phases are data") — knows nothing about any concrete
/// modifier type, only the IModifier/ModifierDuration contract. Removing everything whose
/// Duration is UntilEndOfTurn is correct without a per-modifier expiry turn number: M1's phase
/// sequence has exactly one End phase per turn, so "until end of turn" already means "until the
/// next End phase reached from here," regardless of whose turn it is.
/// </summary>
public sealed class ExpireModifiersEffect : IPhaseEffect
{
    public IEnumerable<EventIntent> Apply(TrueState state) =>
        state.ActiveModifiers
            .Where(m => m.Duration == ModifierDuration.UntilEndOfTurn)
            .Select(m => (EventIntent)new RemoveModifierIntent(m.Id))
            .ToList();
}
