using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// D130: a target-dependent cost is paid at casting against the caster's view. At the start of
/// resolution, for each target whose true surcharge is higher than what was paid, the caster may
/// pay the difference; a target not topped up becomes illegal for this trace. A lower true cost
/// is never refunded, and a top-up doesn't change the trace's cost.
/// </summary>
public static class TopUps
{
    /// <summary>Settles the trace's top-ups in order. Returns true if it is waiting for the
    /// caster's decision. A Neutral Behavior pays when it can.</summary>
    public static bool Ask(TrueState state, TraceObject trace)
    {
        if (trace.Ability is not { } ability || Payer(state, trace) is not { } payer)
            return false;
        foreach (var (choice, paid) in trace.SurchargePaid.ToList())
        {
            if (trace.IllegalTargets.Contains(choice))
                continue;
            var missing = Costs.TargetSurcharge(state, ability, choice, trace.You, trueState: true) - paid;
            if (missing <= 0)
                continue;
            var canPay = payer.CurrentAp >= missing;
            if (trace.You is { } you)
            {
                state.Decision = new TopUpDecision(you, trace.Id, choice, missing, canPay);
                return true;
            }
            Apply(state, trace, choice, missing, canPay);
        }
        return false;
    }

    public static void Apply(TrueState state, TraceObject trace, TargetChoice choice, int ap, bool pay)
    {
        if (pay && Payer(state, trace) is { } payer)
        {
            payer.CurrentAp -= ap;
            trace.SurchargePaid[choice] += ap;
            state.Note($"{trace.Text}: {ap} AP topped up for a target.", trace.Id);
            return;
        }
        trace.IllegalTargets.Add(choice);
        state.Note($"{trace.Text}: a target isn't topped up and becomes illegal.", trace.Id);
    }

    private static Permanent? Payer(TrueState state, TraceObject trace) =>
        trace.ActingPermanent is { } id && state.Find<Permanent>(id) is { } p ? state.ApPayer(p) : null;
}
