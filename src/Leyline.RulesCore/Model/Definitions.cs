namespace Leyline.RulesCore.Model;

/// <summary>A cost under the prototype scope (D104): a fixed list — mana pips, an AP amount with
/// its flavor (D116), and Life. A mana pip is an Element or, when null, generic (G1 in
/// docs/architecture/implementation-plan.md).</summary>
public sealed record Cost(IReadOnlyList<Element?> Mana, int Ap = 0, ApFlavor Flavor = ApFlavor.Plain, int Life = 0)
{
    public static readonly Cost Free = new([]);

    public static Cost ApOnly(int ap, ApFlavor flavor = ApFlavor.Plain) => new([], ap, flavor);

    public bool HasAp => Ap > 0 || Flavor != ApFlavor.Plain;

    public override string ToString()
    {
        var parts = new List<string>();
        var generic = Mana.Count(m => m is null);
        if (generic > 0)
            parts.Add($"{{{generic}}}");
        foreach (var e in Mana.Where(m => m is not null))
            parts.Add($"{{{e}}}");
        if (HasAp)
        {
            var mark = Flavor switch { ApFlavor.Exhaust => "!", ApFlavor.Done => "~", ApFlavor.OncePerCycle => "*", _ => "" };
            parts.Add($"{Ap}{mark}AP");
        }
        if (Life > 0)
            parts.Add($"{Life} Life");
        return parts.Count == 0 ? "0" : string.Join(" ", parts);
    }
}

/// <summary>What kind of object a target selection picks.</summary>
public enum TargetKind
{
    /// <summary>A creature-type permanent: Creature, Companion or Champion.</summary>
    Creature,
    /// <summary>A creature-type permanent or a Structure — anything with Life.</summary>
    Actor,
    Permanent,
    Terrain,
    Remnant,
    Item,
    Structure,
}

public enum ControlFilter
{
    Any,
    You,
    NotYou,
}

/// <summary>A target selection under the prototype scope: chosen by you, at cast (D104).
/// <see cref="WithinOfSource"/> and <see cref="Control"/> are the target's own qualifiers,
/// consumed at binding (D83); <see cref="RelativeTo"/>/<see cref="WithinOfTarget"/> name a
/// different target, so they are a relation kept in the trace and rechecked at resolution.</summary>
public sealed record TargetSpec(
    string Name,
    TargetKind Kind,
    ControlFilter Control = ControlFilter.Any,
    int? WithinOfSource = null,
    string? RelativeTo = null,
    int? WithinOfTarget = null,
    int Min = 1,
    int Max = 1);

/// <summary>One instruction, `verb arg*` (effect-form.md). Target names a selection, or "self"
/// for the source permanent. Only the fields a verb uses are set.</summary>
public sealed record Instruction(
    string Verb,
    string? Target = null,
    int Amount = 0,
    string? Card = null,
    string? Stat = null,
    bool UntilEndOfTurn = false);

public enum TriggerEvent
{
    EntersIsland,
    Falls,
    BeginningOfYourTurn,
    EndOfYourTurn,
}

/// <summary>The default abilities whose instructions the engine carries out itself (D10, D106 —
/// they are written on the card like any ability, only their verbs are built in).</summary>
public enum BuiltinAbility
{
    None,
    Move,
    Attack,
    Defend,
    Equip,
    Unequip,
    Ascend,
    Descend,
    Bond,
    Draw,
    Collapse,
}

public sealed record AbilityDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public BuiltinAbility Builtin { get; init; }
    public TriggerEvent? Trigger { get; init; }
    public Cost Cost { get; init; } = Cost.Free;
    public Speed Speed { get; init; } = Speed.Slow;
    public bool Physical { get; init; }
    public int Duration { get; init; } = 5;
    public IReadOnlyList<TargetSpec> Targets { get; init; } = [];
    public IReadOnlyList<Instruction> Instructions { get; init; } = [];
    public string Text { get; init; } = "";

    public bool IsTriggered => Trigger is not null;
}

/// <summary>A static keyword (D10: range is a keyword). Value carries N for Ranged N, X for
/// Haste X (0 = plain Haste: enters with its max).</summary>
public sealed record Keyword(string Name, int Value = 0)
{
    public const string Flying = "Flying";
    public const string Subterranean = "Subterranean";
    public const string Knotting = "Knotting";
    public const string Defender = "Defender";
    public const string Haste = "Haste";
    public const string Ranged = "Ranged";

    public override string ToString() => Value > 0 ? $"{Name} {Value}" : Name;
}

/// <summary>A card definition — the printed card a name refers to (D99). Champion and Terrain
/// cards never become Card objects; they are placed at Setup (D76, D114).</summary>
public sealed record CardDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required CardType Type { get; init; }
    public IReadOnlyList<string> Subtypes { get; init; } = [];
    public Cost Cost { get; init; } = Cost.Free;
    public Speed Speed { get; init; } = Speed.Slow;

    public int Attack { get; init; }
    public int Life { get; init; }
    public int Ap { get; init; }

    public IReadOnlyList<Keyword> Keywords { get; init; } = [];

    /// <summary>Printed activated and triggered abilities, in printed order.</summary>
    public IReadOnlyList<AbilityDefinition> Abilities { get; init; } = [];

    /// <summary>Ids of default abilities this card doesn't have (D10: "a beast can't wield a sword").</summary>
    public IReadOnlyList<string> RemovedDefaults { get; init; } = [];

    // Spell
    public IReadOnlyList<TargetSpec> Targets { get; init; } = [];
    public IReadOnlyList<Instruction> Instructions { get; init; } = [];
    public int Duration { get; init; } = 5;

    // Terrain
    public IReadOnlyList<Element> Produces { get; init; } = [];
    public int MoveCost { get; init; } = 1;
    public bool IsVoid { get; init; }

    // Item: a static bonus to its carrier (continuous-effects.md §Item).
    public int CarrierAttack { get; init; }
    public int CarrierLife { get; init; }

    /// <summary>The Slice filter (D32, D110) when the card names one explicitly; otherwise
    /// derived — Flying: Sky, else Ground (Subterranean doesn't change it, D90).</summary>
    public Slice? EntersSlice { get; init; }

    /// <summary>A Companion's Bond cost (companions.md: `3*AP`, tuning).</summary>
    public int BondAp { get; init; } = 3;

    public string Text { get; init; } = "";

    public bool HasKeyword(string name) => Keywords.Any(k => k.Name == name);
    public int KeywordValue(string name) => Keywords.FirstOrDefault(k => k.Name == name)?.Value ?? 0;

    public Slice SliceFilter => EntersSlice ?? (HasKeyword(Keyword.Flying) ? Slice.Sky : Slice.Ground);

    /// <summary>D51/D100: Elements are derived — the mana it produces and its cost's pips.</summary>
    public IReadOnlyList<Element> Elements =>
        Produces.Concat(Cost.Mana.Where(m => m is not null).Select(m => m!.Value)).Distinct().OrderBy(e => e).ToList();
}

public interface ICardDefinitionRepository
{
    CardDefinition Get(string id);
    bool Contains(string id);
    IEnumerable<CardDefinition> All { get; }
}

public sealed class CardDefinitionRepository : ICardDefinitionRepository
{
    private readonly Dictionary<string, CardDefinition> _definitions;

    public CardDefinitionRepository(IEnumerable<CardDefinition> definitions)
    {
        _definitions = new Dictionary<string, CardDefinition>(StringComparer.Ordinal);
        foreach (var def in definitions)
        {
            if (!_definitions.TryAdd(def.Id, def))
                throw new InvalidDataException($"Duplicate card definition '{def.Id}' (names are unique, D99).");
        }
        _definitions.TryAdd(BuiltinCards.VoidId, BuiltinCards.Void);
    }

    public CardDefinition Get(string id) =>
        _definitions.TryGetValue(id, out var def) ? def : throw new KeyNotFoundException($"No card definition '{id}'.");

    public bool Contains(string id) => _definitions.ContainsKey(id);

    public IEnumerable<CardDefinition> All => _definitions.Values.OrderBy(d => d.Id, StringComparer.Ordinal);
}

/// <summary>Cards the rules themselves need (G14 in docs/architecture/implementation-plan.md).</summary>
public static class BuiltinCards
{
    public const string VoidId = "void";

    public static readonly CardDefinition Void = new()
    {
        Id = VoidId,
        Name = "Void",
        Type = CardType.Terrain,
        IsVoid = true,
        Text = "A hole in the Island: produces nothing, can't be bonded or entered.",
    };
}
