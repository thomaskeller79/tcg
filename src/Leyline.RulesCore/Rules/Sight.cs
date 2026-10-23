using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// Observation (asymmetric-information.md §Observation, D152): each terrain observes its Slices
/// for every Champion. Ground and Sky: everyone; Root: the permanent's controller and, by D67's
/// proximity, whoever controls a permanent in Root on the same or an adjacent hex. A dark Slice
/// (Darkness) shows a permanent only to its controller and to whoever controls a permanent in that
/// Slice of that terrain. An observer is a Champion (a PlayerId) or, for a Neutral permanent's own
/// decisions, Neutral (null, D85). <see cref="Observation.Update"/> turns it into beliefs.
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
        if (state.Controller(p) == observer)
            return true;
        return state.ObservesSlice(observer, p.Hex, IsInRoot(p) ? Slice.Root : p.Slice);
    }

    public static bool CanSee(this TrueState state, PlayerId? observer, ObjectId id) =>
        state.Find<GameObject>(id) switch
        {
            Permanent p => state.CanSee(observer, p),
            TraceObject or CardObject => true,
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

    /// <summary>D155: a Champion observes a Slice where it would observe a permanent it doesn't
    /// control: Ground and Sky always, the Root by D67's proximity. No Slice: a terrain, always.</summary>
    public static bool ObservesSlice(this TrueState state, PlayerId? observer, HexCoord hex, Slice? slice)
    {
        if (slice is not { } s)
            return true;
        if (state.IsOnBoard(hex) && state.IsDark(state.TerrainOf(hex), s))
            return state.Permanents.Any(q => q.Kind != PermanentKind.Terrain && q.Carrier is null && q.Hex == hex && q.Slice == s && state.Controller(q) == observer);
        return s != Slice.Root || HasRootPresenceNear(state, observer, hex);
    }

    /// <summary>Darkness replaces the observation ability of one Slice of a terrain while its trace
    /// exists (D152, D144): a modifier on the terrain.</summary>
    public static bool IsDark(this TrueState state, Permanent terrain, Slice slice) =>
        state.Modifiers.Any(m => m.Subject == terrain.Id && m.Stat == Stats.DarkStat(slice));

    /// <summary>D159 visibility: the Champion believes the permanent is where it truly is. Neutral
    /// reads the true state (PLAN.md Track A item 61).</summary>
    public static bool KnowsWhere(this TrueState state, PlayerId? who, Permanent p)
    {
        if (p.Kind == PermanentKind.Terrain || who is not { } w)
            return true;
        if (p.Carrier is { } carrier)
            return state.Find<Permanent>(carrier) is { } c && state.KnowsWhere(who, c);
        var b = state.Beliefs.Location(w, p.Id);
        return b.Hex == p.Hex && b.Slice == p.Slice;
    }

    /// <summary>The location a Champion believes a permanent has; null for `?`. A carried Item is
    /// believed where its carrier is.</summary>
    public static (HexCoord Hex, Slice Slice)? BelievedLocation(this TrueState state, PlayerId who, Permanent p)
    {
        if (p.Kind == PermanentKind.Terrain)
            return (p.Hex, p.Slice);
        if (p.Carrier is { } carrier)
            return state.Find<Permanent>(carrier) is { } c ? state.BelievedLocation(who, c) : null;
        var b = state.Beliefs.Location(who, p.Id);
        return b.Hex is { } h ? (h, b.Slice) : null;
    }
}
