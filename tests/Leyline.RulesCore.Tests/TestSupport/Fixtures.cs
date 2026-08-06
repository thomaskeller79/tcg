using Leyline.Content.Json;
using Leyline.RulesCore;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Tests.TestSupport;

public static class Fixtures
{
    public static readonly PlayerId P1 = new(1);
    public static readonly PlayerId P2 = new(2);
    public static readonly CardDefinitionId Grunt = new("test.grunt");

    public static ICardDefinitionRepository Content(int attack = 3, int life = 5, int maxAp = 3) =>
        JsonCardDefinitionRepository.FromDefinitions(
        [
            new CardDefinition(Grunt, "Grunt", attack, life, maxAp, ["core.move", "core.attack"]),
        ]);

    public static Board SmallBoard(int size = 4)
    {
        var cells = new List<Cell>();
        for (var q = 0; q < size; q++)
            for (var r = 0; r < size; r++)
                cells.Add(new Cell { Coord = new HexCoord(q, r) });
        return new Board(cells);
    }

    /// <summary>One grunt for each player, adjacent at (0,0) and (1,0), on a 4x4 board.</summary>
    public static Match Adjacent1v1(DefendRuleVariant variant = DefendRuleVariant.Exhaust, int attack = 3, int life = 5, int maxAp = 3) =>
        MatchFactory.CreateMatch(
            SmallBoard(),
            [P1, P2],
            [
                new CreaturePlacement(P1, Grunt, new HexCoord(0, 0)),
                new CreaturePlacement(P2, Grunt, new HexCoord(1, 0)),
            ],
            new MatchConfig(variant),
            Content(attack, life, maxAp),
            seed: 1);
}
