using Leyline.RulesCore;
using Leyline.RulesCore.State;

namespace Leyline.SimHarness;

/// <summary>Shared fixtures for the harness's batch/interactive modes.</summary>
public static class TestMatches
{
    public static readonly CardDefinitionId Grunt = new("test.grunt");
    public static readonly CardDefinitionId Champion = new("test.champion");

    public static ICardDefinitionRepository DefaultContent() =>
        Leyline.Content.Json.JsonCardDefinitionRepository.FromDefinitions(
        [
            new CardDefinition(Grunt, "Grunt (test)", Attack: 3, Life: 5, MaxAp: 3, AbilityIds: ["core.move", "core.attack"]),
            new CardDefinition(Champion, "Champion (test)", Attack: 2, Life: 15, MaxAp: 2, AbilityIds: ["core.move", "core.attack"]),
        ]);

    public static Match TwoVsTwoGruntsWithChampions(DefendRuleVariant variant, ICardDefinitionRepository content)
    {
        var p1 = new PlayerId(1);
        var p2 = new PlayerId(2);
        var board = SmallBoard();

        return MatchFactory.CreateMatch(
            board,
            [p1, p2],
            [
                new CreaturePlacement(p1, Grunt, new HexCoord(0, 0)),
                new CreaturePlacement(p1, Grunt, new HexCoord(1, 0)),
                new CreaturePlacement(p2, Grunt, new HexCoord(0, 3)),
                new CreaturePlacement(p2, Grunt, new HexCoord(1, 3)),
            ],
            new MatchConfig(variant),
            content,
            seed: 0,
            champions:
            [
                new ChampionPlacement(p1, Champion, new HexCoord(2, 0)),
                new ChampionPlacement(p2, Champion, new HexCoord(2, 3)),
            ]);
    }

    private static Board SmallBoard()
    {
        var cells = new List<Cell>();
        for (var q = 0; q < 4; q++)
            for (var r = 0; r < 4; r++)
                cells.Add(new Cell { Coord = new HexCoord(q, r) });
        return new Board(cells);
    }
}
