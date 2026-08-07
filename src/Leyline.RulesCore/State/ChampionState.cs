namespace Leyline.RulesCore.State;

/// <summary>
/// D9 (revised 2026-08-06): the win-condition target and mana-network root. An attackable
/// entity (Combat targets ActorState uniformly, so no Combat-pipeline change was needed to
/// make this a valid attack/undefended target). Mechanically a special king-like creature —
/// it runs the same mana + AP shape as CreatureState (refilled to MaxAp every Beginning
/// phase, spent directly on Move/Attack/Bond) — but is not literally a Creature card type
/// (see PLAN.md §3.2's card-type taxonomy); IHasCardDefinition is the shared trait instead of
/// inheritance.
/// </summary>
public sealed class ChampionState : ActorState, IHasCardDefinition
{
    public required CardDefinitionId Definition { get; init; }
    public TerrainNetworkState Network { get; } = new();
}
