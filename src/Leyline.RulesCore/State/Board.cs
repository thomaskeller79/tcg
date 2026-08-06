namespace Leyline.RulesCore.State;

public sealed class Board
{
    private readonly Dictionary<HexCoord, Cell> _cells;

    public Board(IEnumerable<Cell> cells)
    {
        _cells = cells.ToDictionary(c => c.Coord);
    }

    public bool Contains(HexCoord coord) => _cells.ContainsKey(coord);

    public Cell? TryGetCell(HexCoord coord) => _cells.GetValueOrDefault(coord);

    public Cell GetCell(HexCoord coord) =>
        _cells.TryGetValue(coord, out var cell)
            ? cell
            : throw new KeyNotFoundException($"No cell at {coord}.");

    /// <summary>All cells in canonical order — never raw dictionary enumeration order.</summary>
    public IReadOnlyList<Cell> AllCells => _cells.Values.OrderBy(c => c.Coord).ToList();

    public IReadOnlyList<HexCoord> AdjacentCoords(HexCoord coord) =>
        coord.Neighbors().Where(Contains).OrderBy(c => c).ToList();

    public int Distance(HexCoord a, HexCoord b) => a.DistanceTo(b);

    /// <summary>
    /// Shortest hop-count path between two cells (BFS), optionally restricted to a
    /// passable-cell predicate (e.g. "no enemy-occupied cells" for terrain-network
    /// reachability, Slice 3). Null if unreachable. Deterministic: frontier expansion
    /// always walks AdjacentCoords in canonical order.
    /// </summary>
    public IReadOnlyList<HexCoord>? FindPath(HexCoord from, HexCoord to, Func<HexCoord, bool>? passable = null)
    {
        if (!Contains(from) || !Contains(to))
            return null;

        var frontier = new Queue<HexCoord>();
        frontier.Enqueue(from);
        var cameFrom = new Dictionary<HexCoord, HexCoord?> { [from] = null };

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            if (current.Equals(to))
                return ReconstructPath(cameFrom, to);

            foreach (var next in AdjacentCoords(current))
            {
                if (cameFrom.ContainsKey(next))
                    continue;
                if (passable is not null && !next.Equals(to) && !passable(next))
                    continue;

                cameFrom[next] = current;
                frontier.Enqueue(next);
            }
        }

        return null;
    }

    /// <summary>
    /// All cells reachable from <paramref name="from"/> through cells matching
    /// <paramref name="passable"/> (exclusive of the start). Used for movement reachability
    /// and terrain-network connectivity (Slice 3) alike.
    /// </summary>
    public IReadOnlySet<HexCoord> ReachableFrom(HexCoord from, Func<HexCoord, bool> passable)
    {
        var visited = new SortedSet<HexCoord>();
        if (!Contains(from))
            return visited;

        var frontier = new Queue<HexCoord>();
        frontier.Enqueue(from);
        var seen = new HashSet<HexCoord> { from };

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            foreach (var next in AdjacentCoords(current))
            {
                if (!seen.Add(next))
                    continue;
                if (!passable(next))
                    continue;

                visited.Add(next);
                frontier.Enqueue(next);
            }
        }

        return visited;
    }

    private static IReadOnlyList<HexCoord> ReconstructPath(Dictionary<HexCoord, HexCoord?> cameFrom, HexCoord to)
    {
        var path = new List<HexCoord> { to };
        var current = to;
        while (cameFrom[current] is { } prev)
        {
            path.Add(prev);
            current = prev;
        }
        path.Reverse();
        return path;
    }
}
