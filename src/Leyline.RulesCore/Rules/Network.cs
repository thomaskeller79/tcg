using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// The terrain network (D8, D22, D77, D121, D122; resources-terrain.md §Severing behavior).
/// A root is a Champion or Companion. A bonded terrain's path runs through terrain bonded by the
/// same root back to the root's current tile, which must itself be bonded by that root.
/// <list type="bullet">
/// <item><b>Connected</b> (the cut check): such a path exists, counting knotted terrain.</item>
/// <item><b>Flowing</b>: such a path exists through unknotted terrain only — the terrain produces
/// mana and its Bond edge is active.</item>
/// <item><b>Paused</b>: connected but not flowing.</item>
/// </list>
/// Everything here is a live query; only the bond record (the terrain's Parent) and the
/// drawn-this-cycle flag are stored.
/// </summary>
public static class Network
{
    public static IReadOnlyList<Permanent> Roots(TrueState state) =>
        state.Permanents.Where(p => p.Kind.IsRoot()).ToList();

    public static Permanent? BonderOf(TrueState state, Permanent terrain) =>
        terrain.Parent is { } id ? state.Find<Permanent>(id) : null;

    /// <summary>The side a root's network belongs to: its controller (a Neutral Companion → null).</summary>
    public static PlayerId? SideOf(TrueState state, Permanent root) => state.Controller(root);

    /// <summary>D121: a terrain is knotted for a side while an enemy permanent with Knotting
    /// stands on it, in any Slice. Neutral is an enemy to both sides.</summary>
    public static bool IsKnottedFor(TrueState state, HexCoord hex, PlayerId? side) =>
        KnottersOn(state, hex).Any(k => state.Controller(k, ignoreEdgeActivity: true) != side);

    private static IEnumerable<Permanent> KnottersOn(TrueState state, HexCoord hex) =>
        state.StandingOn(hex).Where(p => state.HasKeyword(p, Keyword.Knotting));

    public static IEnumerable<Permanent> BondedBy(TrueState state, ObjectId root) =>
        state.Permanents.Where(p => p.Kind == PermanentKind.Terrain && p.Parent == root);

    /// <summary>D122 realm lock and cost doubling: the root's own tile is bonded by itself.</summary>
    public static bool IsRootConnected(TrueState state, Permanent root) =>
        state.TerrainAt.TryGetValue(state.PositionOf(root), out var tile) && state.Get<Permanent>(tile).Parent == root.Id;

    /// <summary>Hexes connected to the root through its own bonded terrain, knots included.</summary>
    public static HashSet<HexCoord> Connected(TrueState state, Permanent root) => Walk(state, root, throughKnots: true);

    /// <summary>Hexes with an unknotted path to the root — producing, Bond edge active.</summary>
    public static HashSet<HexCoord> Flowing(TrueState state, Permanent root) => Walk(state, root, throughKnots: false);

    private static HashSet<HexCoord> Walk(TrueState state, Permanent root, bool throughKnots)
    {
        var result = new HashSet<HexCoord>();
        if (!IsRootConnected(state, root))
            return result;

        var bonded = BondedBy(state, root.Id).Select(t => t.Hex).ToHashSet();
        HashSet<HexCoord>? knotted = null;
        if (!throughKnots)
        {
            var side = SideOf(state, root);
            knotted = bonded.Where(h => IsKnottedFor(state, h, side)).ToHashSet();
        }

        var start = state.PositionOf(root);
        if (knotted is not null && knotted.Contains(start))
            return result;

        var frontier = new Queue<HexCoord>();
        frontier.Enqueue(start);
        result.Add(start);
        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            foreach (var next in state.Neighbors(current))
            {
                if (result.Contains(next) || !bonded.Contains(next))
                    continue;
                if (knotted is not null && knotted.Contains(next))
                    continue;
                result.Add(next);
                frontier.Enqueue(next);
            }
        }
        return result;
    }

    public static bool IsFlowing(TrueState state, Permanent terrain) =>
        BonderOf(state, terrain) is { } root && Flowing(state, root).Contains(terrain.Hex);

    public static bool IsPaused(TrueState state, Permanent terrain) =>
        BonderOf(state, terrain) is { } root
        && Connected(state, root).Contains(terrain.Hex)
        && !Flowing(state, root).Contains(terrain.Hex);

    /// <summary>D122 cut: a bonded terrain with no path at all, even counting knots, loses its
    /// bond record immediately. Also drops records pointing at a root that no longer exists.</summary>
    public static bool ApplyCuts(TrueState state)
    {
        var changed = false;
        var connectedByRoot = new Dictionary<ObjectId, HashSet<HexCoord>>();
        foreach (var terrain in state.Permanents.Where(p => p.Kind == PermanentKind.Terrain && p.Parent is not null))
        {
            var rootId = terrain.Parent!.Value;
            if (state.Find<Permanent>(rootId) is not { } root)
            {
                terrain.Parent = null;
                changed = true;
                continue;
            }
            if (!connectedByRoot.TryGetValue(rootId, out var connected))
                connectedByRoot[rootId] = connected = Connected(state, root);
            if (!connected.Contains(terrain.Hex))
            {
                terrain.Parent = null;
                changed = true;
                state.Note($"{state.NameOf(terrain.Id)} is cut from {state.NameOf(rootId)}'s network and unbonds.", terrain.Id);
            }
        }
        return changed;
    }

    /// <summary>D77: the instant a bonded, flowing producer hasn't been drawn this cycle, its
    /// production credits straight to its bonder's pool and its flag flips.</summary>
    public static bool CreditMana(TrueState state)
    {
        var changed = false;
        foreach (var root in Roots(state))
        {
            if (root.Pool is null)
                continue;
            var flowing = Flowing(state, root);
            foreach (var terrain in BondedBy(state, root.Id).Where(t => !t.Drawn && flowing.Contains(t.Hex)))
            {
                foreach (var unit in state.Def(terrain).Produces)
                    root.Pool.Add(unit, 1);
                terrain.Drawn = true;
                changed = true;
            }
        }
        return changed;
    }

    /// <summary>The terrain's controller (D54): its bonder's controller while flowing, else Neutral.</summary>
    public static PlayerId? TerrainController(TrueState state, Permanent terrain) => state.Controller(terrain);

    /// <summary>G11: what a root may Bond — its own tile while that isn't bonded by itself;
    /// otherwise any terrain adjacent to its flowing network. The target must be unbonded, or an
    /// enemy root's terrain currently paused by a knot (theft, D122).</summary>
    public static IReadOnlyList<HexCoord> BondCandidates(TrueState state, Permanent root)
    {
        var tile = state.PositionOf(root);
        if (!IsRootConnected(state, root))
            return IsBondable(state, root, tile) ? [tile] : [];

        var flowing = Flowing(state, root);
        return flowing
            .SelectMany(h => state.Neighbors(h))
            .Where(h => !flowing.Contains(h))
            .Distinct()
            .Where(h => IsBondable(state, root, h))
            .OrderBy(h => h)
            .ToList();
    }

    private static bool IsBondable(TrueState state, Permanent root, HexCoord hex)
    {
        if (state.IsVoid(hex))
            return false;
        var terrain = state.TerrainOf(hex);
        if (terrain.Parent is null)
            return true;
        if (BonderOf(state, terrain) is not { } other || other.Id == root.Id)
            return false;
        return SideOf(state, other) != SideOf(state, root) && IsPaused(state, terrain);
    }

    public static void Bond(TrueState state, Permanent root, HexCoord hex)
    {
        var terrain = state.TerrainOf(hex);
        if (terrain.Parent is { } previous && previous != root.Id)
            state.Note($"{state.NameOf(root.Id)} steals {state.NameOf(terrain.Id)} from {state.NameOf(previous)}'s network.", terrain.Id);
        else
            state.Note($"{state.NameOf(root.Id)} bonds {state.NameOf(terrain.Id)}.", terrain.Id);
        terrain.Parent = root.Id;
    }

    /// <summary>D9: Collapse Network drops every bond of this root outright.</summary>
    public static void Collapse(TrueState state, Permanent root)
    {
        foreach (var terrain in BondedBy(state, root.Id).ToList())
            terrain.Parent = null;
        state.Note($"{state.NameOf(root.Id)} collapses its network.", root.Id);
    }
}
