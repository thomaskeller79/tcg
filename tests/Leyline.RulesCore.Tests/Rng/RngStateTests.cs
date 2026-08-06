using Leyline.RulesCore.Rng;

namespace Leyline.RulesCore.Tests.Rng;

public class RngStateTests
{
    [Fact]
    public void Same_seed_produces_identical_sequence()
    {
        var a = RngState.FromSeed(42);
        var b = RngState.FromSeed(42);

        for (var i = 0; i < 100; i++)
        {
            var (valueA, nextA) = a.NextUInt32();
            var (valueB, nextB) = b.NextUInt32();
            Assert.Equal(valueA, valueB);
            a = nextA;
            b = nextB;
        }
    }

    [Fact]
    public void Different_seeds_diverge()
    {
        var a = RngState.FromSeed(1);
        var b = RngState.FromSeed(2);

        var (valueA, _) = a.NextUInt32();
        var (valueB, _) = b.NextUInt32();

        Assert.NotEqual(valueA, valueB);
    }

    [Fact]
    public void NextInt_stays_within_exclusive_bound()
    {
        var rng = RngState.FromSeed(7);
        for (var i = 0; i < 500; i++)
        {
            var (value, next) = rng.NextInt(6);
            Assert.InRange(value, 0, 5);
            rng = next;
        }
    }

    [Fact]
    public void Original_state_is_unmutated_by_Next()
    {
        var original = RngState.FromSeed(123);
        var before = original.State;
        _ = original.NextUInt32();

        Assert.Equal(before, original.State);
    }
}
