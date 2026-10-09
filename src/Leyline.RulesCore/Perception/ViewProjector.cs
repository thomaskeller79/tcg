using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Perception;

/// <summary>
/// (TrueState, observer) → View (architecture.md §2.2). Redaction rules: hidden permanents (Root,
/// D12/D67) are left out; mana pools are shown only for the observer's own roots (D18); Hand and
/// Discard contents only to their owner, Library contents to the owner in canonical order (D71);
/// a trace whose acting permanent is hidden shows as a hidden action (G17). The omniscient
/// projection is the debug UI's sanctioned true-state exception (architecture.md §2.9).
/// </summary>
public static class ViewProjector
{
    public static View Project(TrueState state, PlayerId? observer, bool omniscient = false, int logLines = 60)
    {
        bool Sees(Permanent p) => omniscient || state.CanSee(observer, p);
        bool Owns(PlayerId player) => omniscient || observer == player;

        var homeOf = new Dictionary<HexCoord, string>();
        foreach (var (player, hexes) in state.Map.HomeGrounds)
            foreach (var h in hexes)
                homeOf[h] = player.ToString();

        var hexViews = state.Hexes.Select(hex =>
        {
            var t = state.TerrainOf(hex);
            var def = state.Def(t);
            var bonder = Network.BonderOf(state, t);
            return new HexView(
                hex, t.Id.Value, def.Id, def.Name, t.TerrainType, def.Produces.Select(u => u.ToString()).ToList(), def.IsVoid, def.Text,
                bonder?.Id.Value,
                bonder is null ? null : ControllerName(state.Controller(bonder)),
                Network.IsFlowing(state, t),
                Network.IsPaused(state, t),
                t.Drawn,
                def.Abilities.Count > 0 || def.Statics.Count > 0,
                homeOf.GetValueOrDefault(hex));
        }).ToList();

        var permanents = state.Permanents
            .Where(p => p.Kind != PermanentKind.Terrain && Sees(p))
            .Select(p => ToView(state, p, observer, omniscient))
            .ToList();

        var players = state.Players.Select(ps =>
        {
            var owner = Owns(ps.Id);
            var champion = state.ChampionOf(ps.Id);
            return new ZonesView(
                ps.Id.ToString(),
                champion?.Id.Value,
                ps.Hand.Count,
                owner ? ps.Hand.Select(id => Card(state, id)).ToList() : null,
                ps.Library.Count,
                owner ? (omniscient ? ps.Library.AsEnumerable() : ps.Library.OrderBy(id => state.Get<CardObject>(id).Definition, StringComparer.Ordinal).ThenBy(id => id)).Select(id => Card(state, id)).ToList() : null,
                ps.Discard.Count,
                owner ? ps.Discard.Select(id => Card(state, id)).ToList() : null,
                owner && champion?.Pool is { } pool ? Pool(pool) : null);
        }).ToList();

        var pending = state.Pending.Select(id => Trace(state, state.Get<TraceObject>(id), observer, omniscient)).ToList();
        var past = state.Past.Select(id => Trace(state, state.Get<TraceObject>(id), observer, omniscient)).ToList();
        var resolving = state.Resolving is { } r ? Trace(state, state.Get<TraceObject>(r), observer, omniscient) : null;

        DecisionView? decision = state.Decision switch
        {
            DamageSplitDecision d => new DecisionView("DamageSplit", d.Decider.ToString(),
                $"{(d.Defended ? "Defended" : "Undefended")}: split {d.Amount} damage", d.Candidates.Select(c => c.Value).ToList(), d.Amount),
            TopUpDecision t => new DecisionView("TopUp", t.Decider.ToString(),
                $"Top up {t.Ap} AP for {(t.Target.Object is not { } target ? t.Target.ToString() : omniscient || state.CanSee(observer, target) ? state.NameOf(target) : "a hidden object")}, or it becomes illegal for this trace",
                [], t.Ap),
            _ => null,
        };

        var log = state.Log
            .Where(e => omniscient || e.About.All(id => !state.Exists(id) || state.CanSee(observer, id)))
            .TakeLast(logLines)
            .Select(e => new LogView(e.Seq, e.Round, e.Text))
            .ToList();

        return new View(
            omniscient ? "Omniscient" : observer?.ToString() ?? "Neutral",
            state.TurnNumber,
            state.Round,
            Turns.SeatName(state.ActiveSeat),
            state.Phase.ToString(),
            state.PriorityHolder?.ToString(),
            observer is { } o && state.PriorityHolder == o && state.Decision is null && !state.IsOver,
            decision,
            observer is { } o2 && state.Decision?.Decider == o2,
            state.Winner?.ToString(),
            state.IsDraw,
            hexViews,
            permanents,
            players,
            pending,
            past,
            resolving,
            state.Content.All.Select(CardInfoOf).ToList(),
            log,
            state.Map.Name);
    }

    public static string ControllerName(PlayerId? controller) => controller?.ToString() ?? "Neutral";

    private static CardView Card(TrueState state, ObjectId id) => new(id.Value, state.Get<CardObject>(id).Definition);

    private static IReadOnlyDictionary<string, int> Pool(ManaPool pool) =>
        pool.Amounts.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);

    private static PermanentView ToView(TrueState state, Permanent p, PlayerId? observer, bool omniscient)
    {
        var def = state.Def(p);
        var controller = state.Controller(p);
        var showPool = p.Pool is not null && (omniscient || (observer is not null && controller == observer));
        return new PermanentView(
            p.Id.Value,
            def.Id,
            p.Kind == PermanentKind.Remnant ? $"Remnant of {def.Name}" : def.Name,
            p.Kind.ToString(),
            ControllerName(controller),
            state.PositionOf(p),
            p.Slice.ToString(),
            p.Carrier?.Value,
            state.Attack(p),
            p.Kind.IsActor() ? p.CurrentLife : 0,
            state.MaxLife(p),
            p.CurrentAp,
            state.MaxAp(p),
            p.Kind == PermanentKind.Remnant ? [] : def.Keywords.Select(k => k.ToString()).ToList(),
            state.Abilities(p).Select(AbilityOf).ToList(),
            p.Locked,
            p.UsedThisCycle.OrderBy(x => x, StringComparer.Ordinal).ToList(),
            p.Behavior?.ToString(),
            showPool ? Pool(p.Pool!) : null,
            Sight.IsInRoot(p),
            p.Kind.IsRoot() && Network.IsRootConnected(state, p));
    }

    public static AbilityView AbilityOf(AbilityDefinition a) =>
        new(a.Id, a.Name, a.Cost.ToString(), a.Speed.ToString(), a.Physical,
            a.Text.Length > 0 ? a.Text : string.Join(" ", a.Instructions.Select(i => InstructionText(i, name => a.Targets.FirstOrDefault(t => t.Name == name) is { } spec ? SpecText(spec) : null))),
            a.Trigger?.ToString());

    public static CardInfo CardInfoOf(CardDefinition d) =>
        new(d.Id, d.Name, d.Type.ToString(), d.Subtypes, d.Cost.ToString(), d.Speed.ToString(), d.Attack, d.Life, d.Ap,
            d.Keywords.Select(k => k.ToString()).ToList(),
            (d.Type is CardType.Spell or CardType.Terrain ? d.Abilities : DefaultAbilities.For(d)).Select(AbilityOf).ToList(),
            d.Instructions.Select(i => InstructionText(i, name => d.Targets.FirstOrDefault(t => t.Name == name) is { } spec ? SpecText(spec) : null)).ToList(),
            d.Produces.Select(u => u.ToString()).ToList(), d.Elements, d.Text);

    private static TraceView Trace(TrueState state, TraceObject t, PlayerId? observer, bool omniscient)
    {
        var hidden = !omniscient && !state.CanSeeTrace(observer, t);
        var controller = ControllerName(t.You);
        if (hidden)
            return new TraceView(t.Id.Value, t.Kind.ToString(), "A hidden action", controller, t.Physical, t.Speed.ToString(), true, [], [], [], "", null, null, null);

        string NameIfVisible(ObjectId id) =>
            state.Find<GameObject>(id) is Permanent p && !omniscient && !state.CanSee(observer, p) ? "a hidden object" : state.NameOf(id);

        var targets = t.Targets
            .SelectMany(kv => kv.Value.Select(c => $"{kv.Key}: {(c.Object is { } o ? NameIfVisible(o) : c.ToString())}"))
            .ToList();
        if (t.Location is { } loc)
            targets.Add($"location: {loc}");

        AttackView? attack = t.Attack is { } a
            ? new AttackView(a.Hex, a.Slice.ToString(), ControllerName(a.Entity),
                a.Defenders.Where(d => state.Find<Permanent>(d) is not { } dp || omniscient || state.CanSee(observer, dp)).Select(d => d.Value).ToList())
            : null;

        var instructions = t.Instructions.Select(i => InstructionText(i, name =>
            t.Targets.TryGetValue(name, out var choices) && choices.Count > 0
                ? string.Join(" and ", choices.Select(c => c.Object is { } o ? NameIfVisible(o) : c.ToString()))
                : name == "self" && t.ActingPermanent is { } ap ? NameIfVisible(ap) : null)).ToList();

        int? fades = t.RoundResolved is { } r ? r + t.Duration : null;
        return new TraceView(t.Id.Value, t.Kind.ToString(), t.Text, controller, t.Physical, t.Speed.ToString(), false,
            targets, instructions, t.Notes.ToList(), t.PaidCost, fades, attack, t.ActingPermanent?.Value);
    }

    /// <summary>"up to one target creature you control within 2", for card text.</summary>
    public static string SpecText(TargetSpec s)
    {
        var kind = s.Kind.ToString().ToLowerInvariant();
        var text = (s.Min == 0 ? "up to one " : "") + "target " + kind;
        if (s.Control == ControlFilter.You)
            text += " you control";
        else if (s.Control == ControlFilter.NotYou)
            text += " you don't control";
        if (s.WithinOfSource is { } d)
            text += $" within {d}";
        if (s.RelativeTo is { } r)
            text += $" within {s.WithinOfTarget ?? 0} of {r}";
        return text + $" ({s.Name})";
    }

    /// <summary>Plain-language text for an instruction; <paramref name="bound"/> resolves a
    /// target name to what it's bound to (null = keep the name).</summary>
    public static string InstructionText(Instruction i, Func<string, string?>? bound)
    {
        string T(string? name) => name switch
        {
            null => "it",
            "self" => bound?.Invoke("self") ?? "this",
            "here" => "its terrain",
            _ => bound?.Invoke(name) ?? name,
        };
        return i.Verb switch
        {
            "damage" => $"Deal {i.Amount} damage to {T(i.Target)}.",
            "heal" => $"Heal {T(i.Target)} by {i.Amount}.",
            "draw" => $"Draw {Math.Max(1, i.Amount)} card{(Math.Max(1, i.Amount) == 1 ? "" : "s")}.",
            "gainAp" => $"{T(i.Target)} gains {i.Amount} Activation Points.",
            "buff" => $"{T(i.Target)} gets {(i.Amount >= 0 ? "+" : "")}{i.Amount} {i.Stat}{(i.UntilEndOfTurn ? " until end of turn" : "")}.",
            "fell" => $"Fell {T(i.Target)}.",
            "destroy" => $"Destroy {T(i.Target)}.",
            "unbond" => $"Unbond {T(i.Target)}.",
            "bounce" => $"Return {T(i.Target)} to its controller's hand.",
            "flicker" => $"Flicker {T(i.Target)}.",
            "create" => $"Create a {i.Card} on {T(i.Target)}.",
            "raise" => $"Raise {T(i.Target)}.",
            _ => i.Verb,
        };
    }
}
