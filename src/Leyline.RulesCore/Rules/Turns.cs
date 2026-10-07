using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// Turn structure and priority (D21, D45, D60, D92, D114; G2, G4, G5). A round is four turns:
/// Champion A, Neutral A, Champion B, Neutral B. Each turn: Beginning (refresh, triggers — no
/// priority), Action, End (triggers, expiry — no priority). In the Action phase priority moves
/// between the two Champions; `Now` advances past the top of Pending when both have passed in
/// succession, and the phase ends when both pass with Pending empty. While an Instant is in
/// Pending nobody gets priority and `Now` advances automatically.
/// </summary>
public static class Turns
{
    /// <summary>The Champion who gets priority first in this turn: the active seat's Champion,
    /// or in a neutral turn the next Champion in turn order (G2).</summary>
    public static PlayerId FirstChampion(TrueState state) =>
        state.ActiveSeat.Champion() ?? (state.ActiveSeat == Seat.NeutralA ? PlayerId.B : PlayerId.A);

    public static bool IsNeutralTurn(TrueState state) => state.ActiveSeat.Champion() is null;

    /// <summary>Permanents that refresh in this seat's Beginning phase: everything its Champion
    /// controls, or the Neutral permanents assigned to this neutral seat (D56).</summary>
    public static IReadOnlyList<Permanent> SeatPermanents(TrueState state, Seat seat)
    {
        if (seat.Champion() is { } player)
            return state.Permanents.Where(p => state.Controller(p) == player).ToList();
        return state.Permanents.Where(p => state.Controller(p) is null && p.Behavior?.NeutralSeat == seat).ToList();
    }

    public static void BeginTurn(TrueState state)
    {
        state.Phase = Phase.Beginning;
        state.PriorityHolder = null;
        var seat = state.ActiveSeat;
        state.Note($"— Round {state.Round}, {SeatName(seat)}'s turn —");

        // D50: a trace fades from Past once its Duration in rounds has passed.
        foreach (var id in state.Past.ToList())
        {
            var t = state.Get<TraceObject>(id);
            if (t.RoundResolved is { } r && state.Round >= r + t.Duration)
                Creation.CeaseToExist(state, id, "");
        }

        // D21/D77/D89: mana refresh (pool and drawn flags of this seat's roots) and AP refresh,
        // simultaneous, no checks in between.
        var permanents = SeatPermanents(state, seat);
        foreach (var root in permanents.Where(p => p.Kind.IsRoot() && p.Pool is not null))
        {
            root.Pool!.Clear();
            foreach (var terrain in Network.BondedBy(state, root.Id))
                terrain.Drawn = false;
        }
        foreach (var actor in permanents.Where(p => p.Kind.IsActor()))
        {
            actor.CurrentAp = state.MaxAp(actor);
            actor.UsedThisCycle.Clear();
        }
        Consequences.Run(state);

        foreach (var p in permanents)
            Triggers.Fire(state, TriggerEvent.BeginningOfYourTurn, p);
        Triggers.Flush(state);
        AutoResolve(state);
        if (state.IsOver)
            return;

        StartActionPhase(state);
    }

    public static void StartActionPhase(TrueState state)
    {
        state.Phase = Phase.Action;
        state.PriorityHolder = FirstChampion(state);
        state.ConsecutivePasses = 0;
        state.NeutralActionsDone = false;
        if (IsNeutralTurn(state))
            Behaviors.Step(state);
    }

    /// <summary>A Champion with priority passes.</summary>
    public static void Pass(TrueState state, PlayerId actor)
    {
        state.ConsecutivePasses++;
        state.PriorityHolder = actor.Opponent;
        if (state.ConsecutivePasses < 2)
            return;

        if (state.Pending.Count > 0)
        {
            // A Neutral permanent may still answer an Attack on it (D65, D117).
            var top = state.Get<TraceObject>(state.Pending[^1]);
            if (top.Attack is { Entity: null } && Behaviors.TryDefend(state, top))
                return;

            if (!Resolution.ResolveTop(state))
                return; // suspended for a decision
            AfterResolution(state);
            return;
        }

        EndTurn(state);
    }

    /// <summary>After a trace finished resolving: Instants auto-advance, then priority returns
    /// to the turn's first Champion; in a neutral turn the Behaviors get to act again.</summary>
    public static void AfterResolution(TrueState state)
    {
        if (state.IsOver)
            return;
        AutoAdvanceInstants(state);
        if (state.IsOver || state.Decision is not null)
            return;
        state.PriorityHolder = FirstChampion(state);
        state.ConsecutivePasses = 0;
        if (IsNeutralTurn(state) && state.Pending.Count == 0)
        {
            state.NeutralActionsDone = false;
            Behaviors.Step(state);
        }
    }

    /// <summary>After a trace entered Pending: its Champion gets priority first (G2).</summary>
    public static void AfterActivation(TrueState state, PlayerId? actor)
    {
        if (state.IsOver)
            return;
        state.PriorityHolder = actor ?? FirstChampion(state);
        state.ConsecutivePasses = 0;
        AutoAdvanceInstants(state);
    }

    /// <summary>D92: while an Instant trace is in Pending, nobody can act and `Now` advances
    /// automatically until it has resolved (triggers on top resolve first).</summary>
    public static void AutoAdvanceInstants(TrueState state)
    {
        var resolved = false;
        while (!state.IsOver && state.Decision is null && Speeds.AnyInstantPending(state))
        {
            if (!Resolution.ResolveTop(state))
                return;
            resolved = true;
        }
        if (resolved && !state.IsOver)
        {
            state.PriorityHolder = FirstChampion(state);
            state.ConsecutivePasses = 0;
        }
    }

    /// <summary>G5: outside the Action phase triggers resolve without priority.</summary>
    public static void AutoResolve(TrueState state)
    {
        while (!state.IsOver && state.Pending.Count > 0 && state.Decision is null)
        {
            if (!Resolution.ResolveTop(state))
                return;
        }
    }

    public static void EndTurn(TrueState state)
    {
        state.Phase = Phase.End;
        state.PriorityHolder = null;
        foreach (var p in SeatPermanents(state, state.ActiveSeat))
            Triggers.Fire(state, TriggerEvent.EndOfYourTurn, p);
        Triggers.Flush(state);
        AutoResolve(state);
        if (state.IsOver)
            return;

        // "Until end of turn" effects end (an instruction, so its consequences apply, D112).
        state.Modifiers.RemoveAll(m => m.UntilEndOfTurn);
        foreach (var p in state.Permanents)
        {
            p.Locked = false; // D116 `~` lasts until the end of this turn
            if (p.Kind.IsActor())
                p.CurrentLife = Math.Min(p.CurrentLife, state.MaxLife(p));
        }
        Consequences.Run(state);
        if (state.IsOver)
            return;

        state.TurnNumber++;
        BeginTurn(state);
    }

    public static string SeatName(Seat seat) => seat switch
    {
        Seat.ChampionA => "Champion A",
        Seat.ChampionB => "Champion B",
        Seat.NeutralA => "Neutral A",
        _ => "Neutral B",
    };
}
