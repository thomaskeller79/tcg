using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>Spatial queries over the Island (overview.md §2): positions, adjacency, occupants.
/// Occupants are derived from the occupants' own locations, never stored on the hex (D101).</summary>
public static class Island
{
    /// <summary>D119/overview §2: a Slice holds at most 3 creature-type permanents.</summary>
    public const int SliceCapacity = 3;

    public static CardDefinition Def(this TrueState state, Permanent p) => state.Content.Get(p.Definition);
    public static CardDefinition Def(this TrueState state, CardObject c) => state.Content.Get(c.Definition);

    public static string NameOf(this TrueState state, ObjectId id) => state.Find<GameObject>(id) switch
    {
        Permanent p when p.Kind == PermanentKind.Remnant => $"Remnant of {state.Def(p).Name} {p.Id}",
        Permanent p => $"{state.Def(p).Name} {p.Id}",
        CardObject c => $"{state.Def(c).Name} (card {c.Id})",
        TraceObject t => $"trace {t.Id}",
        _ => $"{id} (gone)",
    };

    /// <summary>D105: follow `location` until it reaches a terrain.</summary>
    public static HexCoord PositionOf(this TrueState state, Permanent p) =>
        p.Carrier is { } carrier ? state.PositionOf(state.Get<Permanent>(carrier)) : p.Hex;

    public static bool IsOnBoard(this TrueState state, HexCoord hex) => state.TerrainAt.ContainsKey(hex);

    public static bool IsVoid(this TrueState state, HexCoord hex) =>
        !state.TerrainAt.TryGetValue(hex, out var id) || state.Content.Get(state.Get<Permanent>(id).Definition).IsVoid;

    public static IReadOnlyList<HexCoord> Neighbors(this TrueState state, HexCoord hex) =>
        hex.Neighbors().Where(state.IsOnBoard).OrderBy(h => h).ToList();

    public static IReadOnlyList<HexCoord> WithinDistance(this TrueState state, HexCoord hex, int distance) =>
        state.Hexes.Where(h => h.DistanceTo(hex) <= distance).ToList();

    /// <summary>Creature-type permanents in one Slice of one hex (D119: these are the occupants
    /// the entry rule looks at).</summary>
    public static IReadOnlyList<Permanent> CreaturesIn(this TrueState state, HexCoord hex, Slice slice) =>
        state.Permanents.Where(p => p.Kind.IsCreatureType() && p.Carrier is null && p.Hex == hex && p.Slice == slice).ToList();

    /// <summary>D24/D40: a hex has a Ground slot (Ground + Sky) and a Root slot.</summary>
    public static Permanent? StructureIn(this TrueState state, HexCoord hex, Slice slot) =>
        state.Permanents.FirstOrDefault(p => p.Kind == PermanentKind.Structure && p.Hex == hex && p.Slice == SlotOf(slot));

    public static Slice SlotOf(Slice slice) => slice == Slice.Root ? Slice.Root : Slice.Ground;

    /// <summary>Loose Items and Remnants on a hex (capacity-exempt, D34, D110).</summary>
    public static IReadOnlyList<Permanent> LooseAt(this TrueState state, HexCoord hex) =>
        state.Permanents.Where(p => (p.Kind == PermanentKind.Remnant || (p.Kind == PermanentKind.Item && p.Carrier is null)) && p.Hex == hex).ToList();

    public static IReadOnlyList<Permanent> CarriedBy(this TrueState state, ObjectId carrier) =>
        state.Permanents.Where(p => p.Kind == PermanentKind.Item && p.Carrier == carrier).ToList();

    /// <summary>D105: a dropped Item or a Remnant lands in Root if the source was in Root,
    /// otherwise in Ground — never in Sky.</summary>
    public static Slice LooseSliceFor(Slice from) => from == Slice.Root ? Slice.Root : Slice.Ground;

    /// <summary>Everything that stands on a hex and could knot it (D121): permanents positioned
    /// there in any Slice, carried Items excluded (they are on their carrier).</summary>
    public static IEnumerable<Permanent> StandingOn(this TrueState state, HexCoord hex) =>
        state.Permanents.Where(p => p.Kind != PermanentKind.Terrain && p.Carrier is null && p.Hex == hex);
}
