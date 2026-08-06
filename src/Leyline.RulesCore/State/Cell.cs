namespace Leyline.RulesCore.State;

public sealed class Cell
{
    public required HexCoord Coord { get; init; }
    public string? Terrain { get; set; }
    public int MoveCost { get; set; } = 1;

    public LayerOccupancy Ground { get; } = new();
    public LayerOccupancy Below { get; } = new();
    public LayerOccupancy Above { get; } = new();

    public LayerOccupancy LayerOf(Layer layer) => layer switch
    {
        Layer.Ground => Ground,
        Layer.Below => Below,
        Layer.Above => Above,
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null),
    };

    /// <summary>M1 scope: Ground + Below only — Above/flying is unused (no flyer type exists).</summary>
    public IEnumerable<ActorId> GroundAndBelowOccupants => Ground.Occupants.Concat(Below.Occupants);
}
