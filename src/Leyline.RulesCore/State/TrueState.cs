using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rng;

namespace Leyline.RulesCore.State;

/// <summary>D16/D37: one Champion's Mind zones, each ordered. Library[0] is the top.</summary>
public sealed class PlayerState
{
    public required PlayerId Id { get; init; }
    public List<ObjectId> Library { get; } = [];
    public List<ObjectId> Hand { get; } = [];
    public List<ObjectId> Discard { get; } = [];
    public ObjectId? Champion { get; set; }
}

/// <summary>A continuous change to a queried value (continuous-effects.md): folded in append
/// order on every query, never stored into the permanent.</summary>
public sealed record Modifier(ModifierId Id, ObjectId Subject, string Stat, int Delta, bool UntilEndOfTurn);

/// <summary>A line of the match log. <see cref="About"/> lists the objects it mentions, so a
/// view can drop lines about objects its observer can't see.</summary>
public sealed record LogEntry(int Seq, int Round, Seat Seat, string Text, IReadOnlyList<ObjectId> About);

/// <summary>The map's Setup-time facts kept for display: start tiles and home grounds (D11).</summary>
public sealed record MapInfo(
    string Name,
    IReadOnlyDictionary<PlayerId, HexCoord> StartTiles,
    IReadOnlyDictionary<PlayerId, IReadOnlyList<HexCoord>> HomeGrounds)
{
    public static readonly MapInfo Empty = new("", new Dictionary<PlayerId, HexCoord>(), new Dictionary<PlayerId, IReadOnlyList<HexCoord>>());
}

/// <summary>A triggered ability that fired and waits to enter Pending (D92).</summary>
public sealed record PendingTrigger(
    ObjectId SourcePermanent,
    AbilityDefinition Ability,
    int PrintedIndex,
    int Instruction,
    int EventIndex,
    PlayerId? Controller,
    Seat ControllerSeat,
    ObjectId? Parent,
    BehaviorAssignment? Behavior,
    string SourceName);

/// <summary>A cast or activation, held between its steps (D70, D94); it is built and committed
/// inside one command.</summary>
public sealed class ActivationDraft
{
    public required PlayerId? Actor { get; init; }
    public required ObjectId Source { get; init; }
    public required bool IsCast { get; init; }
    public required AbilityDefinition? Ability { get; init; }
    public required CardDefinition? Card { get; init; }
    public required List<List<TargetChoice>> Targets { get; init; }
    public required Cost Cost { get; init; }
    public required IReadOnlyList<ManaUnit> ManaPlan { get; init; }
    public required ObjectId? ManaPayer { get; init; }
    public required ObjectId? ApPayer { get; init; }
    public required ObjectId? Parent { get; init; }
    public required BehaviorAssignment? Behavior { get; init; }
}

public abstract record PendingDecision(PlayerId Decider);

/// <summary>G16: an Attack resolving with more than one possible recipient — its attacker's
/// Champion splits the Attack value (D13, D117).</summary>
public sealed record DamageSplitDecision(PlayerId Decider, ObjectId AttackTrace, int Amount, IReadOnlyList<ObjectId> Candidates, bool Defended)
    : PendingDecision(Decider);

/// <summary>D130: at the start of resolution a target's true surcharge is higher than what was
/// paid — the caster may top up the difference, or the target becomes illegal for this trace.</summary>
public sealed record TopUpDecision(PlayerId Decider, ObjectId Trace, TargetChoice Target, int Ap, bool CanPay)
    : PendingDecision(Decider);

/// <summary>
/// The single source of truth for a match (architecture.md §2.1). Mutated only by Rules code;
/// every view of it goes through Perception. Id and timestamp counters live here so assignment is
/// part of the replayable state.
/// </summary>
public sealed class TrueState
{
    public required ICardDefinitionRepository Content { get; init; }
    public RngState Rng { get; set; }

    // --- Matter: the Island. Every hex holds exactly one Terrain (D100).
    public Dictionary<HexCoord, ObjectId> TerrainAt { get; } = new();
    public MapInfo Map { get; set; } = MapInfo.Empty;

    // --- Objects of every domain.
    private readonly Dictionary<ObjectId, GameObject> _objects = new();
    public PlayerState[] Players { get; } = [new PlayerState { Id = PlayerId.A }, new PlayerState { Id = PlayerId.B }];

    // --- Turn structure (D21, D60): Champion A, Neutral A, Champion B, Neutral B.
    public int TurnNumber { get; set; } = 1;
    public Seat ActiveSeat => (Seat)((TurnNumber - 1) % 4);
    public int Round => (TurnNumber - 1) / 4 + 1;
    public Phase Phase { get; set; } = Phase.Setup;

    // --- Aether (D38). Pending is bottom → top; Past oldest first.
    public List<ObjectId> Pending { get; } = [];
    public List<ObjectId> Past { get; } = [];
    public List<ObjectId> Future { get; } = [];

    /// <summary>A trace mid-resolution, waiting for a decision (G16).</summary>
    public ObjectId? Resolving { get; set; }

    // --- Priority (G2/G4).
    public PlayerId? PriorityHolder { get; set; }
    public int ConsecutivePasses { get; set; }

    /// <summary>In a neutral turn's Action phase: the Behaviors have nothing left to do this
    /// turn, so the Champions' closing pass round runs (G4).</summary>
    public bool NeutralActionsDone { get; set; }

    public PendingDecision? Decision { get; set; }

    // --- Result (D9, D113).
    public PlayerId? Winner { get; set; }
    public bool IsDraw { get; set; }
    public bool IsOver => Winner is not null || IsDraw;

    public List<Modifier> Modifiers { get; } = [];
    public List<PendingTrigger> CollectedTriggers { get; } = [];

    /// <summary>Permanents a "fell" instruction marked; they fall in the next consequences pass.</summary>
    public HashSet<ObjectId> ToFall { get; } = [];

    /// <summary>Which instruction of the resolving trace is running, and a running count of
    /// events inside it — the last two tiebreaks of trigger order (D92).</summary>
    public int InstructionIndex { get; set; }
    public int EventCounter { get; set; }
    public List<LogEntry> Log { get; } = [];

    private int _nextId = 1;
    private int _nextTimestamp = 1;
    private int _nextModifier = 1;

    public ObjectId NewId() => new(_nextId++);
    public int NewTimestamp() => _nextTimestamp++;
    public ModifierId NewModifierId() => new(_nextModifier++);

    public PlayerState Player(PlayerId id) => Players[id.Value - 1];

    public void Add(GameObject obj) => _objects.Add(obj.Id, obj);
    public void Remove(ObjectId id) => _objects.Remove(id);
    public bool Exists(ObjectId id) => _objects.ContainsKey(id);

    public T Get<T>(ObjectId id) where T : GameObject =>
        _objects.TryGetValue(id, out var obj) && obj is T t ? t : throw new KeyNotFoundException($"No {typeof(T).Name} {id}.");

    public T? Find<T>(ObjectId id) where T : GameObject =>
        _objects.TryGetValue(id, out var obj) ? obj as T : null;

    public IEnumerable<GameObject> AllObjects => _objects.Values.OrderBy(o => o.Id);

    /// <summary>Every permanent on the Island, in ID order (never dictionary order — determinism).</summary>
    public IReadOnlyList<Permanent> Permanents => _objects.Values.OfType<Permanent>().OrderBy(p => p.Id).ToList();

    public Permanent TerrainOf(HexCoord hex) => Get<Permanent>(TerrainAt[hex]);
    public IEnumerable<HexCoord> Hexes => TerrainAt.Keys.OrderBy(h => h);

    public void Note(string text, params ObjectId[] about) =>
        Log.Add(new LogEntry(Log.Count + 1, Round, ActiveSeat, text, about));
}
