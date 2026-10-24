using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Perception;

/// <summary>
/// (TrueState, observer) → View (architecture.md §2.2). Redaction rules: hidden permanents (Root,
/// D12/D67) are left out; mana pools are shown only for the observer's own roots (D18); cards as
/// the observer believes them (D154); a trace whose acting permanent is hidden shows as a hidden
/// action (G17). The omniscient
/// projection is the debug UI's sanctioned true-state exception (architecture.md §2.9).
/// </summary>
public static class ViewProjector
{
    public static View Project(TrueState state, PlayerId? observer, bool omniscient = false, int logLines = 60)
    {
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

        // D155: every permanent is in every belief; where it is follows the observer's belief (D152).
        var permanents = state.Permanents
            .Where(p => p.Kind != PermanentKind.Terrain)
            .Select(p => ToView(state, p, observer, omniscient))
            .ToList();

        var players = state.Players.Select(ps =>
        {
            var owner = Owns(ps.Id);
            var champion = state.ChampionOf(ps.Id);
            List<CardView> Cards(List<ObjectId> section) =>
                section.Select((id, i) => Card(state, id, i + 1, observer, omniscient, owner)).ToList();
            return new MindView(
                ps.Id.ToString(),
                champion?.Id.Value,
                ps.Hand.Count,
                Cards(ps.Hand),
                ps.Library.Count,
                Cards(ps.Library).Where(c => c.Card is not null).ToList(),
                ps.Discard.Count,
                Cards(ps.Discard),
                owner && champion?.Pool is { } pool ? Pool(pool) : null);
        }).ToList();

        var pending = state.Pending.Select(id => Trace(state, state.Get<TraceObject>(id), observer, omniscient)).ToList();
        var past = state.Past.Select(id => state.Get<TraceObject>(id))
            .Where(t => omniscient || observer is not { } o || !t.FadedFor.Contains(o)) // D149: ended in this copy
            .Select(t => Trace(state, t, observer, omniscient)).ToList();
        var resolving = state.Resolving is { } r ? Trace(state, state.Get<TraceObject>(r), observer, omniscient) : null;

        DecisionView? decision = state.Decision switch
        {
            DamageSplitDecision d => new DecisionView("DamageSplit", d.Decider.ToString(),
                $"{(d.Defended ? "Defended" : "Undefended")}: split {d.Amount} damage", d.Candidates.Select(c => c.Value).ToList(), d.Amount),
            TopUpDecision t => new DecisionView("TopUp", t.Decider.ToString(),
                $"Top up {t.Ap} AP for {(t.Target.Object is not { } target ? t.Target.ToString() : state.NameOf(target))}, or it becomes illegal for this trace",
                [], t.Ap),
            _ => null,
        };

        var log = state.Log
            .Where(e => omniscient || e.About.All(id => !state.Exists(id) || state.CanSee(observer, id) && !Deceived(state, observer, id)))
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

    /// <summary>A log line names the true card and effect, so it is left out for a Champion who
    /// reads a face or a Pretend of the object it is about (D150).</summary>
    private static bool Deceived(TrueState state, PlayerId? observer, ObjectId id) =>
        observer is { } who && state.Find<GameObject>(id) switch
        {
            Permanent p => state.Beliefs.Values(who, p.Id).Face is not null,
            TraceObject t => Copies.ReadsPretend(t, who),
            _ => false,
        };

    public static string ControllerName(PlayerId? controller) => controller?.ToString() ?? "Neutral";

    private static CardView Card(TrueState state, ObjectId id, int position, PlayerId? observer, bool omniscient, bool owner)
    {
        if (omniscient || observer is not { } who)
            return new(id.Value, state.Get<CardObject>(id).Definition, position);
        var belief = state.Beliefs.Card(who, id);
        return new(owner ? id.Value : null, belief.Definition, belief.PositionKnown ? position : null);
    }

    private static IReadOnlyDictionary<string, int> Pool(ManaPool pool) =>
        pool.Amounts.ToDictionary(kv => kv.Key.ToString(), kv => kv.Value);

    private static PermanentView ToView(TrueState state, Permanent p, PlayerId? observer, bool omniscient)
    {
        // D149: the permanent as it is in the observer's copy — a Mimic's face, its believed values.
        var copy = omniscient ? null : observer;
        var def = state.DefIn(p, copy);
        var controller = state.Controller(p);
        var showPool = p.Pool is not null && (omniscient || (observer is not null && controller == observer));
        (HexCoord Hex, Slice Slice)? location = omniscient || observer is not { } who
            ? (state.PositionOf(p), p.Slice)
            : state.BelievedLocation(who, p);
        return new PermanentView(
            p.Id.Value,
            def.Id,
            p.Kind == PermanentKind.Remnant ? $"Remnant of {def.Name}" : def.Name,
            state.KindIn(p, copy).ToString(),
            ControllerName(controller),
            location?.Hex,
            location?.Slice.ToString(),
            p.Carrier?.Value,
            state.Attack(p, copy),
            p.Kind.IsActor() ? state.LifeIn(p, copy) : 0,
            state.MaxLife(p, copy),
            state.ApIn(p, copy),
            state.MaxAp(p, copy),
            state.Keywords(p, copy).Select(k => k.ToString()).ToList(),
            state.Abilities(p, copy).Select(AbilityOf).ToList(),
            p.Locked,
            p.UsedThisCycle.OrderBy(x => x, StringComparer.Ordinal).ToList(),
            p.Behavior?.ToString(),
            showPool ? Pool(p.Pool!) : null,
            omniscient ? Sight.IsInRoot(p) : p.Carrier is null && location?.Slice == Slice.Root,
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
        // D155: a trace's content is true for everyone and names every permanent (D153); a location
        // target shows only to its Champion and to those who observe that Slice.
        var controller = ControllerName(t.You);
        bool Observes(HexCoord hex, Slice? slice) => omniscient || observer == t.You || state.ObservesSlice(observer, hex, slice);

        string Name(ObjectId id) => omniscient ? state.NameOf(id) : state.Find<GameObject>(id) switch
        {
            CardObject c when observer is not { } who || state.Beliefs.Card(who, c.Id).Definition is null => $"a card in Champion {c.Owner}'s {c.Section}",
            Permanent p when p.Kind != PermanentKind.Remnant => $"{state.DefIn(p, observer).Name} {p.Id}",
            _ => state.NameOf(id),
        };

        // D150: the opponent of the trace's Champion reads its Pretend — what it seems to do, the
        // targets that names, how long it seems to last; its notes would tell the truth.
        var reader = omniscient ? null : observer;
        var pretended = reader is { } r0 && Copies.ReadsPretend(t, r0);
        var shownInstructions = Copies.InstructionsFor(t, reader);
        var shownTargets = reader is { } r1 && Copies.ReadsPretendText(t, r1)
            ? shownInstructions.SelectMany(i => new[] { i.Target, i.To }).OfType<string>().ToHashSet()
            : null;
        string Choice(TargetChoice c) =>
            c.Object is { } o ? Name(o) : c.Hex is { } h && !Observes(h, c.Slice) ? "a hidden location" : c.ToString();

        var locations = t.Targets.Values.SelectMany(l => l).ToList();
        if (t.Location is { } l0)
            locations.Add(l0);
        if (t.Attack is { } a0)
            locations.Add(TargetChoice.ForLocation(a0.Hex, a0.Slice));
        var allObserved = locations.All(c => c.Object is not null || c.Hex is not { } h || Observes(h, c.Slice));

        var targets = t.Targets
            .Where(kv => shownTargets is null || shownTargets.Contains(kv.Key))
            .SelectMany(kv => kv.Value.Select(c => $"{kv.Key}: {Choice(c)}"))
            .ToList();
        if (t.Location is { } loc)
            targets.Add($"location: {Choice(loc)}");

        AttackView? attack = null;
        if (t.Attack is { } a)
        {
            if (Observes(a.Hex, a.Slice))
                attack = new AttackView(a.Hex, a.Slice.ToString(), ControllerName(a.Entity), a.Defenders.Select(d => d.Value).ToList());
            else
                targets.Add($"attack: a hidden location, defenders: {string.Join(", ", a.Defenders.Select(Name).DefaultIfEmpty("none yet"))}");
        }

        var instructions = shownInstructions.Select(i => InstructionText(i, name =>
            t.Targets.TryGetValue(name, out var choices) && choices.Count > 0
                ? string.Join(" and ", choices.Select(Choice))
                : name == "self" && t.ActingPermanent is { } ap ? Name(ap) : null)).ToList();

        int? fades = t.Section == TraceSection.Past ? Copies.DurationFor(t, reader) - t.TurnEndsInPast : null;
        var text = pretended && t.PretendDefinition is { } face ? $"Cast {state.Content.Get(face).Name}" : t.Text;
        // Notes may say where something went, so they show only where every location does.
        return new TraceView(t.Id.Value, t.FromCard, text, controller, t.Physical, t.Speed.ToString(),
            targets, instructions, allObserved && !pretended ? t.Notes.ToList() : [], t.PaidCost, fades, attack, t.ActingPermanent?.Value);
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
        if (s.Section is { } section)
            text += $" in {section}";
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
            "quicken" => $"Quicken {T(i.Target)} by {i.Amount}.",
            "slow" => $"Slow {T(i.Target)} by {i.Amount}.",
            "power" => $"{T(i.Target)} gets {(i.Amount >= 0 ? "+" : "")}{i.Amount} Power.",
            "health" => $"{T(i.Target)} gets {(i.Amount >= 0 ? "+" : "")}{i.Amount} Health.",
            "ap" => $"{T(i.Target)} gets {(i.Amount >= 0 ? "+" : "")}{i.Amount} AP.",
            "fell" => $"Fell {T(i.Target)}.",
            "destroy" => $"Destroy {T(i.Target)}.",
            "unbond" => $"Unbond {T(i.Target)}.",
            "darken" => $"{T(i.Target)}'s {i.Section} becomes dark.",
            "put" => i.Section == "Hand" ? $"Return {T(i.Target)} to its controller's hand." : $"Flicker {T(i.Target)}.",
            "move" => $"Move {T(i.Target)} to {(i.To is null ? i.Section : T(i.To))}.",
            "discard" => $"Discard {T(i.Target)}.",
            "look at" => $"Look at {T(i.Target)}.",
            "reveal" => $"Reveal {T(i.Target)}.",
            "mill" => $"{(i.Target is null ? "Mill" : $"{T(i.Target)} mills")} {i.Amount}.",
            "shuffle" => $"Shuffle your {i.Section}.",
            "copy" => i.To is null ? $"Copy {T(i.Target)}." : $"Copy {T(i.Target)} onto {T(i.To)}.",
            "gain" => $"{T(i.Target)} gains {i.Ability}.",
            "lose" => $"{T(i.Target)} loses {i.Ability}.",
            "add mana" => $"Add {i.Amount} {i.Mana} mana.",
            "attach" => $"Attach {T(i.Target)} to {T(i.To)}.",
            "detach" => $"Detach {T(i.Target)}.",
            "gain control" => $"Gain control of {T(i.Target)}.",
            "create" => i.Card is null ? $"Create its card's permanent at {T(i.Target)}." : $"Create a {i.Card} on {T(i.Target)}.",
            "raise" => $"Raise {T(i.Target)}.",
            _ => i.Verb,
        };
    }
}
