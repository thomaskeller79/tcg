using Leyline.RulesCore.Events;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Perception;

/// <summary>D10's three stats (Attack/Life/AP), plus the two "what this resets/compares to"
/// values: MaxLife (the printed/starting Life, for a damaged-vs-fresh comparison — Life itself
/// never auto-heals, D14) and MaxAp (the effective max CurrentAp refills to every Beginning
/// phase, Query.ResolveMaxAp). Attack has no separate base/current split — it's never stored,
/// always freshly resolved via Query.ResolveAttack, so this one value already is "current."</summary>
public sealed record ActorView(ActorId Id, PlayerId Owner, string Name, string Kind, int Attack, int Life, int MaxLife, int CurrentAp, int MaxAp, IReadOnlyList<string> AbilityIds, HexCoord Position, Level Level);

/// <summary>NetworkOwner/NetworkProducing: D8's mana network is public info (needed for
/// positional denial), so this is never redacted per-observer — null means unbonded.</summary>
public sealed record CellView(HexCoord Coord, string? Terrain, IReadOnlyList<ActorId> Surface, IReadOnlyList<ActorId> Underground, IReadOnlyList<ActorId> Air, PlayerId? NetworkOwner, bool NetworkProducing);

/// <summary>D18's resource border: the mana *network* (bonded/producing terrain) is public,
/// but the live mana *balance* is the one deliberately-hidden standing quantity — Mana is only
/// populated for the observer's own entry; null for every other player's (the engine of
/// cost-deception, per `docs/rules/design-asymmetric-information.md`).</summary>
public sealed record PlayerManaView(PlayerId Player, int? Mana);

/// <summary>D7's resource border: hand *size* is public, contents are not — Count is always
/// there; Cards is populated only for the observer's own hand (null for the opponent's).</summary>
public sealed record HandView(PlayerId Player, int Count, IReadOnlyList<CardDefinitionId>? Cards);

/// <summary>The Mind domain's "future" zone (glossary). A player knows which cards are in their
/// own Library — Cards is populated for the observer's own entry (null for the opponent's, like
/// HandView) — but never the draw order: even the owner's own Cards is sorted into a canonical
/// order by ViewProjector, never PlayerState.Library's true (deterministic-for-testing, would
/// otherwise be shuffled) sequence. Count is public either way.</summary>
public sealed record LibraryView(PlayerId Player, int Count, IReadOnlyList<CardDefinitionId>? Cards);

/// <summary>D37: Discard — the Mind domain's "past" zone — is fully public (matches the genre
/// convention for a discard/graveyard-shaped zone; no rule hides it), so unlike Hand/Library
/// there's no per-observer redaction here.</summary>
public sealed record DiscardView(PlayerId Player, IReadOnlyList<CardDefinitionId> Cards);

/// <summary>A queued (Pending) or scheduled-later (Future) Aether trace, projected for display.
/// Fully public in M1 — nothing hidden (a Trap, D6) exists yet to redact.</summary>
public sealed record TraceView(TraceId Id, PlayerId Controller, string Description);

/// <summary>A resolved trace sitting in Past, with its fade window (D50).</summary>
public sealed record PastTraceView(TraceId Id, PlayerId Controller, string Description, int CreatedAtRound, int FadesAtRound);

public sealed record View(
    PlayerId Observer,
    int TurnNumber,
    int RoundNumber,
    PlayerId? ActivePlayer,
    string CurrentPhase,
    IReadOnlyList<CellView> Cells,
    IReadOnlyList<ActorView> Actors,
    IReadOnlyList<PlayerManaView> Mana,
    IReadOnlyList<HandView> Hands,
    IReadOnlyList<LibraryView> Libraries,
    IReadOnlyList<DiscardView> Discards,
    IReadOnlyList<PastTraceView> Past,
    IReadOnlyList<TraceView> Pending,
    IReadOnlyList<TraceView> Future,
    PlayerId? Winner,
    bool AwaitingYourPriority);

/// <summary>A true event, projected for one observer. 1:1 passthrough in M1 — the only
/// redaction axis (below-layer occupancy) doesn't transform event shape, only visibility.</summary>
public sealed record ObservedEvent(IEvent Projected);
