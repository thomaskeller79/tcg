using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Tests.State;

public class LevelOccupancyTests
{
    [Fact]
    public void Add_up_to_capacity_succeeds()
    {
        var level = new LevelOccupancy();
        for (var i = 0; i < LevelOccupancy.Capacity; i++)
            level.Add(new ActorId(i));

        Assert.Equal(LevelOccupancy.Capacity, level.Occupants.Count);
        Assert.False(level.HasRoom);
    }

    [Fact]
    public void Add_beyond_capacity_throws()
    {
        var level = new LevelOccupancy();
        for (var i = 0; i < LevelOccupancy.Capacity; i++)
            level.Add(new ActorId(i));

        Assert.Throws<InvalidOperationException>(() => level.Add(new ActorId(99)));
    }

    [Fact]
    public void Remove_missing_actor_throws()
    {
        var level = new LevelOccupancy();
        Assert.Throws<InvalidOperationException>(() => level.Remove(new ActorId(1)));
    }

    [Fact]
    public void Remove_then_Add_frees_capacity()
    {
        var level = new LevelOccupancy();
        var actor = new ActorId(1);
        level.Add(actor);
        level.Remove(actor);

        Assert.True(level.HasRoom);
        Assert.DoesNotContain(actor, level.Occupants);
    }
}
