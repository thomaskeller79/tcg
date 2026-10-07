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
                return;
        }
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

/// <summary>Mind-domain helpers (D37, D71): drawing and shuffling.</summary>
public static class Mind
{
    public static void Draw(TrueState state, PlayerId player, int count)
    {
        var zones = state.Player(player);
        for (var i = 0; i < count; i++)
        {
            if (zones.Library.Count == 0)
            {
                state.Note($"Champion {player} can't draw: the Library is empty.");
                return;
            }
            var id = zones.Library[0];
            zones.Library.RemoveAt(0);
            zones.Hand.Add(id);
            state.Get<CardObject>(id).Zone = Zone.Hand;
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
