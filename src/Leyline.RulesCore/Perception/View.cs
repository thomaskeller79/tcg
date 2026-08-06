using Leyline.RulesCore.Events;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Perception;

public sealed record ActorView(ActorId Id, PlayerId Owner, int Life, int CurrentAp, HexCoord Position, Layer Layer);

public sealed record CellView(HexCoord Coord, string? Terrain, IReadOnlyList<ActorId> Ground, IReadOnlyList<ActorId> Below, IReadOnlyList<ActorId> Above);

public sealed record View(
    PlayerId Observer,
    int TurnNumber,
    PlayerId ActivePlayer,
    string CurrentPhase,
    IReadOnlyList<CellView> Cells,
    IReadOnlyList<ActorView> Actors,
    bool AwaitingYourPriority);

/// <summary>A true event, projected for one observer. 1:1 passthrough in M1 — the only
/// redaction axis (below-layer occupancy) doesn't transform event shape, only visibility.</summary>
public sealed record ObservedEvent(IEvent Projected);
