using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Model;

/// <summary>How a Map fills its neutral ground (D76: no default — every map names its
/// algorithm). G15: <c>fixed</c> lists each hex's terrain; <c>random</c> draws uniformly from a
/// pool, with replacement.</summary>
public sealed record NeutralPopulation(IReadOnlyDictionary<HexCoord, string> Fixed, IReadOnlyList<string> RandomPool)
{
    public static NeutralPopulation FixedAssignment(IReadOnlyDictionary<HexCoord, string> map) => new(map, []);
    public static NeutralPopulation Random(IReadOnlyList<string> pool) => new(new Dictionary<HexCoord, string>(), pool);
}

/// <summary>A map card (map.md, D11, D52, D53): Layout, start positions, home grounds, the
/// neutral ground's population rule, Terrain Types. Holes are Void terrain (G14).</summary>
public sealed record MapDefinition
{
    public required string Name { get; init; }
    public required IReadOnlyList<HexCoord> Hexes { get; init; }
    public IReadOnlySet<HexCoord> Void { get; init; } = new HashSet<HexCoord>();
    public IReadOnlyDictionary<HexCoord, string> TerrainTypes { get; init; } = new Dictionary<HexCoord, string>();
    public required IReadOnlyDictionary<PlayerId, HexCoord> StartTiles { get; init; }
    public required IReadOnlyDictionary<PlayerId, IReadOnlyList<HexCoord>> HomeGrounds { get; init; }
    public required NeutralPopulation Neutral { get; init; }

    /// <summary>A hexagon-shaped Layout of the given radius around (0,0).</summary>
    public static IReadOnlyList<HexCoord> Hexagon(int radius)
    {
        var result = new List<HexCoord>();
        for (var q = -radius; q <= radius; q++)
            for (var r = Math.Max(-radius, -q - radius); r <= Math.Min(radius, -q + radius); r++)
                result.Add(new HexCoord(q, r));
        return result;
    }

    /// <summary>The hexes within <paramref name="radius"/> of a center, nearest first.</summary>
    public static IReadOnlyList<HexCoord> Around(HexCoord center, int radius, IEnumerable<HexCoord> layout) =>
        layout.Where(h => h.DistanceTo(center) <= radius).OrderBy(h => h.DistanceTo(center)).ThenBy(h => h).ToList();
}

/// <summary>One player's four match components minus the Map (overview.md §1, D11).</summary>
public sealed record PlayerSetup(string Champion, IReadOnlyList<string> TerrainDeck, IReadOnlyList<string> Deck);

/// <summary>A Map- or scenario-placed Neutral permanent (Setup S8). The scenario must state the
/// Behavior and neutral turn (D82).</summary>
public sealed record NeutralPlacement(string Card, HexCoord Hex, Slice? Slice, BehaviorAssignment Behavior);

public sealed record MatchSetup
{
    public required ICardDefinitionRepository Content { get; init; }
    public ulong Seed { get; init; }
    public required MapDefinition Map { get; init; }
    public required PlayerSetup A { get; init; }
    public required PlayerSetup B { get; init; }
    public IReadOnlyList<NeutralPlacement> Neutrals { get; init; } = [];

    /// <summary>Tests and scenarios may keep the deck order as written.</summary>
    public bool ShuffleLibraries { get; init; } = true;

    /// <summary>G18: opening hand 5, Champions start with at least 4 AP (placeholders).</summary>
    public int OpeningHand { get; init; } = 5;
    public int ChampionStartAp { get; init; } = 4;
}
