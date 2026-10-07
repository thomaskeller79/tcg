using System.Text.Json;
using System.Text.Json.Serialization;
using Leyline.RulesCore.Model;

namespace Leyline.Content.Json;

/// <summary>
/// Loads card definitions from plain JSON (A5: engine-neutral content). A file holds one card
/// object or an array of them. Format: docs/tools/card-format.md.
/// </summary>
public static class CardJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static CardDefinitionRepository LoadDirectory(string path)
    {
        var defs = new List<CardDefinition>();
        foreach (var file in Directory.EnumerateFiles(path, "*.json", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal))
        {
            try
            {
                defs.AddRange(Parse(File.ReadAllText(file)));
            }
            catch (Exception ex) when (ex is JsonException or FormatException or InvalidDataException)
            {
                throw new InvalidDataException($"{Path.GetFileName(file)}: {ex.Message}", ex);
            }
        }
        return new CardDefinitionRepository(defs);
    }

    public static IReadOnlyList<CardDefinition> Parse(string json)
    {
        var trimmed = json.TrimStart();
        var dtos = trimmed.StartsWith('[')
            ? JsonSerializer.Deserialize<List<CardDto>>(json, Options) ?? []
            : [JsonSerializer.Deserialize<CardDto>(json, Options) ?? throw new InvalidDataException("Empty card file.")];
        return dtos.Select(ToDefinition).ToList();
    }

    private static CardDefinition ToDefinition(CardDto d)
    {
        if (string.IsNullOrWhiteSpace(d.Id))
            throw new InvalidDataException("A card needs an id.");
        return new CardDefinition
        {
            Id = d.Id,
            Name = d.Name ?? d.Id,
            Type = d.Type,
            Subtypes = d.Subtypes ?? [],
            Cost = ParseCost(d.Cost),
            Speed = d.Speed,
            Attack = d.Attack,
            Life = d.Life,
            Ap = d.Ap,
            Keywords = (d.Keywords ?? []).Select(ParseKeyword).ToList(),
            Abilities = (d.Abilities ?? []).Select(ToAbility).ToList(),
            RemovedDefaults = d.RemovedDefaults ?? [],
            Targets = (d.Targets ?? []).Select(ToTarget).ToList(),
            Instructions = (d.Instructions ?? []).Select(ToInstruction).ToList(),
            Duration = d.Duration ?? 5,
            Produces = d.Produces ?? [],
            MoveCost = d.MoveCost ?? 1,
            IsVoid = d.IsVoid,
            CarrierAttack = d.CarrierAttack,
            CarrierLife = d.CarrierLife,
            EntersSlice = d.EntersSlice,
            BondAp = d.BondAp ?? 3,
            Text = d.Text ?? "",
        };
    }

    private static AbilityDefinition ToAbility(AbilityDto a) => new()
    {
        Id = a.Id ?? throw new InvalidDataException("An ability needs an id."),
        Name = a.Name ?? a.Id,
        Trigger = a.Trigger,
        Cost = ParseCost(a.Cost),
        Speed = a.Speed,
        Physical = a.Physical,
        Duration = a.Duration ?? (a.Physical ? 0 : 5),
        Targets = (a.Targets ?? []).Select(ToTarget).ToList(),
        Instructions = (a.Instructions ?? []).Select(ToInstruction).ToList(),
        Text = a.Text ?? "",
    };

    private static TargetSpec ToTarget(TargetDto t) =>
        new(t.Name ?? "T1", t.Kind, t.Control, t.WithinOfSource, t.RelativeTo, t.WithinOfTarget, t.Min ?? 1, t.Max ?? 1);

    private static Instruction ToInstruction(InstructionDto i) =>
        new(i.Verb ?? throw new InvalidDataException("An instruction needs a verb."), i.Target, i.Amount, i.Card, i.Stat, i.UntilEndOfTurn);

    /// <summary>"Ranged 2", "Haste", "Haste 1", "Flying".</summary>
    public static Keyword ParseKeyword(string text)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            1 => new Keyword(parts[0]),
            2 when int.TryParse(parts[1], out var v) => new Keyword(parts[0], v),
            _ => throw new FormatException($"Bad keyword '{text}'."),
        };
    }

    /// <summary>
    /// Compact cost syntax, tokens separated by spaces or '+':
    /// a mana token is digits (generic) and Element letters — L Light, F Fire, M Metal, E Earth,
    /// D Darkness, I Ice, W Water, A Air — e.g. "2FF"; an AP token ends in "AP" with an optional
    /// flavor mark: "3AP", "3~AP", "2!AP", "5*AP"; a Life token is "2Life". Empty or "0" = free.
    /// </summary>
    public static Cost ParseCost(string? text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Trim() == "0")
            return Cost.Free;
        var mana = new List<Element?>();
        int ap = 0, life = 0;
        var flavor = ApFlavor.Plain;
        foreach (var token in text.Split([' ', '+'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.EndsWith("AP", StringComparison.OrdinalIgnoreCase))
            {
                var body = token[..^2];
                if (body.Length > 0 && body[^1] is '~' or '!' or '*')
                {
                    flavor = body[^1] switch { '~' => ApFlavor.Done, '!' => ApFlavor.Exhaust, _ => ApFlavor.OncePerCycle };
                    body = body[..^1];
                }
                ap = body.Length == 0 ? 0 : int.Parse(body);
                continue;
            }
            if (token.EndsWith("Life", StringComparison.OrdinalIgnoreCase))
            {
                life = int.Parse(token[..^4]);
                continue;
            }
            var digits = new string(token.TakeWhile(char.IsDigit).ToArray());
            if (digits.Length > 0)
                mana.AddRange(Enumerable.Repeat<Element?>(null, int.Parse(digits)));
            foreach (var ch in token[digits.Length..])
                mana.Add(ElementOf(ch));
        }
        return new Cost(mana, ap, flavor, life);
    }

    public static Element ElementOf(char letter) => char.ToUpperInvariant(letter) switch
    {
        'L' => Element.Light,
        'F' => Element.Fire,
        'M' => Element.Metal,
        'E' => Element.Earth,
        'D' => Element.Darkness,
        'I' => Element.Ice,
        'W' => Element.Water,
        'A' => Element.Air,
        _ => throw new FormatException($"Unknown Element letter '{letter}'."),
    };

    private sealed class CardDto
    {
        public string Id { get; set; } = "";
        public string? Name { get; set; }
        public CardType Type { get; set; }
        public List<string>? Subtypes { get; set; }
        public string? Cost { get; set; }
        public Speed Speed { get; set; } = Speed.Slow;
        public int Attack { get; set; }
        public int Life { get; set; }
        public int Ap { get; set; }
        public List<string>? Keywords { get; set; }
        public List<AbilityDto>? Abilities { get; set; }
        public List<string>? RemovedDefaults { get; set; }
        public List<TargetDto>? Targets { get; set; }
        public List<InstructionDto>? Instructions { get; set; }
        public int? Duration { get; set; }
        public List<Element>? Produces { get; set; }
        public int? MoveCost { get; set; }
        public bool IsVoid { get; set; }
        public int CarrierAttack { get; set; }
        public int CarrierLife { get; set; }
        public Slice? EntersSlice { get; set; }
        public int? BondAp { get; set; }
        public string? Text { get; set; }
    }

    private sealed class AbilityDto
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public TriggerEvent? Trigger { get; set; }
        public string? Cost { get; set; }
        public Speed Speed { get; set; } = Speed.Slow;
        public bool Physical { get; set; }
        public int? Duration { get; set; }
        public List<TargetDto>? Targets { get; set; }
        public List<InstructionDto>? Instructions { get; set; }
        public string? Text { get; set; }
    }

    private sealed class TargetDto
    {
        public string? Name { get; set; }
        public TargetKind Kind { get; set; }
        public ControlFilter Control { get; set; }
        public int? WithinOfSource { get; set; }
        public string? RelativeTo { get; set; }
        public int? WithinOfTarget { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
    }

    private sealed class InstructionDto
    {
        public string? Verb { get; set; }
        public string? Target { get; set; }
        public int Amount { get; set; }
        public string? Card { get; set; }
        public string? Stat { get; set; }
        public bool UntilEndOfTurn { get; set; }
    }
}
