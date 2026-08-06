using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Queries;

/// <summary>
/// Two reusable, hand-authored modifier kinds covering the "add" and "set" shapes of a
/// continuous int effect (e.g. "+1 Attack until end of turn" / "Attack becomes 0") — a small
/// fixed catalog rather than a bespoke type per card. Subject == null applies to every subject
/// of the given QueryKind (unused today; keeps aura-style effects free later). Sealed records:
/// value equality, no closures, built only from primitives/ids — see IQueryModifier's purity
/// requirement.
/// </summary>
public sealed record IntDeltaModifier(
    ModifierId Id, string QueryKind, ActorId? Subject, int Delta, ModifierDuration Duration)
    : IQueryModifier<int>
{
    public bool AppliesTo(QueryContext ctx, TrueState state) => Subject is null || ctx.Subject == Subject;
    public int Resolve(QueryContext ctx, int current, TrueState state) => current + Delta;
}

public sealed record IntSetModifier(
    ModifierId Id, string QueryKind, ActorId? Subject, int Value, ModifierDuration Duration)
    : IQueryModifier<int>
{
    public bool AppliesTo(QueryContext ctx, TrueState state) => Subject is null || ctx.Subject == Subject;
    public int Resolve(QueryContext ctx, int current, TrueState state) => Value;
}
