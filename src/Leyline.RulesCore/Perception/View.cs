using Leyline.RulesCore.Model;

namespace Leyline.RulesCore.Perception;

/// <summary>One hex as an observer sees it: its terrain (always visible, D7), the network state
/// (public, D8), and a mark for terrain with static or triggered abilities (D87).</summary>
public sealed record HexView(
    HexCoord Coord,
    int TerrainId,
    string TerrainCard,
    string TerrainName,
    string? TerrainType,
    IReadOnlyList<string> Produces,
    bool IsVoid,
    int MoveCost,
    int? BondedBy,
    string? BondedByChampion,
    bool Flowing,
    bool Paused,
    bool Drawn,
    bool HasAbilities,
    string? HomeOf);

public sealed record AbilityView(string Id, string Name, string Cost, string Speed, bool Physical, string Text, string? Trigger);

/// <summary>A permanent as seen by the observer. <see cref="Controller"/> is "A", "B" or
/// "Neutral". <see cref="Pool"/> is only filled for the observer's own roots (D18: the live mana
/// balance is the one hidden standing quantity).</summary>
public sealed record PermanentView(
    int Id,
    string Card,
    string Name,
    string Kind,
    string Controller,
    HexCoord Hex,
    string Slice,
    int? Carrier,
    int Attack,
    int Life,
    int MaxLife,
    int Ap,
    int MaxAp,
    IReadOnlyList<string> Keywords,
    IReadOnlyList<AbilityView> Abilities,
    bool Locked,
    IReadOnlyList<string> UsedThisCycle,
    string? Behavior,
    IReadOnlyDictionary<string, int>? Pool,
    bool InRoot,
    bool RootConnected);

public sealed record CardView(int Id, string Card);

/// <summary>A Champion's Mind zones as the observer sees them (D71): counts are public;
/// contents only for the owner — and the owner's own Library in a canonical order, never the
/// draw order.</summary>
public sealed record ZonesView(
    string Player,
    int? ChampionId,
    int HandCount,
    IReadOnlyList<CardView>? Hand,
    int LibraryCount,
    IReadOnlyList<CardView>? Library,
    int DiscardCount,
    IReadOnlyList<CardView>? Discard,
    IReadOnlyDictionary<string, int>? Pool);

public sealed record AttackView(HexCoord Hex, string Slice, string Entity, IReadOnlyList<int> Defenders);

public sealed record TraceView(
    int Id,
    string Kind,
    string Text,
    string Controller,
    bool Physical,
    string Speed,
    bool Hidden,
    IReadOnlyList<string> Targets,
    IReadOnlyList<string> Instructions,
    IReadOnlyList<string> Notes,
    string PaidCost,
    int? FadesAtRound,
    AttackView? Attack,
    int? ActingPermanent);

public sealed record DecisionView(string Kind, string Decider, string Text, IReadOnlyList<int> Candidates, int Amount);

public sealed record CardInfo(
    string Id,
    string Name,
    string Type,
    IReadOnlyList<string> Subtypes,
    string Cost,
    string Speed,
    int Attack,
    int Life,
    int Ap,
    IReadOnlyList<string> Keywords,
    IReadOnlyList<AbilityView> Abilities,
    IReadOnlyList<string> Instructions,
    IReadOnlyList<string> Produces,
    IReadOnlyList<Element> Elements,
    string Text);

public sealed record LogView(int Seq, int Round, string Text);

/// <summary>What one observer perceives now (glossary "View"). Never true state, except the
/// debug UI's omniscient projection (architecture.md §2.9).</summary>
public sealed record View(
    string Observer,
    int TurnNumber,
    int Round,
    string ActiveSeat,
    string Phase,
    string? PriorityHolder,
    bool YourPriority,
    DecisionView? Decision,
    bool YourDecision,
    string? Winner,
    bool IsDraw,
    IReadOnlyList<HexView> Hexes,
    IReadOnlyList<PermanentView> Permanents,
    IReadOnlyList<ZonesView> Players,
    IReadOnlyList<TraceView> Pending,
    IReadOnlyList<TraceView> Past,
    TraceView? Resolving,
    IReadOnlyList<CardInfo> Cards,
    IReadOnlyList<LogView> Log,
    string MapName);
