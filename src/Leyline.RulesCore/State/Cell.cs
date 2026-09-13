namespace Leyline.RulesCore.State;

public sealed class Cell
{
    public required HexCoord Coord { get; init; }
    public string? Terrain { get; set; }
    public int MoveCost { get; set; } = 1;

    public LevelOccupancy Surface { get; } = new();
    public LevelOccupancy Air { get; } = new();
    public LevelOccupancy Underground { get; } = new();

    public LevelOccupancy LevelOf(Level level) => level switch
    {
        Level.Surface => Surface,
        Level.Air => Air,
        Level.Underground => Underground,
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
    };

    /// <summary>M1 scope: Surface + Underground only — Air/flying is unused (no flyer type exists).</summary>
    public IEnumerable<ActorId> SurfaceAndUndergroundOccupants => Surface.Occupants.Concat(Underground.Occupants);
}
