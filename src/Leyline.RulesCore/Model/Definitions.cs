namespace Leyline.RulesCore.Model;

/// <summary>A cost under the prototype scope (D104): a fixed list — mana pips, an AP amount with
/// its flavor (D116), and Life. Mana pips are generic or a disjunction over Elements (D125).</summary>
public sealed record Cost(IReadOnlyList<ManaPip> Mana, int Ap = 0, ApFlavor Flavor = ApFlavor.Plain, int Life = 0)
{
    public static readonly Cost Free = new([]);

    public static Cost ApOnly(int ap, ApFlavor flavor = ApFlavor.Plain) => new([], ap, flavor);

    public bool HasAp => Ap > 0 || Flavor != ApFlavor.Plain;

    public override string ToString()
    {
        var parts = new List<string>();
        var generic = Mana.Count(m => m.IsGeneric);
        if (generic > 0)
            parts.Add($"{{{generic}}}");
        foreach (var pip in Mana.Where(m => !m.IsGeneric))
            parts.Add(pip.ToString());
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
    /// <summary>A card in any Mind. Its existence and position are visible; its content may be hidden.</summary>
    Card,
    /// <summary>A trace in Past, Pending or Future.</summary>
    Trace,
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
/// different target, so they are a relation kept in the trace and rechecked at resolution.
/// <see cref="Section"/>: a card's or trace's section (D138), e.g. "Hand" or "Pending".</summary>
public sealed record TargetSpec(
    string Name,
    TargetKind Kind,
    ControlFilter Control = ControlFilter.Any,
    int? WithinOfSource = null,
    string? RelativeTo = null,
    int? WithinOfTarget = null,
    int Min = 1,
    int Max = 1,
    string? Section = null);

/// <summary>One instruction, `verb arg*` (effect-form.md). Target names a selection, or "self"
/// for the source permanent; To names a second selection (where a copy or a move goes, who an
/// Item is attached to). Section is a destination or a section of your Mind, Ability an ability
/// gained or lost (a keyword, or "card/ability-id"), Mana what add mana adds. Only the fields a
/// verb uses are set. Verb names are working names (PLAN.md Track A item 39).</summary>
public sealed record Instruction(
    string Verb,
    string? Target = null,
    int Amount = 0,
    string? Card = null,
    string? To = null,
    string? Section = null,
    string? Ability = null,
    ManaUnit? Mana = null);

/// <summary>D150 Pretend: what the opponent's copy reads instead of the true text. <see cref="Card"/>
/// is the card it seems to come from (a face: its name, and for a permanent its whole definition);
/// <see cref="Instructions"/> what it seems to do (empty: "no effect"; null: the face card's own);
/// <see cref="Duration"/> how long it seems to last (null: the true one). Instructions name the
/// card's own targets; a target no pretended instruction names stays unseen.</summary>
public sealed record Pretend(string? Card = null, IReadOnlyList<Instruction>? Instructions = null, int? Duration = null);

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
    /// <summary>D129: the kind of action (Move, Attack, Defend…), so effects can modify abilities
    /// by subtype. Not inherited from the card.</summary>
    public IReadOnlyList<string> Subtypes { get; init; } = [];
    public BuiltinAbility Builtin { get; init; }
    public TriggerEvent? Trigger { get; init; }
    public Cost Cost { get; init; } = Cost.Free;
    public Speed Speed { get; init; } = Speed.Slow;
    public bool Physical { get; init; }
    public int Duration { get; init; } = DefaultAbilities.Duration;
    public IReadOnlyList<TargetSpec> Targets { get; init; } = [];
    public IReadOnlyList<Instruction> Instructions { get; init; } = [];

    /// <summary>D150: what the opponent sees instead of the instructions.</summary>
    public Pretend? Pretend { get; init; }
    public string Text { get; init; } = "";

    public bool IsTriggered => Trigger is not null;
}

/// <summary>D129: which abilities a static cost change reaches — those of permanents standing on
/// this terrain (leaving it), or those that target this permanent or terrain (entering it).</summary>
public enum StaticScope
{
    OnThis,
    TargetingThis,
}

/// <summary>A static ability (D35, D87). The prototype builds one shape (D129): abilities with
/// <see cref="Subtype"/> cost <see cref="Ap"/> more AP — e.g. Sucking Mire, "Move abilities that
/// target this terrain cost 1 more AP".</summary>
public sealed record StaticAbility(string Subtype, StaticScope Scope, int Ap);

/// <summary>A static keyword (D10: range is a keyword). Value carries X for Haste X (0 = plain
/// Haste: enters with its max); Ranged a-b carries a in Value and b in To (D127).</summary>
public sealed record Keyword(string Name, int Value = 0, int? To = null)
{
    public const string Flying = "Flying";
    public const string Subterranean = "Subterranean";
    public const string Knotting = "Knotting";
    public const string Defender = "Defender";
    public const string Haste = "Haste";
    public const string Ranged = "Ranged";

    public override string ToString() => To is { } to ? $"{Name} {Value}-{to}" : Value > 0 ? $"{Name} {Value}" : Name;

    /// <summary>"Ranged 1-3", "Haste", "Haste 1", "Flying".</summary>
    public static Keyword Parse(string text)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var range = parts.Length == 2 ? parts[1].Split('-') : [];
        return parts.Length switch
        {
            1 => new Keyword(parts[0]),
            2 when int.TryParse(parts[1], out var v) => new Keyword(parts[0], v),
            2 when range.Length == 2 && int.TryParse(range[0], out var from) && int.TryParse(range[1], out var to) && from <= to
                => new Keyword(parts[0], from, to),
            _ => throw new FormatException($"Bad keyword '{text}'."),
        };
    }
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

    /// <summary>Printed static abilities (D129).</summary>
    public IReadOnlyList<StaticAbility> Statics { get; init; } = [];

    /// <summary>Ids of default abilities this card doesn't have (D10: "a beast can't wield a sword").</summary>
    public IReadOnlyList<string> RemovedDefaults { get; init; } = [];

    // Spell
    public IReadOnlyList<TargetSpec> Targets { get; init; } = [];
    public IReadOnlyList<Instruction> Instructions { get; init; } = [];
    public int Duration { get; init; } = DefaultAbilities.Duration;

    /// <summary>D150: what the opponent sees of this card's cast. A permanent card's Pretend is a
    /// face (a Mimic, D155: never "no permanent").</summary>
    public Pretend? Pretend { get; init; }

    // Terrain
    public IReadOnlyList<ManaUnit> Produces { get; init; } = [];
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
        Produces.Select(u => u.Colors).Concat(Cost.Mana.Select(m => m.Colors))
            .SelectMany(set => set.Elements()).Distinct().OrderBy(e => e).ToList();
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
