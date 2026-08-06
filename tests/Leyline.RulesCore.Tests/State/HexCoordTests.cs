using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Tests.State;

public class HexCoordTests
{
    [Fact]
    public void S_is_derived_so_cube_coordinates_sum_to_zero()
    {
        var c = new HexCoord(2, -3);
        Assert.Equal(1, c.S);
        Assert.Equal(0, c.Q + c.R + c.S);
    }

    [Fact]
    public void Neighbors_returns_six_distinct_adjacent_coords()
    {
        var origin = new HexCoord(0, 0);
        var neighbors = origin.Neighbors().ToList();

        Assert.Equal(6, neighbors.Count);
        Assert.Equal(6, neighbors.Distinct().Count());
        Assert.All(neighbors, n => Assert.Equal(1, origin.DistanceTo(n)));
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 0)]
    [InlineData(0, 0, 3, 0, 3)]
    [InlineData(0, 0, -2, 2, 2)]
    [InlineData(1, -1, 4, -4, 3)]
    public void DistanceTo_matches_expected_hex_distance(int q1, int r1, int q2, int r2, int expected)
    {
        var a = new HexCoord(q1, r1);
        var b = new HexCoord(q2, r2);
        Assert.Equal(expected, a.DistanceTo(b));
        Assert.Equal(expected, b.DistanceTo(a));
    }

    [Fact]
    public void CompareTo_orders_by_Q_then_R()
    {
        var coords = new[] { new HexCoord(1, 5), new HexCoord(0, 9), new HexCoord(1, -1) };
        var sorted = coords.OrderBy(c => c).ToList();

        Assert.Equal(new HexCoord(0, 9), sorted[0]);
        Assert.Equal(new HexCoord(1, -1), sorted[1]);
        Assert.Equal(new HexCoord(1, 5), sorted[2]);
    }
}
