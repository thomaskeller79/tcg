using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// Perception rules (asymmetric-information.md): the board is visible by default; the Root Slice
/// is hidden (D12) except to the object's controller and, by D67's proximity exception, to an
/// observer who controls a permanent in Root on the same or an adjacent hex. An observer is a
/// Champion (a PlayerId) or, for a Neutral permanent's own decisions, Neutral (null, D85).
/// </summary>
public static class Sight
{
    /// <summary>Is this permanent in hidden space — a creature-type, Remnant or loose Item in
    /// the Root Slice, or a Structure in the Root slot (D12, D40, D110)?</summary>
    public static bool IsInRoot(Permanent p) =>
        p.Kind != PermanentKind.Terrain && p.Carrier is null && p.Slice == Slice.Root;

    public static bool CanSee(this TrueState state, PlayerId? observer, Permanent p)
    {
        if (p.Kind == PermanentKind.Terrain)
            return true;
        if (p.Carrier is { } carrier)
            return state.Find<Permanent>(carrier) is { } c && state.CanSee(observer, c);
        if (!IsInRoot(p))
            return true;
        if (state.Controller(p) == observer)
            return true;
        return HasRootPresenceNear(state, observer, p.Hex);
    }

    public static bool CanSee(this TrueState state, PlayerId? observer, ObjectId id) =>
        state.Find<GameObject>(id) switch
        {
            Permanent p => state.CanSee(observer, p),
            TraceObject t => state.CanSeeTrace(observer, t),
            CardObject => true,
            _ => false,
        };

    /// <summary>D67: matching-Slice presence of any permanent the observer controls, on the hex
    /// or a neighbor — a live query, lost again when the presence leaves.</summary>
    public static bool HasRootPresenceNear(TrueState state, PlayerId? observer, HexCoord hex) =>
        state.Permanents.Any(q =>
            IsInRoot(q)
            && q.Kind != PermanentKind.Item
            && q.Kind != PermanentKind.Remnant
            && q.Hex.DistanceTo(hex) <= 1
            && state.Controller(q) == observer);

    /// <summary>G17: a trace is visible when its acting permanent is (an Ability trace of a
    /// hidden creature would otherwise reveal it); cast traces are always visible.</summary>
    public static bool CanSeeTrace(this TrueState state, PlayerId? observer, TraceObject t) =>
        t.ActingPermanent is not { } acting
        || state.Find<Permanent>(acting) is not { } p
        || state.CanSee(observer, p);
}
