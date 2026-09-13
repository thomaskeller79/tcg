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
    public required ICardDefinitionRepository Content { get; init; }

    public RngState Rng { get; set; }

    /// <summary>Ever-increasing engine-turn counter, 1-based. ActivePlayer/RoundNumber are both
    /// derived from it — see their doc comments.</summary>
    public int TurnNumber { get; set; } = 1;

    /// <summary>D60: each round is `Player[0], Neutral, Player[1], Neutral, …` (`Player A →
    /// Neutral A → Player B → Neutral B` for the 2-player case) — an even 0-based seat index is
    /// that player's own turn; an odd one is the neutral turn following them. Null means it's
    /// currently a neutral turn: nobody holds it (Neutral A/B are turn-order participants only,
    /// never a "player," `neutral-permanents.md`), so every player-scoped Beginning-phase effect
    /// (AP/mana refresh, once-per-turn reset) simply has nothing to act on until Neutral
    /// permanents/Behavior exist.</summary>
    public PlayerId? ActivePlayer =>
        SeatIndex % 2 == 0 ? Players[SeatIndex / 2].Id : null;

    /// <summary>D60: increments every time the seat order wraps back to Player[0].</summary>
    public int RoundNumber => (TurnNumber - 1) / (2 * Players.Count) + 1;

    private int SeatIndex => (TurnNumber - 1) % (2 * Players.Count);

    public int CurrentPhaseIndex { get; set; }

    /// <summary>D38: the Aether's Pending zone — traces about to resolve, LIFO.</summary>
    public PendingZone Pending { get; } = new();

    /// <summary>D38: the Aether's "past" zone — resolved traces, fading after
    /// DefaultTraceDuration rounds (ExpireFadedTracesEffect, End phase).</summary>
    public List<PastTrace> Past { get; } = [];

    /// <summary>D38: the Aether's "already instantiated but scheduled later than next" zone.
    /// Structurally real but always empty in M1 — nothing yet pushes a trace further out than
    /// Pending (delayed effects, a re-cast pushed forward) since no card does that.</summary>
    public List<Trace> Future { get; } = [];

    /// <summary>D50: Trace Duration placeholder — a Past trace fades this many rounds after it
    /// resolves. The real number is still tuning-deferred; only the mechanism is locked in.</summary>
    public const int DefaultTraceDuration = 5;

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
    private int _nextTraceId;
    private int _nextModifierId;

    public ActorId AllocateActorId() => new(_nextActorId++);
    public CombatId AllocateCombatId() => new(_nextCombatId++);
    public TraceId AllocateTraceId() => new(_nextTraceId++);
    public ModifierId AllocateModifierId() => new(_nextModifierId++);

    public PhaseDefinition CurrentPhase => PhaseSequence[CurrentPhaseIndex];

    public void AddActor(ActorState actor)
    {
        _actors.Add(actor.Id, actor);
        Board.GetCell(actor.Position).LevelOf(actor.Level).Add(actor.Id);
    }

    public ActorState? FindActor(ActorId id) => _actors.GetValueOrDefault(id);

    public ActorState GetActor(ActorId id) =>
        _actors.TryGetValue(id, out var actor) ? actor : throw new KeyNotFoundException($"No actor {id}.");

    public void RemoveActor(ActorId id)
    {
        if (_actors.Remove(id, out var actor))
            Board.GetCell(actor.Position).LevelOf(actor.Level).Remove(id);
    }

    /// <summary>All actors in canonical Id order — never dictionary enumeration order.</summary>
    public IReadOnlyList<ActorState> AllActors => _actors.Values.OrderBy(a => a.Id).ToList();

    public IEnumerable<ActorState> ActorsOwnedBy(PlayerId player) => AllActors.Where(a => a.Owner == player);

    public CombatState? FindCombat(CombatId id) => ActiveCombats.FirstOrDefault(c => c.Id == id);

    public CombatState GetCombat(CombatId id) =>
        FindCombat(id) ?? throw new KeyNotFoundException($"No active combat {id}.");
}
