namespace Leyline.RulesCore.State;

/// <summary>
/// D9: the win-condition target and mana-network root. An attackable entity (Combat targets
/// ActorState uniformly, so no Combat-pipeline change was needed to make this a valid
/// attack/undefended target). D9: bonding IS the Channel's "bond a terrain" option, so
/// ChannelUsedThisTurn is the single flag gating both it and Slice 4's "act as a creature"
/// option — never two separate counters. CurrentAp stays 0 until Slice 4 wires
/// ChannelActCommand (the Champion only has spendable AP on a turn it Channel-acts).
/// </summary>
public sealed class ChampionState : ActorState
{
    public required CardDefinitionId Definition { get; init; }
    public TerrainNetworkState Network { get; } = new();
    public bool ChannelUsedThisTurn { get; set; }
}
