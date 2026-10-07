using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rng;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// Setup (setup.md, D76, D114): Champion A's first Beginning phase. Observable, opens no
/// priority. S1 Island shape · S2 deal terrain · S3 terrain mulligan · S4 terrain enters ·
/// S5 Champions enter, bond their home tiles, AP = max(current, 4) · S6 shuffle and draw ·
/// S7 hand mulligan · S8 Neutral permanents · S9 trigger step. Mulligans: everyone keeps (G13).
/// </summary>
public static class Setup
{
    public static TrueState CreateMatch(MatchSetup setup)
    {
        var state = new TrueState { Content = setup.Content, Rng = RngState.FromSeed(setup.Seed) };
        var map = setup.Map;
        state.Phase = Phase.Setup;

        // S1 + S2: the Layout and what is dealt onto it.
        var dealt = new Dictionary<HexCoord, string>();
        foreach (var hex in map.Void)
            dealt[hex] = BuiltinCards.VoidId;
        foreach (var player in new[] { PlayerId.A, PlayerId.B })
        {
            var deck = (player == PlayerId.A ? setup.A : setup.B).TerrainDeck.ToList();
            var home = map.HomeGrounds.TryGetValue(player, out var h) ? h.OrderBy(x => x).ToList() : [];
            if (deck.Count != home.Count)
                throw new InvalidDataException($"Champion {player}'s terrain deck has {deck.Count} cards but the home ground has {home.Count} hexes (D11: they must match).");
            Mind.Shuffle(state, deck);
            for (var i = 0; i < home.Count; i++)
                dealt[home[i]] = deck[i];
        }
        foreach (var hex in map.Hexes.OrderBy(x => x))
        {
            if (dealt.ContainsKey(hex))
                continue;
            if (map.Neutral.Fixed.TryGetValue(hex, out var fixedCard))
            {
                dealt[hex] = fixedCard;
            }
            else if (map.Neutral.RandomPool.Count > 0)
            {
                var (i, next) = state.Rng.NextInt(map.Neutral.RandomPool.Count);
                state.Rng = next;
                dealt[hex] = map.Neutral.RandomPool[i];
            }
            else
            {
                throw new InvalidDataException($"The map's neutral-ground rule gives {hex} no terrain (D76).");
            }
        }

        // S3: G13 — everyone keeps.
        // S4: all terrain enters the Island, as one creation event.
        var terrainTimestamp = state.NewTimestamp();
        foreach (var (hex, card) in dealt.OrderBy(kv => kv.Key))
        {
            var terrain = Creation.CreatePermanent(state, setup.Content.Get(card), hex, Slice.Ground, null, null, null, terrainTimestamp);
            terrain.TerrainType = map.TerrainTypes.GetValueOrDefault(hex);
            state.TerrainAt[hex] = terrain.Id;
        }

        // S5: Champions enter on their home tiles, bond them (an instruction, not the Bond
        // ability), and get at least the starting AP.
        foreach (var player in new[] { PlayerId.A, PlayerId.B })
        {
            var ps = player == PlayerId.A ? setup.A : setup.B;
            var tile = map.StartTiles[player];
            var champion = Creation.CreatePermanent(state, setup.Content.Get(ps.Champion), tile, Slice.Ground, null, null, null);
            state.Player(player).Champion = champion.Id;
            state.TerrainOf(tile).Parent = champion.Id;
            champion.CurrentAp = Math.Max(champion.CurrentAp, setup.ChampionStartAp);
            state.Note($"Champion {player} bonds its home tile {tile}.", champion.Id);
        }
        Consequences.Run(state); // mana credits immediately (D77)

        // S6: shuffle and draw.
        foreach (var player in new[] { PlayerId.A, PlayerId.B })
        {
            var ps = player == PlayerId.A ? setup.A : setup.B;
            foreach (var card in ps.Deck)
                Creation.CreateCard(state, card, player, Zone.Library);
            if (setup.ShuffleLibraries)
                Mind.Shuffle(state, state.Player(player).Library);
            Mind.Draw(state, player, setup.OpeningHand);
        }

        // S7: G13 — everyone keeps.
        // S8: Map- or scenario-placed Neutral permanents.
        foreach (var n in setup.Neutrals)
        {
            var def = setup.Content.Get(n.Card);
            Creation.CreatePermanent(state, def, n.Hex, n.Slice ?? Activation.ExpectedSlice(def), null, n.Behavior, null);
        }

        // S9: every trigger from S1–S8 enters Pending as one batch and resolves without priority.
        state.Map = new MapInfo(map.Name, map.StartTiles, map.HomeGrounds);
        state.InstructionIndex = 0;
        Triggers.Flush(state);
        Turns.AutoResolve(state);

        // Then Champion A's Action phase, with the match's first priority window.
        if (!state.IsOver)
            Turns.StartActionPhase(state);
        return state;
    }
}

