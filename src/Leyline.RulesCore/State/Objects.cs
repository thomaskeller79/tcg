using Leyline.RulesCore.Model;

namespace Leyline.RulesCore.State;

/// <summary>Every game object: an ID and timestamp (D61, D93) and a `source` link to its
/// immediate predecessor (D97), joined up when the predecessor ceases to exist.</summary>
public abstract class GameObject
{
    public required ObjectId Id { get; init; }
    public required int Timestamp { get; init; }
    public ObjectId? Source { get; set; }
    public Zone Zone { get; set; }
}

/// <summary>A Mind-domain object (D59). Its parent is the Champion whose zone contains it, live
/// (D81) — <see cref="Owner"/> is that zone's owner, not a separate ownership concept.</summary>
public sealed class CardObject : GameObject
{
    public required string Definition { get; init; }
    public required PlayerId Owner { get; set; }

    /// <summary>Max-tier changes carried back from a bounced permanent (object-properties.md §3).</summary>
    public int AttackDelta { get; init; }
    public int MaxLifeDelta { get; init; }
}

/// <summary>D56/D84: a Behavior and the neutral turn it acts in, one property.</summary>
public sealed record BehaviorAssignment(string Behavior, PlayerId? Toward, Seat NeutralSeat)
{
    public const string Aggressive = "Aggressive";

    public override string ToString() =>
        Toward is { } t ? $"{Behavior} toward Champion {t} ({NeutralSeat})" : $"{Behavior} ({NeutralSeat})";
}

/// <summary>D51/D77/G1: a mana pool keyed by Element. Held by a Champion and by each Companion.</summary>
public sealed class ManaPool
{
    private readonly SortedDictionary<Element, int> _amounts = new();

    public IReadOnlyDictionary<Element, int> Amounts => _amounts;
    public int Total => _amounts.Values.Sum();

    public void Add(Element element, int amount)
    {
        _amounts[element] = _amounts.GetValueOrDefault(element) + amount;
    }

    public void Clear() => _amounts.Clear();

    /// <summary>The Elements that pay for the pips, or null if the pool can't. Colored pips
    /// first; each generic pip from the Element with the most mana left, ties by Element order (G1).</summary>
    public IReadOnlyList<Element>? PlanPayment(IReadOnlyList<Element?> pips)
    {
        var left = new SortedDictionary<Element, int>(_amounts);
        var plan = new List<Element>();
        foreach (var pip in pips.Where(p => p is not null))
        {
            var e = pip!.Value;
            if (left.GetValueOrDefault(e) <= 0)
                return null;
            left[e]--;
            plan.Add(e);
        }
        foreach (var _ in pips.Where(p => p is null))
        {
            var best = left.Where(kv => kv.Value > 0).OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).Select(kv => (Element?)kv.Key).FirstOrDefault();
            if (best is null)
                return null;
            left[best.Value]--;
            plan.Add(best.Value);
        }
        return plan;
    }

    public void Spend(IEnumerable<Element> elements)
    {
        foreach (var e in elements)
        {
            var have = _amounts.GetValueOrDefault(e);
            if (have <= 0)
                throw new InvalidOperationException($"Pool has no {e} mana to spend.");
            if (have == 1)
                _amounts.Remove(e);
            else
                _amounts[e] = have - 1;
        }
    }

    public override string ToString() =>
        _amounts.Count == 0 ? "empty" : string.Join(" ", _amounts.Select(kv => $"{kv.Key}:{kv.Value}"));
}

/// <summary>
/// A permanent on the Island (D44, D103), one class for every type; a type simply leaves the
/// fields it has no use for at their defaults (object-properties.md §5 — properties are never
/// added or removed during play, "none" is an ordinary value).
/// </summary>
public sealed class Permanent : GameObject
{
    public required string Definition { get; init; }
    public required PermanentKind Kind { get; init; }

    /// <summary>The terrain this permanent's location names (D32). For a Terrain, its own
    /// position in the Layout. Meaningless while <see cref="Carrier"/> is set — use
    /// Rules.Board.PositionOf, which follows the carrier (D105).</summary>
    public HexCoord Hex { get; set; }

    /// <summary>Creature-type permanents: their Slice. Structure: its slot (Ground or Root).
    /// Loose Item and Remnant: Ground or Root (D105, D110).</summary>
    public Slice Slice { get; set; } = Slice.Ground;

    /// <summary>An equipped Item's location is its carrying Actor (D26).</summary>
    public ObjectId? Carrier { get; set; }

    /// <summary>Stored parent (D101): Terrain → its bond record (the bonding root), Creature and
    /// Companion → the payer root fixed at creation. Read off the location for every other type.</summary>
    public ObjectId? Parent { get; set; }

    public BehaviorAssignment? Behavior { get; set; }

    public int CurrentAp { get; set; }
    public int CurrentLife { get; set; }

    /// <summary>Permanent changes to the max tiers (object-properties.md §3) — a "+2 Life
    /// permanently" raises max-Life; an until-end-of-turn buff is a modifier instead.</summary>
    public int MaxLifeDelta { get; set; }
    public int AttackDelta { get; set; }

    /// <summary>D116 `~`: can't activate abilities until the end of this turn.</summary>
    public bool Locked { get; set; }

    /// <summary>D116 `*`: ability ids used this own-turn cycle.</summary>
    public HashSet<string> UsedThisCycle { get; } = new(StringComparer.Ordinal);

    /// <summary>Champion and Companion only (D22).</summary>
    public ManaPool? Pool { get; set; }

    /// <summary>Terrain only (D77): its production has been credited this cycle.</summary>
    public bool Drawn { get; set; }

    /// <summary>Terrain only: the map's flavour classification (D52), copied at Setup.</summary>
    public string? TerrainType { get; set; }
}

/// <summary>An attack's target (D115) — a terrain, a Slice and an entity (Champion A, B or
/// Neutral; <see cref="Entity"/> null = Neutral) — plus the defenders its Defend traces have
/// added so far (D117).</summary>
public sealed class AttackInfo
{
    public required HexCoord Hex { get; init; }
    public required Slice Slice { get; init; }
    public required PlayerId? Entity { get; init; }
    public List<ObjectId> Defenders { get; } = [];
}

/// <summary>One bound choice of a target: an object, a location (terrain + optional Slice), or
/// an attack target (terrain + Slice + entity).</summary>
public sealed record TargetChoice(ObjectId? Object = null, HexCoord? Hex = null, Slice? Slice = null, bool HasEntity = false, PlayerId? Entity = null)
{
    public static TargetChoice ForObject(ObjectId id) => new(Object: id);
    public static TargetChoice ForLocation(HexCoord hex, Slice? slice = null) => new(Hex: hex, Slice: slice);
    public static TargetChoice ForAttack(HexCoord hex, Slice slice, PlayerId? entity) => new(Hex: hex, Slice: slice, HasEntity: true, Entity: entity);

    public override string ToString()
    {
        if (Object is { } o)
            return o.ToString();
        var s = Hex?.ToString() ?? "?";
        if (Slice is { } sl)
            s += $" {sl}";
        if (HasEntity)
            s += Entity is { } e ? $" vs Champion {e}" : " vs Neutral";
        return s;
    }
}

/// <summary>An Aether object (D38, D109): Permanent, Spell or Ability trace.</summary>
public sealed class TraceObject : GameObject
{
    public required TraceKind Kind { get; init; }

    /// <summary>The card name for a Permanent or Spell trace (D99); none for an Ability trace (D109).</summary>
    public string? Definition { get; init; }

    /// <summary>The ability an Ability trace runs.</summary>
    public AbilityDefinition? Ability { get; init; }

    /// <summary>The permanent whose ability created this trace (also its initial `source`).</summary>
    public ObjectId? ActingPermanent { get; init; }

    /// <summary>D81/D83: flattened to the Champion or Companion that paid, at creation.</summary>
    public ObjectId? Parent { get; set; }

    public BehaviorAssignment? Behavior { get; set; }

    /// <summary>"You", bound at declaration (D83/D91): the Champion who activated or cast it;
    /// null for a trace from a Neutral source.</summary>
    public PlayerId? You { get; init; }

    public bool Physical { get; init; }
    public int Duration { get; init; }
    public Speed Speed { get; init; }

    public Dictionary<string, List<TargetChoice>> Targets { get; } = new(StringComparer.Ordinal);

    /// <summary>The target selections, kept so a relation between two targets can be rechecked
    /// at resolution (D83).</summary>
    public IReadOnlyList<TargetSpec> Specs { get; init; } = [];
    public IReadOnlyList<Instruction> Instructions { get; init; } = [];

    /// <summary>Max-tier changes carried on a Permanent trace (cast from a bounced card, or Flicker).</summary>
    public int AttackDelta { get; init; }
    public int MaxLifeDelta { get; init; }

    public required string Text { get; set; }
    public string PaidCost { get; init; } = "";

    public AttackInfo? Attack { get; init; }

    /// <summary>Permanent trace: the location target that becomes the permanent's location (D105).</summary>
    public TargetChoice? Location { get; init; }

    public int? RoundResolved { get; set; }
    public List<string> Notes { get; } = [];

    public bool IsInstant => Speed == Speed.Instant;
}
