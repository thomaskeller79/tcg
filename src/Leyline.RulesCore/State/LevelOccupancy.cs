namespace Leyline.RulesCore.State;

/// <summary>One vertical level's occupants on a single cell (D12: capacity 3 per level).</summary>
public sealed class LevelOccupancy
{
    public const int Capacity = 3;

    private readonly List<ActorId> _occupants = [];

    public IReadOnlyList<ActorId> Occupants => _occupants;

    public bool HasRoom => _occupants.Count < Capacity;

    public bool Contains(ActorId actor) => _occupants.Contains(actor);

    public void Add(ActorId actor)
    {
        if (!HasRoom)
            throw new InvalidOperationException($"Level is at capacity ({Capacity}); cannot add {actor}.");
        if (!_occupants.Contains(actor))
            _occupants.Add(actor);
    }

    public void Remove(ActorId actor)
    {
        if (!_occupants.Remove(actor))
            throw new InvalidOperationException($"{actor} does not occupy this level.");
    }
}
