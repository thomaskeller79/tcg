using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// Immediate consequences (D112): what the rules make happen because of an instruction, in the
/// same step right after it — no trigger, no trace. Run after every instruction (a trace's,
/// a cost payment, a phase step). Covers: falling (D92, D110, D111), the match ending (D9, D113),
/// network cuts (D122) and live mana crediting (D77).
/// </summary>
public static class Consequences
{
    public static void Run(TrueState state)
    {
        for (var guard = 0; guard < 100 && !state.IsOver; guard++)
        {
            var changed = ResolveFalls(state);
            if (state.IsOver)
                return;
            changed |= Network.ApplyCuts(state);
            changed |= Network.CreditMana(state);
            if (!changed)
                break;
        }
        Observation.Update(state);
    }

    /// <summary>A permanent whose current Life is 0 or less, or that was felled, falls: it
    /// ceases to exist; a Creature or Companion leaves a Remnant, a Structure nothing; a falling
    /// Champion ends the match — a draw if both fall from the same instruction.</summary>
    private static bool ResolveFalls(TrueState state)
    {
        var fallers = state.Permanents
            .Where(p => p.Kind.IsActor() && (p.CurrentLife <= 0 || state.ToFall.Contains(p.Id)))
            .ToList();
        state.ToFall.Clear();
        if (fallers.Count == 0)
            return false;

        var fallenChampions = new List<PlayerId>();
        foreach (var p in fallers)
        {
            if (p.Kind == PermanentKind.Champion)
            {
                if (state.PlayerOfChampion(p.Id) is { } player)
                    fallenChampions.Add(player);
                state.Note($"{state.NameOf(p.Id)} falls.", p.Id);
                continue;
            }

            Triggers.Fire(state, TriggerEvent.Falls, p);
            var name = state.NameOf(p.Id);
            if (p.Kind is PermanentKind.Creature or PermanentKind.Companion)
                Creation.CreateRemnant(state, p);
            Creation.CeaseToExist(state, p.Id, $"{name} falls.");
        }

        if (fallenChampions.Count >= 2)
        {
            state.IsDraw = true;
            state.Note("Both Champions fall from the same instruction — the match is a draw.");
        }
        else if (fallenChampions.Count == 1)
        {
            state.Winner = fallenChampions[0].Opponent;
            state.Note($"Champion {state.Winner} wins.");
        }
        return true;
    }
}

/// <summary>Mind helpers (D37, D71, D138): moving a card between sections, drawing and shuffling.</summary>
public static class Mind
{
    /// <summary>D138: a move within the Mind — the same card, a new section, last in it. D154: a
    /// Champion identifies the moved card if it is <paramref name="named"/> (an instruction's
    /// target), seen (<paramref name="seenBy"/>, who also learns its content) or at a position it
    /// knows; otherwise the card becomes anonymous for it, and so does every card left in that
    /// section whose position it doesn't know. Everyone sees it arrive last.</summary>
    public static void Move(TrueState state, CardObject card, CardSection to, bool named = false, PlayerId? seenBy = null)
    {
        var mind = state.Player(card.Owner);
        var from = mind.Section(card.Section);
        foreach (var who in state.Players.Select(p => p.Id))
        {
            var belief = state.Beliefs.Card(who, card.Id);
            if (who == seenBy)
                belief.Definition = card.Definition;
            else if (!named && !belief.PositionKnown)
            {
                belief.Definition = null;
                foreach (var other in from.Where(id => id != card.Id).Select(id => state.Beliefs.Card(who, id)))
                {
                    if (!other.PositionKnown)
                        other.Definition = null;
                }
            }
            belief.PositionKnown = true;
        }
        from.Remove(card.Id);
        mind.Section(to).Add(card.Id);
        card.Section = to;
    }

    /// <summary>D149/D155: the cast card in Discard copies each belief once from its trace. The
    /// trace is true in every copy unless the card's Pretend says otherwise (not built yet, Track
    /// B item 25), so the opponent knows the card because it saw the trace.</summary>
    public static void BelieveFromTrace(TrueState state, CardObject card, TraceObject trace)
    {
        foreach (var who in state.Players.Select(p => p.Id))
            state.Beliefs.SetCard(who, card.Id, new CardBelief { Definition = trace.Definition, PositionKnown = true });
    }

    /// <summary>Shuffles a section of a Mind: nobody knows a position in it any more (D154).</summary>
    public static void ShuffleSection(TrueState state, PlayerId player, CardSection section)
    {
        var cards = state.Player(player).Section(section);
        Shuffle(state, cards);
        foreach (var who in state.Players.Select(p => p.Id))
            foreach (var id in cards)
                state.Beliefs.Card(who, id).PositionKnown = false;
    }

    public static void Draw(TrueState state, PlayerId player, int count)
    {
        var mind = state.Player(player);
        for (var i = 0; i < count; i++)
        {
            if (mind.Library.Count == 0)
            {
                state.Note($"Champion {player} can't draw: the Library is empty.");
                return;
            }
            Move(state, state.Get<CardObject>(mind.Library[0]), CardSection.Hand, seenBy: player);
        }
        state.Note($"Champion {player} draws {count} card{(count == 1 ? "" : "s")}.");
    }

    /// <summary>Fisher–Yates over the seeded RNG — the only source of randomness (overview §8).</summary>
    public static void Shuffle<T>(TrueState state, IList<T> items)
    {
        for (var i = items.Count - 1; i > 0; i--)
        {
            var (j, next) = state.Rng.NextInt(i + 1);
            state.Rng = next;
            (items[i], items[j]) = (items[j], items[i]);
        }
    }
}
