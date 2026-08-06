using Leyline.RulesCore;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Tests.TestSupport;

public static class TerrainFixtures
{
    /// <summary>4x4 board with a two-hop terrain chain (1,0)->(2,0), Champion-adjacent at (1,0).</summary>
    public static Board BoardWithTerrainChain()
    {
        var cells = new List<Cell>();
        for (var q = 0; q < 4; q++)
        {
            for (var r = 0; r < 4; r++)
            {
                var coord = new HexCoord(q, r);
                var cell = new Cell { Coord = coord };
                if (coord is { Q: 1, R: 0 } or { Q: 2, R: 0 })
                    cell.Terrain = "basic";
                cells.Add(cell);
            }
        }
        return new Board(cells);
    }

    public static Match ChampionWithTerrainChain(int championMaxAp = 2) =>
        MatchFactory.CreateMatch(
            BoardWithTerrainChain(),
            [Fixtures.P1, Fixtures.P2],
            creatures: [],
            new MatchConfig(DefendRuleVariant.Exhaust),
            ChampionFixtures.Content(championMaxAp: championMaxAp),
            seed: 4,
            champions: [new ChampionPlacement(Fixtures.P1, ChampionFixtures.Champion, new HexCoord(0, 0))]);
}
