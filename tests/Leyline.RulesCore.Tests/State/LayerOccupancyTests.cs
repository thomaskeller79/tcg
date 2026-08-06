using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Tests.State;

public class LayerOccupancyTests
{
    [Fact]
    public void Add_up_to_capacity_succeeds()
    {
        var layer = new LayerOccupancy();
        for (var i = 0; i < LayerOccupancy.Capacity; i++)
            layer.Add(new ActorId(i));

        Assert.Equal(LayerOccupancy.Capacity, layer.Occupants.Count);
        Assert.False(layer.HasRoom);
    }

    [Fact]
    public void Add_beyond_capacity_throws()
    {
        var layer = new LayerOccupancy();
        for (var i = 0; i < LayerOccupancy.Capacity; i++)
            layer.Add(new ActorId(i));

        Assert.Throws<InvalidOperationException>(() => layer.Add(new ActorId(99)));
    }

    [Fact]
    public void Remove_missing_actor_throws()
    {
        var layer = new LayerOccupancy();
        Assert.Throws<InvalidOperationException>(() => layer.Remove(new ActorId(1)));
    }

    [Fact]
    public void Remove_then_Add_frees_capacity()
    {
        var layer = new LayerOccupancy();
        var actor = new ActorId(1);
        layer.Add(actor);
        layer.Remove(actor);

        Assert.True(layer.HasRoom);
        Assert.DoesNotContain(actor, layer.Occupants);
    }
}
