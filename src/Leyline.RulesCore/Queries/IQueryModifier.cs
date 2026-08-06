using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Queries;

/// <summary>How long a modifier stays in TrueState.ActiveModifiers without an explicit
/// external removal. UntilEndOfTurn is cleared by ExpireModifiersEffect (the End phase);
/// Permanent is removed only by an explicit RemoveModifierIntent from elsewhere in the engine.</summary>
public enum ModifierDuration
{
    Permanent,
    UntilEndOfTurn,
}

/// <summary>Non-generic identity/lifecycle contract every ActiveModifiers entry satisfies,
/// regardless of which TResult it folds over — what AddModifierEvent/RemoveModifierEvent and
/// ExpireModifiersEffect operate against without knowing about IQueryModifier&lt;TResult&gt;
/// or any concrete modifier type.</summary>
public interface IModifier
{
    ModifierId Id { get; }
    ModifierDuration Duration { get; }
}

/// <summary>
/// Pillar 5's "never hardcode a rule, always ask" mechanism: a chain-of-responsibility fold
/// over a query's baseline answer. Registered into TrueState.ActiveModifiers (a heterogeneous
/// list — Query.Fold filters it by the TResult this handler cares about). Nothing implements
/// this in M1 — no card bends a rule yet — but every base rule is written as the baseline
/// inside the same fold, so adding real content later doesn't require touching the call sites.
/// Modifiers apply in append (insertion) order — see Query.Fold; there is deliberately no
/// Priority field (dropped: with only one thing ever resolving at a time under the
/// priority/stack system, insertion order already is timestamp order).
/// Implementations must be pure functions of (ctx, current, state) — never capture ambient
/// mutable state — so that replaying the same construction call always yields an
/// equal instance (determinism, see TrueState's doc comment).
/// </summary>
public interface IQueryModifier<TResult> : IModifier
{
    string QueryKind { get; }
    bool AppliesTo(QueryContext ctx, TrueState state);
    TResult Resolve(QueryContext ctx, TResult current, TrueState state);
}
