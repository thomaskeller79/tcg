using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Tests.State;

public class BoardTests
{
    // A small 3x3 axial rhombus: Q,R in [0,2].
    private static Board SmallBoard()
    {
        var cells = new List<Cell>();
        for (var q = 0; q < 3; q++)
            for (var r = 0; r < 3; r++)
                cells.Add(new Cell { Coord = new HexCoord(q, r) });
        return new Board(cells);
    }

    [Fact]
    public void AdjacentCoords_excludes_off_board_neighbors_and_is_canonically_ordered()
    {
        var board = SmallBoard();
        var corner = new HexCoord(0, 0);

        var adjacent = board.AdjacentCoords(corner);

        Assert.All(adjacent, c => Assert.True(board.Contains(c)));
        Assert.Equal(adjacent.OrderBy(c => c).ToList(), adjacent);
    }

    [Fact]
    public void FindPath_returns_shortest_path_between_two_cells()
    {
        var board = SmallBoard();
        var from = new HexCoord(0, 0);
        var to = new HexCoord(2, 2);

        var path = board.FindPath(from, to);

        Assert.NotNull(path);
        Assert.Equal(from, path![0]);
        Assert.Equal(to, path[^1]);
        Assert.Equal(board.Distance(from, to) + 1, path.Count);
    }

    [Fact]
    public void FindPath_respects_passable_predicate()
    {
        var board = SmallBoard();
        var from = new HexCoord(0, 0);
        var to = new HexCoord(0, 2);
        var blocked = new HexCoord(0, 1);

        var path = board.FindPath(from, to, coord => coord != blocked);

        Assert.NotNull(path);
        Assert.DoesNotContain(blocked, path!);
    }

    [Fact]
    public void FindPath_returns_null_when_target_off_board()
    {
        var board = SmallBoard();
        var path = board.FindPath(new HexCoord(0, 0), new HexCoord(99, 99));
        Assert.Null(path);
    }

    [Fact]
    public void ReachableFrom_stops_at_a_blocking_cell()
    {
        var board = SmallBoard();
        var root = new HexCoord(0, 0);
        // Block the only path further into the rhombus from directly-adjacent cells at (1,0) and (0,1).
        var reachable = board.ReachableFrom(root, c => c != new HexCoord(1, 0) && c != new HexCoord(0, 1));

        Assert.DoesNotContain(new HexCoord(1, 0), reachable);
        Assert.DoesNotContain(new HexCoord(0, 1), reachable);
    }

    [Fact]
    public void GetCell_throws_for_missing_coord()
    {
        var board = SmallBoard();
        Assert.Throws<KeyNotFoundException>(() => board.GetCell(new HexCoord(99, 99)));
    }
}
