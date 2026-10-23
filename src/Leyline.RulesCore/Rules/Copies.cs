using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// The three copies (asymmetric-information.md §The core model, D149–D151). The true copy is the
/// state itself; each Champion's copy is the true one plus its beliefs. A trace with a Pretend is
/// read by the opponent of its Champion as the Pretend (D150): the true copy and its Champion's run
/// the true instructions, the opponent's copy the pretended ones. After each instruction every
/// copy's prediction is compared with what that Champion sees; a difference is a contradiction,
/// and the large reveal makes the values read and their causes true for it (D151).
/// </summary>
public static class Copies
{
    public static IEnumerable<PlayerId> Champions(TrueState state) => state.Players.Select(p => p.Id);

    /// <summary>Does this Champion read the trace's Pretend (its name, face, Duration)?</summary>
    public static bool ReadsPretend(TraceObject t, PlayerId who) =>
        (t.PretendDefinition is not null || t.PretendInstructions is not null || t.PretendDuration is not null)
        && t.You is { } you && who != you && !t.RevealedTo.Contains(who);

    /// <summary>Does its copy run the pretended instructions instead of the true ones?</summary>
    public static bool ReadsPretendText(TraceObject t, PlayerId who) => t.PretendInstructions is not null && ReadsPretend(t, who);

    public static IReadOnlyList<Instruction> InstructionsFor(TraceObject t, PlayerId? who) =>
        who is { } w && ReadsPretendText(t, w) ? t.PretendInstructions! : t.Instructions;

    public static string? DefinitionFor(TraceObject t, PlayerId? who) =>
        who is { } w && ReadsPretend(t, w) ? t.PretendDefinition ?? t.Definition : t.Definition;

    public static int DurationFor(TraceObject t, PlayerId? who) =>
        who is { } w && ReadsPretend(t, w) ? t.PretendDuration ?? t.Duration : t.Duration;

    /// <summary>The Champions whose copies run the trace's true instructions; null: every one.</summary>
    public static IReadOnlyList<PlayerId>? TrueCopies(TrueState state, TraceObject? t) =>
        t is null || !Champions(state).Any(w => ReadsPretendText(t, w)) ? null : Champions(state).Where(w => !ReadsPretendText(t, w)).ToList();

    // ----------------------------------------------------------------------------- applying

    /// <summary>A change to current Life by the trace's true text: a copy reading its Pretend
    /// doesn't see it (its offset absorbs it, and the trace is the cause).</summary>
    public static void ChangeLife(TrueState state, Permanent p, int delta, TraceObject? trace)
    {
        p.CurrentLife += delta;
        foreach (var who in Champions(state).Where(w => trace is not null && ReadsPretendText(trace, w)))
            Offset(state, who, p, trace!, life: -delta);
    }

    public static void ChangeAp(TrueState state, Permanent p, int delta, TraceObject? trace)
    {
        var before = p.CurrentAp;
        p.CurrentAp = Math.Max(0, p.CurrentAp + delta);
        foreach (var who in Champions(state).Where(w => trace is not null && ReadsPretendText(trace, w)))
            Offset(state, who, p, trace!, ap: before - p.CurrentAp);
    }

    private static void Offset(TrueState state, PlayerId who, Permanent p, TraceObject cause, int life = 0, int ap = 0)
    {
        var v = state.Beliefs.Values(who, p.Id);
        v.LifeOffset += life;
        v.ApOffset += ap;
        v.Causes.Add(cause.Id);
    }

    /// <summary>D149/D151 damage: the true copy takes the true amount, so does the copy of the
    /// target's controller (D155); any other copy takes the amount it predicts from its own beliefs
    /// (<paramref name="predicted"/>), and a copy reading the trace's Pretend doesn't see it. Each
    /// prediction is compared once the instruction's consequences are done.</summary>
    public static void Damage(TrueState state, Permanent target, int amount, Func<PlayerId, int> predicted, IReadOnlyList<ObjectId> reads, TraceObject? trace)
    {
        foreach (var who in Champions(state))
        {
            var v = state.Beliefs.Values(who, target.Id);
            if (trace is not null && ReadsPretendText(trace, who))
            {
                v.LifeOffset += amount;
                v.Causes.Add(trace.Id);
                continue;
            }
            var before = state.LifeIn(target, who);
            var guess = predicted(who);
            if (state.Controller(target) != who)
                v.LifeOffset += amount - guess;
            state.Beliefs.Checks.Add(new BeliefCheck(who, target.Id, before - guess, reads, trace?.Id));
        }
        target.CurrentLife -= amount;
    }

    /// <summary>Runs one pretended instruction in one Champion's copy only (D150). Covers the
    /// instructions a Pretend in the prototype needs: the stat changes, damage and heal, gaining
    /// and losing an ability, and a move (a false location, the decoy); others change nothing.</summary>
    public static string RunPretend(TrueState state, TraceObject trace, PlayerId who, Instruction ins)
    {
        var objects = (ins.Target is { } name && trace.Targets.TryGetValue(name, out var choices) ? choices : [])
            .Select(c => c.Object is { } id ? state.Find<Permanent>(id) : null)
            .OfType<Permanent>()
            .ToList();
        foreach (var p in objects)
        {
            var v = state.Beliefs.Values(who, p.Id);
            v.Causes.Add(trace.Id);
            switch (ins.Verb)
            {
                case "damage":
                    v.LifeOffset -= ins.Amount;
                    break;
                case "heal":
                    v.LifeOffset += Math.Max(0, Math.Min(ins.Amount, state.MaxLife(p, who) - state.LifeIn(p, who)));
                    break;
                case "quicken":
                    v.ApOffset += ins.Amount;
                    break;
                case "slow":
                    v.ApOffset -= Math.Min(ins.Amount, state.ApIn(p, who));
                    break;
                case "power":
                case "health":
                case "ap":
                    var stat = ins.Verb switch { "health" => Stats.MaxLifeStat, "ap" => Stats.MaxApStat, _ => Stats.AttackStat };
                    state.Modifiers.Add(new Modifier(state.NewModifierId(), p.Id, stat, ins.Amount, trace.Id, InTrue: false, Copies: [who]));
                    if (stat == Stats.MaxLifeStat)
                        v.LifeOffset += ins.Amount;
                    else if (stat == Stats.MaxApStat)
                        v.ApOffset += ins.Amount;
                    break;
                case "gain":
                case "lose":
                    var keyword = ins.Ability!.Contains('/') ? null : Keyword.Parse(ins.Ability);
                    var ability = keyword is null ? state.Content.Get(ins.Ability.Split('/')[0]).Abilities.First(a => a.Id == ins.Ability.Split('/')[1]) : null;
                    state.Modifiers.Add(new Modifier(state.NewModifierId(), p.Id, Stats.AbilityStat, ins.Verb == "gain" ? 1 : -1, trace.Id, keyword, ability, InTrue: false, Copies: [who]));
                    break;
                case "move" when ins.To is { } to && trace.Targets.TryGetValue(to, out var dest) && dest.FirstOrDefault()?.Object is { } terrainId
                                 && state.Find<Permanent>(terrainId) is { } terrain:
                    var location = state.Beliefs.Location(who, p.Id);
                    location.Hex = terrain.Hex;
                    location.Slice = Activation.ExpectedSlice(state.DefIn(p, who));
                    location.Observed = false;
                    break;
            }
        }
        return $"(pretended to Champion {who}: {ins.Verb})";
    }

    /// <summary>A Mimic enters: the opponent's copy reads its face (D150). Its believed current
    /// Life and AP are the face's; the cast trace and its card are the cause.</summary>
    public static void EnterWithFace(TrueState state, Permanent p, TraceObject trace)
    {
        foreach (var who in Champions(state).Where(w => ReadsPretend(trace, w) && trace.PretendDefinition is not null))
        {
            var v = state.Beliefs.Values(who, p.Id);
            v.Face = trace.PretendDefinition;
            v.LifeOffset = state.MaxLife(p, who) - p.CurrentLife;
            v.ApOffset = (state.DefIn(p, who).HasKeyword(Keyword.Haste) ? state.MaxAp(p, who) : 0) - p.CurrentAp;
            v.Causes.Add(trace.Id);
            if (trace.Source is { } card)
                v.Causes.Add(card);
            // A face that couldn't have entered there is contradicted at once (a Flying Dragon in
            // the Sky as a Goblin).
            if (state.CanSee(who, p) && Activation.ExpectedSlice(state.DefIn(p, who)) != p.Slice)
                LargeReveal(state, who, [p.Id], null);
        }
    }

    /// <summary>Values set outright in every copy: a refresh to max AP, an exhaust to 0, the
    /// end-of-turn cap of current Life at max (D149: the same function in every copy).</summary>
    public static void SetApInEveryCopy(TrueState state, Permanent p, Func<PlayerId?, int> value)
    {
        p.CurrentAp = value(null);
        foreach (var who in Champions(state))
            state.Beliefs.Values(who, p.Id).ApOffset = value(who) - p.CurrentAp;
    }

    public static void CapLifeInEveryCopy(TrueState state, Permanent p)
    {
        var believed = Champions(state).ToDictionary(w => w, w => Math.Min(state.LifeIn(p, w), state.MaxLife(p, w)));
        p.CurrentLife = Math.Min(p.CurrentLife, state.MaxLife(p));
        foreach (var (who, life) in believed)
            state.Beliefs.Values(who, p.Id).LifeOffset = life - p.CurrentLife;
    }

    /// <summary>A modifier ends (its trace ceased to exist, or its Duration ran out in one copy):
    /// ±Health and ±AP take back from current what they gave, in each copy they were in (D143).</summary>
    public static void EndMirror(TrueState state, Modifier m, PlayerId? onlyIn = null)
    {
        if (m.Stat is not (Stats.MaxLifeStat or Stats.MaxApStat) || state.Find<Permanent>(m.Subject) is not { } p)
            return;
        var life = m.Stat == Stats.MaxLifeStat;
        var trueChange = onlyIn is null && m.InTrue ? -m.Delta : 0;
        if (life)
            p.CurrentLife += trueChange;
        else
            p.CurrentAp = Math.Max(0, p.CurrentAp + trueChange);
        foreach (var who in Champions(state))
        {
            var change = (onlyIn is null || onlyIn == who) && m.In(who) ? -m.Delta : 0;
            var v = state.Beliefs.Values(who, p.Id);
            if (life)
                v.LifeOffset += change - trueChange;
            else
                v.ApOffset += change - trueChange;
        }
    }

    /// <summary>D149 Durations per copy: the trace's Duration ran out in one Champion's copy while
    /// it lasts in the true one (Mirage 3) — its modifiers leave that copy.</summary>
    public static void FadeIn(TrueState state, TraceObject t, PlayerId who)
    {
        t.FadedFor.Add(who);
        foreach (var m in state.Modifiers.Where(m => m.Vessel == t.Id && m.In(who)).ToList())
        {
            EndMirror(state, m, who);
            SetIn(state, m, who, false);
        }
    }

    private static void SetIn(TrueState state, Modifier m, PlayerId who, bool inCopy)
    {
        var copies = (m.Copies ?? Champions(state).ToList()).Where(w => w != who).ToList();
        if (inCopy)
            copies.Add(who);
        var index = state.Modifiers.IndexOf(m);
        if (!m.InTrue && copies.Count == 0)
            state.Modifiers.RemoveAt(index);
        else
            state.Modifiers[index] = m with { Copies = copies.Count == Champions(state).Count() ? null : copies };
    }

    // ----------------------------------------------------------------------------- comparing

    /// <summary>D151, after an instruction and its consequences: each copy's prediction against
    /// what its Champion sees — a fall, and its own permanents' Life — and every creature a copy
    /// believes at 0 Life or less that still lives. A difference is a contradiction.</summary>
    public static void Compare(TrueState state)
    {
        var checks = state.Beliefs.Checks.ToList();
        state.Beliefs.Checks.Clear();
        foreach (var c in checks)
        {
            var target = state.Find<Permanent>(c.Target);
            var fell = target is null;
            var mismatch = fell != c.PredictedLife <= 0
                           || target is not null && state.Controller(target) == c.Who && c.PredictedLife > 0 && c.PredictedLife != state.LifeIn(target, c.Who);
            if (mismatch)
                LargeReveal(state, c.Who, [.. c.Reads, c.Target], c.Trace);
        }
        foreach (var who in Champions(state))
        {
            foreach (var p in state.Permanents.Where(p => p.Kind.IsActor() && state.LifeIn(p, who) <= 0).ToList())
                LargeReveal(state, who, [p.Id], null);
        }
    }

    /// <summary>D151 large reveal for one Champion: every value read becomes true for it, and so
    /// does what caused a false value — the traces and their cards, the Pretend modifiers. A
    /// contradiction never fills in an unknown: locations are left alone.</summary>
    public static void LargeReveal(TrueState state, PlayerId who, IReadOnlyList<ObjectId> reads, ObjectId? trace)
    {
        if (trace is { } t)
            Reveal(state, who, t);
        foreach (var id in reads)
        {
            if (state.Find<Permanent>(id) is not { } p)
                continue;
            var v = state.Beliefs.Values(who, id);
            var wasFalse = v.Face is not null || v.LifeOffset != 0 || v.ApOffset != 0 || v.Causes.Count > 0
                           || state.Modifiers.Any(m => m.Subject == id && m.In(who) != m.InTrue);
            if (!wasFalse)
                continue;
            foreach (var cause in v.Causes.ToList())
                Reveal(state, who, cause);
            foreach (var m in state.Modifiers.Where(m => m.Subject == id && m.In(who) != m.InTrue).ToList())
            {
                Reveal(state, who, m.Vessel);
                if (state.Modifiers.Contains(m))
                    SetIn(state, m, who, m.InTrue);
            }
            v.Causes.Clear();
            v.Face = null;
            v.LifeOffset = 0;
            v.ApOffset = 0;
            state.Note($"Contradiction: Champion {who} learns the truth about {state.NameOf(id)}.", id);
        }
    }

    /// <summary>A trace or card behind a false value becomes true for the Champion: it reads the
    /// trace's true text from now on (which also corrects later false beliefs from it, D151), and
    /// the card the trace came from.</summary>
    private static void Reveal(TrueState state, PlayerId who, ObjectId id)
    {
        switch (state.Find<GameObject>(id))
        {
            case TraceObject t when ReadsPretend(t, who):
                t.RevealedTo.Add(who);
                t.FadedFor.Remove(who);
                foreach (var m in state.Modifiers.Where(m => m.Vessel == t.Id && m.In(who) != m.InTrue).ToList())
                    SetIn(state, m, who, m.InTrue);
                if (t.Source is { } source)
                    Reveal(state, who, source);
                state.Note($"Champion {who} sees through {t.Text}.", t.Id);
                break;
            case CardObject c:
                var belief = state.Beliefs.Card(who, c.Id);
                belief.Definition = c.Definition;
                break;
        }
    }
}
