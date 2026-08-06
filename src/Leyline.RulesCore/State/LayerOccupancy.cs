namespace Leyline.RulesCore.State;

/// <summary>One vertical layer's occupants on a single cell (D12: capacity 3 per layer).</summary>
public sealed class LayerOccupancy
{
    public const int Capacity = 3;

    private readonly List<ActorId> _occupants = [];

    public IReadOnlyList<ActorId> Occupants => _occupants;

    public bool HasRoom => _occupants.Count < Capacity;

    public bool Contains(ActorId actor) => _occupants.Contains(actor);

    public void Add(ActorId actor)
    {
        if (!HasRoom)
            throw new InvalidOperationException($"Layer is at capacity ({Capacity}); cannot add {actor}.");
        if (!_occupants.Contains(actor))
            _occupants.Add(actor);
    }

    public void Remove(ActorId actor)
    {
        if (!_occupants.Remove(actor))
            throw new InvalidOperationException($"{actor} does not occupy this layer.");
    }
}
