using Leyline.Content.Json;
using Leyline.RulesCore;
using Leyline.RulesCore.State;

namespace Leyline.Scenarios;

/// <summary>Loads a scenario text file (see docs/tools/scenario-format.md) into a ready-to-play Match —
/// the data-driven, hand-editable replacement for hard-coded MatchFactory.CreateMatch calls
/// like TestMatches.cs.</summary>
public static class ScenarioLoader
{
    public static Match LoadFromFile(string path) => Load(File.ReadAllText(path));

    public static Match Load(string text)
    {
        var scenario = ScenarioTextParser.Parse(text);

        var p1 = new PlayerId(1);
        var p2 = new PlayerId(2);

        var board = BuildBoard(scenario);
        var content = JsonCardDefinitionRepository.FromDefinitions(scenario.Cards.Values);
        var setups = new[] { p1, p2 }.Select(p => new PlayerSetup(p, scenario.Library[p], scenario.Hand[p])).ToList();

        return MatchFactory.CreateMatch(
            board, [p1, p2], scenario.Creatures, content, (ulong)scenario.Seed,
            scenario.Champions, setups, scenario.Bonds);
    }

    private static Board BuildBoard(ParsedScenario scenario)
    {
        var cells = new List<Cell>();
        for (var q = 0; q < scenario.BoardWidth; q++)
            for (var r = 0; r < scenario.BoardHeight; r++)
                cells.Add(new Cell { Coord = new HexCoord(q, r) });

        var board = new Board(cells);
        foreach (var (coord, terrain, moveCost) in scenario.TerrainOverrides)
        {
            var cell = board.GetCell(coord);
            cell.Terrain = terrain;
            cell.MoveCost = moveCost;
        }
        return board;
    }
}
