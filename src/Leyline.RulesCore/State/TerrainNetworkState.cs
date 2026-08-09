namespace Leyline.RulesCore.State;

/// <summary>
/// D8's terrain/mana connection network, crude M1 scope: a handful of nodes, single
/// generic mana unit (no 8-color system). Bonding is permanent once done — only the mana
/// draw (via Query.ResolveConnectedProducingTerrain) is conditional on an enemy-free path.
/// </summary>
public sealed class TerrainNetworkState
{
    private readonly SortedSet<HexCoord> _bonded = [];

    public IReadOnlySet<HexCoord> Bonded => _bonded;

    public void Bond(HexCoord coord) => _bonded.Add(coord);

    /// <summary>D9 (under test, 2026-08-08): the Champion's free (0AP) "Collapse the network"
    /// ability — drops every bond outright. The only way to become mobile again once rooted;
    /// see ChampionActionIds.CollapseNetwork.</summary>
    public void Collapse() => _bonded.Clear();
}
