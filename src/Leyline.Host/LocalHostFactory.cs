using Leyline.Content.Json;
using Leyline.RulesCore;
using Leyline.RulesCore.State;

namespace Leyline.Host;

/// <summary>
/// Test/dev convenience: builds a ready two-seat LocalHost without the caller ever needing
/// to import RulesCore's state types — keeps Leyline.Host.Tests honest about only talking in
/// Command/View/ObservedEvent (see LocalHost's own doc comment on the anti-pattern this guards).
/// </summary>
public static class LocalHostFactory
{
    public static (IHost Host, SeatId P1Seat, SeatId P2Seat) CreateTwoGruntSkirmish()
    {
        var p1 = new PlayerId(1);
        var p2 = new PlayerId(2);
        var gruntId = new CardDefinitionId("test.grunt");
        var content = JsonCardDefinitionRepository.FromDefinitions(
        [
            new CardDefinition(gruntId, "Grunt", Attack: 3, Life: 5, MaxAp: 3, AbilityIds: ["core.move", "core.attack"]),
        ]);

        var cells = new List<Cell>();
        for (var q = 0; q < 4; q++)
            for (var r = 0; r < 4; r++)
                cells.Add(new Cell { Coord = new HexCoord(q, r) });
        var board = new Board(cells);

        var match = MatchFactory.CreateMatch(
            board,
            [p1, p2],
            [
                new CreaturePlacement(p1, gruntId, new HexCoord(0, 0)),
                new CreaturePlacement(p2, gruntId, new HexCoord(1, 0), Layer.Below),
            ],
            new MatchConfig(DefendRuleVariant.Exhaust),
            content,
            seed: 42);

        var p1Seat = new SeatId(1);
        var p2Seat = new SeatId(2);
        var seats = new Dictionary<SeatId, PlayerId> { [p1Seat] = p1, [p2Seat] = p2 };

        return (new LocalHost(match, seats), p1Seat, p2Seat);
    }
}
