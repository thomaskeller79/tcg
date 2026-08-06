namespace Leyline.RulesCore.State;

public abstract class ActorState
{
    public required ActorId Id { get; init; }
    public required PlayerId Owner { get; init; }
    public int Life { get; set; }
    public int CurrentAp { get; set; }
    public HexCoord Position { get; set; }
    public Layer Layer { get; set; } = Layer.Ground;

    /// <summary>
    /// D19: whether a submerged actor is currently surfaced/visible regardless of layer.
    /// Always true for Ground actors. No Below-layer actor exists before Slice 5, so this
    /// field is inert until then.
    /// </summary>
    public bool Located { get; set; } = true;
}

public sealed class CreatureState : ActorState
{
    public required CardDefinitionId Definition { get; init; }
}
