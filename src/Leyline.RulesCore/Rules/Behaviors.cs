using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// Neutral permanents' Behaviors (neutral-permanents.md; D56, D62–D65, D83). A Behavior picks
/// among the permanent's currently legal activated abilities, live, at every decision point, and
/// only the next single action is taken (receding horizon). Implemented: <c>Aggressive toward X</c>
/// (G19) — attack a visible permanent of X if one is in reach (lowest Life, then lowest ID),
/// otherwise step closer to the nearest one; defend whenever it can.
/// </summary>
public static class Behaviors
{
    /// <summary>In a neutral turn's Action phase with Pending empty: take the next Behavior
    /// action, or — when no Neutral permanent of this seat has anything to do — open the
    /// Champions' closing pass round (G4).</summary>
    public static void Step(TrueState state)
    {
        if (state.IsOver || state.Decision is not null || state.Pending.Count > 0 || state.NeutralActionsDone)
            return;

        var next = NextAction(state);
        if (next is not null && Activation.Build(state, next) is { Draft: { } draft })
        {
            Activation.Commit(state, draft);
            Turns.AfterActivation(state, null);
            return;
        }

        state.NeutralActionsDone = true;
        state.PriorityHolder = Turns.FirstChampion(state);
        state.ConsecutivePasses = 0;
    }

    public static ActivationRequest? NextAction(TrueState state)
    {
        var seat = state.ActiveSeat;
        var actors = state.Permanents
            .Where(p => p.Kind.IsActor() && p.Behavior is { Behavior: BehaviorAssignment.Aggressive } b && b.NeutralSeat == seat && state.Controller(p) is null)
            .ToList();

        foreach (var p in actors)
        {
            var toward = p.Behavior!.Toward;
            var requests = Activation.RequestsFor(state, null, p).ToList();

            var attack = requests
                .Where(r => r.AbilityId == DefaultAbilities.Attack && r.Targets[0][0].Entity == toward)
                .Select(r => (Request: r, Weakest: WeakestAt(state, r.Targets[0][0], toward)))
                .Where(x => x.Weakest is not null)
                .OrderBy(x => x.Weakest!.CurrentLife)
                .ThenBy(x => x.Weakest!.Id)
                .Select(x => x.Request)
                .FirstOrDefault();
            if (attack is not null)
                return attack;

            var goals = state.Permanents
                .Where(q => q.Kind.IsActor() && q.Carrier is null && state.Controller(q) == toward && state.CanSee(null, q))
                .Select(q => q.Hex)
                .ToList();
            if (goals.Count == 0)
                continue;

            int DistanceToGoal(HexCoord h) => goals.Min(g => g.DistanceTo(h));
            var current = DistanceToGoal(p.Hex);
            var move = requests
                .Where(r => r.AbilityId == DefaultAbilities.Move)
                .Select(r => (Request: r, Distance: DistanceToGoal(r.Targets[0][0].Hex!.Value), Hex: r.Targets[0][0].Hex!.Value))
                .Where(x => x.Distance < current)
                .OrderBy(x => x.Distance)
                .ThenBy(x => x.Hex)
                .Select(x => x.Request)
                .FirstOrDefault();
            if (move is not null)
                return move;
        }
        return null;
    }

    private static Permanent? WeakestAt(TrueState state, TargetChoice target, PlayerId? entity) =>
        Combat.TargetsIn(state, target.Hex!.Value, target.Slice!.Value, entity)
            .Where(q => state.CanSee(null, q))
            .OrderBy(q => q.CurrentLife)
            .ThenBy(q => q.Id)
            .FirstOrDefault();

    /// <summary>D65/D117: a Neutral creature answers an Attack on it through the same Behavior
    /// mechanism — Aggressive defends whenever it can, lowest ID first, one Defend at a time.</summary>
    public static bool TryDefend(TrueState state, TraceObject attack)
    {
        foreach (var p in state.Permanents.Where(p => p.Behavior is not null && state.Controller(p) is null))
        {
            if (!Combat.CanDefend(state, p, attack))
                continue;
            var request = new ActivationRequest(null, p.Id, DefaultAbilities.Defend, [[TargetChoice.ForObject(attack.Id)]]);
            if (Activation.Build(state, request) is { Draft: { } draft })
            {
                Activation.Commit(state, draft);
                Turns.AfterActivation(state, null);
                return true;
            }
        }
        return false;
    }
}
