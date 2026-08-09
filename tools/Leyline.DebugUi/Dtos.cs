using Leyline.RulesCore.Perception;
using Leyline.RulesCore.State;

namespace Leyline.DebugUi;

/// <summary>Kind is the command's role ("Move"/"Attack"/"Bond"/"Draw"/"CastCreature"/"CastRite"/
/// else the C# type name). ActorId/TargetHex are populated for Move/Attack (and TargetHex also
/// for CastCreature); Card is populated for CastCreature/CastRite; TargetActorId is populated
/// for CastRite (the actor it would hit) — see CommandLabeler.ToDto.</summary>
public sealed record LegalCommandDto(int Index, string Label, string Kind, int? ActorId, HexCoord? TargetHex, string? Card, int? TargetActorId = null);

/// <summary>Public card-printing info (name/type/cost/stats/effect) for every card definition
/// referenced anywhere in the current match (hand, library, or on the board) — enough for the
/// client to show a hand card's mana cost and effect text without a second round trip. Printed
/// card text isn't hidden information (D7 only hides which cards are in a zone, not their text).</summary>
public sealed record CardCatalogEntryDto(CardDefinitionId Id, string Name, string Type, int ManaCost, int Attack, int Life, int MaxAp, string? EffectId, int EffectAmount);

public sealed record SubmitRequest(int Seat, int Index);

public sealed record SubmitResultDto(bool Accepted, string? RejectionReason);

public sealed record DebugCellDto(HexCoord Coord, string? Terrain, IReadOnlyList<ActorId> Ground, IReadOnlyList<ActorId> Below, IReadOnlyList<ActorId> Above, PlayerId? NetworkOwner, bool NetworkProducing);

public sealed record DebugActorDto(ActorId Id, PlayerId Owner, string Name, string Kind, int Attack, int Life, int MaxLife, int CurrentAp, int MaxAp, IReadOnlyList<string> AbilityIds, HexCoord Position, Layer Layer);

public sealed record DamageAssignmentEntryDto(ActorId Defender, int Amount);

public sealed record ActiveCombatDto(
    CombatId Id,
    ActorId Attacker,
    HexCoord TargetHex,
    IReadOnlyList<ActorId> Defenders,
    IReadOnlyList<DamageAssignmentEntryDto>? DamageAssignment,
    ActorId? UndefendedTarget,
    CombatPhase Phase);

public sealed record PriorityWindowDto(PriorityWindowKind Kind, CombatId Context, IReadOnlyList<PlayerId> Order, PlayerId CurrentPriority);

/// <summary>Unlike View's HandView (which redacts the opponent's hand contents, D7), the true
/// state panel shows everything — Cards is always populated for both players here.</summary>
public sealed record DebugZonesDto(PlayerId Player, IReadOnlyList<CardDefinitionId> Library, IReadOnlyList<CardDefinitionId> Hand);

/// <summary>The deliberate exception to "never hand out true state" (LocalHost's own doc
/// comment) — eyeballing perceived-vs-true state is M1.5's stated exit criterion, so this
/// reads TrueState directly rather than going through a per-observer View.</summary>
public sealed record DebugStateDto(
    int TurnNumber,
    PlayerId ActivePlayer,
    string CurrentPhase,
    IReadOnlyList<DebugCellDto> Cells,
    IReadOnlyList<DebugActorDto> Actors,
    IReadOnlyList<PlayerManaView> Mana,
    IReadOnlyList<DebugZonesDto> Zones,
    PlayerId? Winner,
    IReadOnlyList<ActiveCombatDto> ActiveCombats,
    PriorityWindowDto? ActiveWindow,
    IReadOnlyList<CardCatalogEntryDto> Cards);
