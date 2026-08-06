using Leyline.RulesCore.Queries;
using Leyline.RulesCore.Rng;

namespace Leyline.RulesCore.State;

/// <summary>
/// The single source of truth for a match. Mutated only by applying events — never
/// directly by commands. Id counters live here so id assignment is itself part of
/// replayable state (Guid.NewGuid() is banned inside RulesCore — non-deterministic).
/// </summary>
public sealed class TrueState
{
    public required Board Board { get; init; }
    public required IReadOnlyList<PlayerState> Players { get; init; }
    public required IReadOnlyList<PhaseDefinition> PhaseSequence { get; init; }
    public required MatchConfig Config { get; init; }
    public required ICardDefinitionRepository Content { get; init; }

    public RngState Rng { get; set; }
    public int TurnNumber { get; set; } = 1;
    public required PlayerId ActivePlayer { get; set; }
    public int CurrentPhaseIndex { get; set; }

    public ResolutionStack Stack { get; } = new();
    public PriorityWindow? ActiveWindow { get; set; }
    public List<CombatState> ActiveCombats { get; } = [];

    /// <summary>D9: set once the losing Champion dies. Non-null means the match is over —
    /// RulesEngine stops offering any legal commands.</summary>
    public PlayerId? Winner { get; set; }

    /// <summary>Query-layer modifiers (pillar 5). Populated only via AddModifierEvent, removed
    /// only via RemoveModifierEvent (explicit or via ExpireModifiersEffect's end-of-turn
    /// cleanup) — every query folds over this list rather than hardcoding its base answer.</summary>
    public List<IModifier> ActiveModifiers { get; } = [];

    private readonly Dictionary<ActorId, ActorState> _actors = new();

    private int _nextActorId;
    private int _nextCombatId;
    private int _nextStackItemId;
    private int _nextModifierId;

    public ActorId AllocateActorId() => new(_nextActorId++);
    public CombatId AllocateCombatId() => new(_nextCombatId++);
    public StackItemId AllocateStackItemId() => new(_nextStackItemId++);
    public ModifierId AllocateModifierId() => new(_nextModifierId++);

    public PhaseDefinition CurrentPhase => PhaseSequence[CurrentPhaseIndex];

    public void AddActor(ActorState actor)
    {
        _actors.Add(actor.Id, actor);
        Board.GetCell(actor.Position).LayerOf(actor.Layer).Add(actor.Id);
    }

    public ActorState? FindActor(ActorId id) => _actors.GetValueOrDefault(id);

    public ActorState GetActor(ActorId id) =>
        _actors.TryGetValue(id, out var actor) ? actor : throw new KeyNotFoundException($"No actor {id}.");

    public void RemoveActor(ActorId id)
    {
        if (_actors.Remove(id, out var actor))
            Board.GetCell(actor.Position).LayerOf(actor.Layer).Remove(id);
    }

    /// <summary>All actors in canonical Id order — never dictionary enumeration order.</summary>
    public IReadOnlyList<ActorState> AllActors => _actors.Values.OrderBy(a => a.Id).ToList();

    public IEnumerable<ActorState> ActorsOwnedBy(PlayerId player) => AllActors.Where(a => a.Owner == player);

    public CombatState? FindCombat(CombatId id) => ActiveCombats.FirstOrDefault(c => c.Id == id);

    public CombatState GetCombat(CombatId id) =>
        FindCombat(id) ?? throw new KeyNotFoundException($"No active combat {id}.");
}
