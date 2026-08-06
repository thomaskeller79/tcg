namespace Leyline.RulesCore.State;

public enum CombatPhase
{
    AwaitingDefenders,
    AwaitingAssignment,
    AwaitingUndefendedChoice,
    AwaitingWindow,
    Resolved,
}

/// <summary>The in-flight state of one D3/D13 Combat: {attacker, target hex, declared defenders} → resolve.</summary>
public sealed class CombatState
{
    public required CombatId Id { get; init; }
    public required ActorId Attacker { get; init; }
    public required HexCoord TargetHex { get; init; }
    public List<ActorId> Defenders { get; } = [];
    public Dictionary<ActorId, int>? DamageAssignment { get; set; }
    public ActorId? UndefendedTarget { get; set; }
    public CombatPhase Phase { get; set; } = CombatPhase.AwaitingDefenders;
}
