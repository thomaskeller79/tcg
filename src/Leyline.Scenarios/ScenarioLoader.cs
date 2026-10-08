using System.Text;
using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;

namespace Leyline.Scenarios;

/// <summary>
/// Loads a scenario text file (docs/tools/scenario-format.md) into a ready-to-play match: it
/// describes a Map and both players' components, runs the real Setup (D114), then optionally
/// applies debug edits (placements, bonds, mana, cards in hand) on top — a testing convenience,
/// not part of the rules.
/// </summary>
public static class ScenarioLoader
{
    public static TrueState LoadFromFile(string path, ICardDefinitionRepository content) => Load(File.ReadAllText(path), content);

    public static TrueState Load(string text, ICardDefinitionRepository content)
    {
        var s = Parse(text);
        var layout = s.Layout ?? throw new InvalidDataException("A scenario needs a 'map' line.");

        var homes = new Dictionary<PlayerId, IReadOnlyList<HexCoord>>();
        foreach (var player in new[] { PlayerId.A, PlayerId.B })
        {
            if (!s.Starts.TryGetValue(player, out var start))
                throw new InvalidDataException($"A scenario needs 'start {player}'.");
            homes[player] = s.HomeHexes.TryGetValue(player, out var explicitHexes)
                ? explicitHexes
                : MapDefinition.Around(start, s.HomeRadius.GetValueOrDefault(player, 0), layout.Where(h => !s.Void.Contains(h))).ToList();
        }

        var fixedMap = new Dictionary<HexCoord, string>(s.NeutralFixed);
        if (s.NeutralFill is { } fill)
        {
            foreach (var h in layout)
                if (!fixedMap.ContainsKey(h))
                    fixedMap[h] = fill;
        }

        var map = new MapDefinition
        {
            Name = s.Name,
            Hexes = layout,
            Void = s.Void,
            TerrainTypes = s.Types,
            StartTiles = s.Starts,
            HomeGrounds = homes,
            Neutral = new NeutralPopulation(fixedMap, s.NeutralPool),
        };

        var setup = new MatchSetup
        {
            Content = content,
            Seed = (ulong)s.Seed,
            Map = map,
            A = new PlayerSetup(s.Champions.GetValueOrDefault(PlayerId.A) ?? throw new InvalidDataException("A scenario needs 'champion A'."), s.TerrainDecks[PlayerId.A], s.Decks[PlayerId.A]),
            B = new PlayerSetup(s.Champions.GetValueOrDefault(PlayerId.B) ?? throw new InvalidDataException("A scenario needs 'champion B'."), s.TerrainDecks[PlayerId.B], s.Decks[PlayerId.B]),
            Neutrals = s.Neutrals,
            ShuffleLibraries = s.Shuffle,
            OpeningHand = s.OpeningHand,
            ChampionStartAp = s.StartAp,
        };

        var state = Setup.CreateMatch(setup);
        foreach (var edit in s.Edits)
            edit(state);
        if (s.Edits.Count > 0)
            Consequences.Run(state);
        return state;
    }

    private sealed class Parsed
    {
        public string Name = "";
        public int Seed;
        public IReadOnlyList<HexCoord>? Layout;
        public HashSet<HexCoord> Void = [];
        public Dictionary<HexCoord, string> Types = [];
        public Dictionary<PlayerId, HexCoord> Starts = [];
        public Dictionary<PlayerId, int> HomeRadius = [];
        public Dictionary<PlayerId, IReadOnlyList<HexCoord>> HomeHexes = [];
        public Dictionary<HexCoord, string> NeutralFixed = [];
        public List<string> NeutralPool = [];
        public string? NeutralFill;
        public Dictionary<PlayerId, string> Champions = [];
        public Dictionary<PlayerId, List<string>> TerrainDecks = new() { [PlayerId.A] = [], [PlayerId.B] = [] };
        public Dictionary<PlayerId, List<string>> Decks = new() { [PlayerId.A] = [], [PlayerId.B] = [] };
        public List<NeutralPlacement> Neutrals = [];
        public bool Shuffle = true;
        public int OpeningHand = 5;
        public int StartAp = 4;
        public List<Action<TrueState>> Edits = [];
    }

    private static Parsed Parse(string text)
    {
        var s = new Parsed();
        var lines = text.Replace("\r\n", "\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;
            try
            {
                ParseLine(s, Tokenize(line));
            }
            catch (Exception ex) when (ex is not InvalidDataException || !ex.Message.StartsWith("line "))
            {
                throw new InvalidDataException($"line {i + 1}: {ex.Message}", ex);
            }
        }
        return s;
    }

    private static void ParseLine(Parsed s, string[] t)
    {
        switch (t[0])
        {
            case "name":
                s.Name = string.Join(" ", t.Skip(1));
                break;
            case "seed":
                s.Seed = int.Parse(t[1]);
                break;
            case "map":
                s.Layout = t[1] switch
                {
                    "hexagon" => MapDefinition.Hexagon(int.Parse(t[2])),
                    "rect" => Rect(int.Parse(t[2]), int.Parse(t[3])),
                    _ => throw new InvalidDataException($"unknown map shape '{t[1]}'."),
                };
                break;
            case "void":
                foreach (var h in t.Skip(1))
                    s.Void.Add(Coord(h));
                break;
            case "type":
                foreach (var h in t.Skip(2))
                    s.Types[Coord(h)] = t[1];
                break;
            case "start":
                s.Starts[Player(t[1])] = Coord(t[2]);
                break;
            case "home":
                if (t[2].StartsWith("radius=", StringComparison.Ordinal))
                    s.HomeRadius[Player(t[1])] = int.Parse(t[2]["radius=".Length..]);
                else
                    s.HomeHexes[Player(t[1])] = t.Skip(2).Select(Coord).ToList();
                break;
            case "neutral":
                switch (t[1])
                {
                    case "fixed":
                        foreach (var kv in t.Skip(2))
                        {
                            var parts = kv.Split('=', 2);
                            s.NeutralFixed[Coord(parts[0])] = parts[1];
                        }
                        break;
                    case "random":
                        s.NeutralPool.AddRange(t.Skip(2).SelectMany(Repeat));
                        break;
                    case "fill":
                        s.NeutralFill = t[2];
                        break;
                    default:
                        throw new InvalidDataException($"unknown neutral rule '{t[1]}'.");
                }
                break;
            case "champion":
                s.Champions[Player(t[1])] = t[2];
                break;
            case "terraindeck":
                s.TerrainDecks[Player(t[1])].AddRange(t.Skip(2).SelectMany(Repeat));
                break;
            case "deck":
                s.Decks[Player(t[1])].AddRange(t.Skip(2).SelectMany(Repeat));
                break;
            case "noshuffle":
                s.Shuffle = false;
                break;
            case "openinghand":
                s.OpeningHand = int.Parse(t[1]);
                break;
            case "startap":
                s.StartAp = int.Parse(t[1]);
                break;
            case "neutralpermanent":
            {
                var kv = KeyValues(t, 3);
                var behavior = kv.GetValueOrDefault("behavior") ?? throw new InvalidDataException("a Neutral permanent needs behavior=Aggressive:A|B (D82).");
                var bparts = behavior.Split(':');
                var seat = Enum.Parse<Seat>(kv.GetValueOrDefault("seat") ?? throw new InvalidDataException("a Neutral permanent needs seat=NeutralA|NeutralB (D82)."));
                var assignment = new BehaviorAssignment(bparts[0], bparts.Length > 1 ? Player(bparts[1]) : null, seat);
                s.Neutrals.Add(new NeutralPlacement(t[1], Coord(t[2]), kv.TryGetValue("slice", out var sl) ? Enum.Parse<Slice>(sl, true) : null, assignment));
                break;
            }
            case "place":
            {
                var player = Player(t[1]);
                var card = t[2];
                var hex = Coord(t[3]);
                var kv = KeyValues(t, 4);
                s.Edits.Add(state => Place(state, player, card, hex, kv));
                break;
            }
            case "bond":
            {
                var player = Player(t[1]);
                var hexes = t.Skip(2).Select(Coord).ToList();
                s.Edits.Add(state =>
                {
                    var champion = state.ChampionOf(player)!;
                    foreach (var h in hexes)
                        state.TerrainOf(h).Parent = champion.Id;
                });
                break;
            }
            case "mana":
            {
                var player = Player(t[1]);
                var unit = ManaUnit.Parse(t[2]);
                var amount = int.Parse(t[3]);
                s.Edits.Add(state => state.ChampionOf(player)!.Pool!.Add(unit, amount));
                break;
            }
            case "handcard":
            {
                var player = Player(t[1]);
                var cards = t.Skip(2).SelectMany(Repeat).ToList();
                s.Edits.Add(state =>
                {
                    foreach (var c in cards)
                        Creation.CreateCard(state, c, player, Zone.Hand);
                });
                break;
            }
            default:
                throw new InvalidDataException($"unknown keyword '{t[0]}'.");
        }
    }

    /// <summary>Debug placement: a permanent controlled by the player's Champion (or Neutral with
    /// behavior=…), with full Activation Points unless ap= says otherwise.</summary>
    private static void Place(TrueState state, PlayerId player, string card, HexCoord hex, Dictionary<string, string> kv)
    {
        var def = state.Content.Get(card);
        var slice = kv.TryGetValue("slice", out var sl) ? Enum.Parse<Slice>(sl, true) : Activation.ExpectedSlice(def);
        var champion = state.ChampionOf(player)!;
        var p = Creation.CreatePermanent(state, def, hex, slice, champion.Id, null, null);
        Triggers.Flush(state);
        Turns.AutoResolve(state);
        p.CurrentAp = kv.TryGetValue("ap", out var ap) ? int.Parse(ap) : state.MaxAp(p);
        if (kv.TryGetValue("life", out var life))
            p.CurrentLife = int.Parse(life);
        if (kv.TryGetValue("carrier", out var carrier))
            p.Carrier = state.Permanents.First(q => q.Hex == Coord(carrier) && q.Kind.IsCreatureType() && state.Controller(q) == player).Id;
    }

    private static IReadOnlyList<HexCoord> Rect(int width, int height)
    {
        // Odd-r style rectangle in axial coordinates.
        var list = new List<HexCoord>();
        for (var r = 0; r < height; r++)
            for (var col = 0; col < width; col++)
                list.Add(new HexCoord(col - r / 2, r));
        return list;
    }

    /// <summary>"card*3" → three copies.</summary>
    private static IEnumerable<string> Repeat(string token)
    {
        var parts = token.Split('*');
        var count = parts.Length > 1 ? int.Parse(parts[1]) : 1;
        return Enumerable.Repeat(parts[0], count);
    }

    private static HexCoord Coord(string token)
    {
        var parts = token.Split(',');
        if (parts.Length != 2)
            throw new InvalidDataException($"expected 'q,r', got '{token}'.");
        return new HexCoord(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    private static PlayerId Player(string token) => token switch
    {
        "A" or "P1" => PlayerId.A,
        "B" or "P2" => PlayerId.B,
        _ => throw new InvalidDataException($"expected 'A' or 'B', got '{token}'."),
    };

    private static Dictionary<string, string> KeyValues(string[] tokens, int from) =>
        tokens.Skip(from)
            .Select(t => t.Split('=', 2))
            .ToDictionary(kv => kv[0], kv => kv.Length > 1 ? kv[1] : throw new InvalidDataException($"expected 'key=value', got '{kv[0]}'."));

    private static string[] Tokenize(string line)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
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
