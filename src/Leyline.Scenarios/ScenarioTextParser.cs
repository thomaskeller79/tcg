using Leyline.RulesCore;
using Leyline.RulesCore.State;

namespace Leyline.Scenarios;

/// <summary>Parsed, not-yet-assembled scenario data — ScenarioLoader turns this into a Match.
/// See docs/tools/scenario-format.md for the format itself.</summary>
internal sealed class ParsedScenario
{
    public int Seed { get; set; }
    public DefendRuleVariant DefendRule { get; set; } = DefendRuleVariant.Exhaust;
    public int BoardWidth { get; set; } = 4;
    public int BoardHeight { get; set; } = 4;
    public List<(HexCoord Coord, string Terrain, int MoveCost)> TerrainOverrides { get; } = [];
    public Dictionary<CardDefinitionId, CardDefinition> Cards { get; } = [];
    public List<ChampionPlacement> Champions { get; } = [];
    public List<CreaturePlacement> Creatures { get; } = [];
    public List<TerrainBond> Bonds { get; } = [];
    public Dictionary<PlayerId, List<CardDefinitionId>> Library { get; } = new()
    {
        [new PlayerId(1)] = [],
        [new PlayerId(2)] = [],
    };
    public Dictionary<PlayerId, List<CardDefinitionId>> Hand { get; } = new()
    {
        [new PlayerId(1)] = [],
        [new PlayerId(2)] = [],
    };
}

/// <summary>Hand-rolled line parser — deliberately not a grammar/parser-generator dependency
/// for a format this small. Every failure throws InvalidDataException prefixed "line N: ",
/// since a hand-authored text format needs fast, locatable feedback on typos.</summary>
internal static class ScenarioTextParser
{
    public static ParsedScenario Parse(string text)
    {
        var scenario = new ParsedScenario();
        var lines = text.Replace("\r\n", "\n").Split('\n');

        for (var i = 0; i < lines.Length; i++)
        {
            var lineNumber = i + 1;
            var line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            try
            {
                ParseLine(scenario, Tokenize(line));
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"line {lineNumber}: {ex.Message}", ex);
            }
        }

        return scenario;
    }

    private static void ParseLine(ParsedScenario scenario, string[] tokens)
    {
        switch (tokens[0])
        {
            case "seed":
                scenario.Seed = int.Parse(tokens[1]);
                break;

            case "defendRule":
                scenario.DefendRule = Enum.Parse<DefendRuleVariant>(tokens[1], ignoreCase: true);
                break;

            case "board":
                var dims = tokens[1].Split('x');
                scenario.BoardWidth = int.Parse(dims[0]);
                scenario.BoardHeight = int.Parse(dims[1]);
                break;

            case "terrain":
            {
                var coord = ParseCoord(tokens[1]);
                var name = tokens[2];
                var kv = ParseKeyValues(tokens, from: 3);
                var moveCost = kv.TryGetValue("moveCost", out var mc) ? int.Parse(mc) : 1;
                scenario.TerrainOverrides.Add((coord, name, moveCost));
                break;
            }

            case "card":
            {
                var id = new CardDefinitionId(tokens[1]);
                var type = Enum.Parse<CardType>(tokens[2], ignoreCase: true);
                var name = tokens[3];
                var kv = ParseKeyValues(tokens, from: 4);
                var abilities = kv.TryGetValue("abilities", out var a) ? a.Split(',', StringSplitOptions.RemoveEmptyEntries) : [];
                scenario.Cards[id] = new CardDefinition(
                    id, name,
                    Attack: IntOr(kv, "attack", 0),
                    Life: IntOr(kv, "life", 0),
                    MaxAp: IntOr(kv, "ap", 0),
                    AbilityIds: abilities,
                    Type: type,
                    ManaCost: IntOr(kv, "mana", 0),
                    EffectId: kv.TryGetValue("effect", out var eff) ? "rite." + eff : null,
                    EffectAmount: IntOr(kv, "amount", 0));
                break;
            }

            case "champion":
            {
                var owner = ParsePlayer(tokens[1]);
                var cardId = new CardDefinitionId(tokens[2]);
                scenario.Champions.Add(new ChampionPlacement(owner, cardId, ParseCoord(tokens[3])));
                break;
            }

            case "creature":
            {
                var owner = ParsePlayer(tokens[1]);
                var cardId = new CardDefinitionId(tokens[2]);
                var coord = ParseCoord(tokens[3]);
                var kv = ParseKeyValues(tokens, from: 4);
                var layer = kv.TryGetValue("layer", out var l) ? Enum.Parse<Layer>(l, ignoreCase: true) : Layer.Ground;
                scenario.Creatures.Add(new CreaturePlacement(owner, cardId, coord, layer));
                break;
            }

            case "bond":
            {
                var owner = ParsePlayer(tokens[1]);
                scenario.Bonds.Add(new TerrainBond(owner, ParseCoord(tokens[2])));
                break;
            }

            case "library":
                scenario.Library[ParsePlayer(tokens[1])].AddRange(ParseCardList(tokens[2]));
                break;

            case "hand":
                scenario.Hand[ParsePlayer(tokens[1])].AddRange(ParseCardList(tokens[2]));
                break;

            default:
                throw new InvalidDataException($"unknown keyword '{tokens[0]}'.");
        }
    }

    private static IEnumerable<CardDefinitionId> ParseCardList(string token) =>
        token.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(id => new CardDefinitionId(id));

    private static HexCoord ParseCoord(string token)
    {
        var parts = token.Split(',');
        if (parts.Length != 2)
            throw new InvalidDataException($"expected 'q,r', got '{token}'.");
        return new HexCoord(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    private static PlayerId ParsePlayer(string token) => token switch
    {
        "P1" => new PlayerId(1),
        "P2" => new PlayerId(2),
        _ => throw new InvalidDataException($"expected 'P1' or 'P2', got '{token}'."),
    };

    private static Dictionary<string, string> ParseKeyValues(string[] tokens, int from) =>
        tokens.Skip(from)
            .Select(t => t.Split('=', 2))
            .ToDictionary(kv => kv[0], kv => kv.Length > 1 ? kv[1] : throw new InvalidDataException($"expected 'key=value', got '{kv[0]}'."));

    private static int IntOr(Dictionary<string, string> kv, string key, int fallback) =>
        kv.TryGetValue(key, out var v) ? int.Parse(v) : fallback;

    /// <summary>Splits on whitespace, respecting "double-quoted spans" (which may contain
    /// spaces — e.g. a card name) as single tokens with the quotes stripped.</summary>
    private static string[] Tokenize(string line)
    {
        var tokens = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (var ch in line)
        {
            if (ch == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }
            if (char.IsWhiteSpace(ch) && !inQuotes)
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }
                continue;
            }
            current.Append(ch);
        }
        if (current.Length > 0)
            tokens.Add(current.ToString());

        return tokens.ToArray();
    }
}
