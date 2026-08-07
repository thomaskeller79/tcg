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

    /// <summary>D9's `*` cost flavor: action ids used this turn that may not repeat regardless
    /// of AP remaining or refilled (e.g. the Champion's Bond). Cleared each Beginning phase by
    /// ResetOncePerTurnActionsEffect.</summary>
    public HashSet<string> OncePerTurnActionsUsed { get; } = new();
}

public sealed class CreatureState : ActorState, IHasCardDefinition
{
    public required CardDefinitionId Definition { get; init; }
}
