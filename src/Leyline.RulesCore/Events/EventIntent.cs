using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Events;

/// <summary>A proposed mutation, before replacement effects (pillar 5) get a chance to intercept it.</summary>
public abstract record EventIntent;

public sealed record DamageIntent(ActorId Source, ActorId Target, int Amount) : EventIntent;
public sealed record MoveIntent(ActorId Actor, HexCoord Destination) : EventIntent;
public sealed record ApChangeIntent(ActorId Actor, int NewAp) : EventIntent;
public sealed record DestroyIntent(ActorId Actor) : EventIntent;
public sealed record CombatDeclaredIntent(CombatId Combat, ActorId Attacker, HexCoord TargetHex) : EventIntent;
public sealed record DefendersDeclaredIntent(CombatId Combat, IReadOnlyList<ActorId> Defenders) : EventIntent;
public sealed record DamageAssignedIntent(CombatId Combat, IReadOnlyDictionary<ActorId, int> Assignment) : EventIntent;
public sealed record UndefendedTargetChosenIntent(CombatId Combat, ActorId Target) : EventIntent;
public sealed record CombatResolvedIntent(CombatId Combat) : EventIntent;
public sealed record PhaseChangedIntent(int NewPhaseIndex) : EventIntent;
public sealed record TurnAdvancedIntent(int NewTurnNumber) : EventIntent;
public sealed record BondTerrainIntent(PlayerId Player, HexCoord Target) : EventIntent;
public sealed record OncePerTurnActionUsedIntent(ActorId Actor, string ActionId) : EventIntent;
public sealed record OncePerTurnActionsResetIntent(ActorId Actor) : EventIntent;
public sealed record ManaChangeIntent(PlayerId Player, int NewMana) : EventIntent;

/// <summary>D19 (provisional — flagged "confirm next session" in the source decision):
/// attacking from Underground surfaces the attacker; moving away re-conceals it.</summary>
public sealed record ActorRevealedIntent(ActorId Actor) : EventIntent;
public sealed record ActorConcealedIntent(ActorId Actor) : EventIntent;

public sealed record AddModifierIntent(IModifier Modifier) : EventIntent;
public sealed record RemoveModifierIntent(ModifierId Modifier) : EventIntent;

public sealed record CardDrawnIntent(PlayerId Player, CardDefinitionId Card) : EventIntent;
public sealed record HandCardRemovedIntent(PlayerId Player, CardDefinitionId Card) : EventIntent;

/// <summary>D37: a card discharges into Discard — the Mind domain's "past" zone — the instant
/// it's cast, fully decoupled from whatever happens later to the permanent it produces.</summary>
public sealed record CardDischargedIntent(PlayerId Player, CardDefinitionId Card) : EventIntent;

/// <summary>D50: a Past trace's Duration has elapsed — it fades out of the Aether entirely.</summary>
public sealed record TraceFadedIntent(TraceId Trace) : EventIntent;
public sealed record CreatureSummonedIntent(ActorId NewActor, PlayerId Owner, CardDefinitionId Definition, HexCoord Position) : EventIntent;
public sealed record HealIntent(ActorId Target, int Amount) : EventIntent;
public sealed record NetworkCollapsedIntent(ActorId Champion) : EventIntent;

/// <summary>A replacement effect's way of cancelling an intent outright.</summary>
public sealed record NoOpIntent : EventIntent;
