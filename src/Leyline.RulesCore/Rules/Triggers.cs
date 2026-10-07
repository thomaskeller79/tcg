using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// Triggered abilities (D35, D92). A trigger fires when its event happens and is collected; when
/// the current trace (or setup/phase step) finishes, the collected triggers enter Pending so the
/// earliest instruction's triggers end up on top. Within one instruction they enter in this order:
/// APNAP over the four seats from the active seat (it enters first, resolves last), then source
/// ID oldest first, then printed ability order, then event order.
/// </summary>
public static class Triggers
{
    /// <summary>Fires a permanent's own triggers for an event ("when this enters/falls", "at the
    /// beginning/end of your turn"). Under the prototype scope (D104) triggers are targetless and
    /// choiceless, so even a dormant permanent's triggers happen (D83).</summary>
    public static void Fire(TrueState state, TriggerEvent ev, Permanent source)
    {
        var abilities = state.Abilities(source);
        for (var i = 0; i < abilities.Count; i++)
        {
            var ability = abilities[i];
            if (ability.Trigger != ev)
                continue;

            var controller = state.Controller(source);
            var seat = controller is { } c ? c.SeatOf() : source.Behavior?.NeutralSeat ?? Seat.NeutralA;
            state.CollectedTriggers.Add(new PendingTrigger(
                source.Id,
                ability,
                PrintedIndex: i,
                Instruction: state.InstructionIndex,
                EventIndex: state.EventCounter++,
                controller,
                seat,
                Parent: state.ManaPayer(source)?.Id,
                Behavior: controller is null ? source.Behavior : null,
                SourceName: state.NameOf(source.Id)));
        }
    }

    /// <summary>Moves every collected trigger into Pending, in the D92 order.</summary>
    public static void Flush(TrueState state)
    {
        if (state.CollectedTriggers.Count == 0)
            return;

        var active = (int)state.ActiveSeat;
        int SeatRank(Seat s) => ((int)s - active + 4) % 4;

        var groups = state.CollectedTriggers
            .GroupBy(t => t.Instruction)
            .OrderByDescending(g => g.Key);
        foreach (var group in groups)
        {
            var ordered = group
                .OrderBy(t => SeatRank(t.ControllerSeat))
                .ThenBy(t => t.SourcePermanent)
                .ThenBy(t => t.PrintedIndex)
                .ThenBy(t => t.EventIndex);
            foreach (var trigger in ordered)
                Push(state, trigger);
        }
        state.CollectedTriggers.Clear();
    }

    private static void Push(TrueState state, PendingTrigger trigger)
    {
        var sourceName = trigger.SourceName;
        var trace = new TraceObject
        {
            Id = state.NewId(),
            Timestamp = state.NewTimestamp(),
            Kind = TraceKind.Ability,
            Ability = trigger.Ability,
            ActingPermanent = trigger.SourcePermanent,
            Source = state.Exists(trigger.SourcePermanent) ? trigger.SourcePermanent : null,
            Parent = trigger.Parent,
            Behavior = trigger.Behavior,
            You = trigger.Controller,
            Physical = trigger.Ability.Physical,
            Duration = trigger.Ability.Duration,
            Speed = trigger.Ability.Speed,
            Instructions = trigger.Ability.Instructions,
            Text = $"{sourceName}: {trigger.Ability.Name}",
        };
        Creation.CreateTrace(state, trace);
        state.Note($"Trigger: {trace.Text}.", trace.Id);
    }
}
