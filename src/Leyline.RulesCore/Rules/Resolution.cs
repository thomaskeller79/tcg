using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// Resolving a trace (D33, D83, D85, D92; interaction-stack.md §Resolution). Instructions run in
/// card order, each checked just before it runs against what the trace still states and the
/// current state: a bound object must still exist (identity), be visible to the trace's Champion
/// (visibility), and still be something the verb can act on. Only the failing instruction
/// fizzles. After every instruction its immediate consequences happen (D112). When the trace is
/// done it crosses into Past (a physical trace instead ceases to exist, D45), and the triggers
/// it fired enter Pending.
/// </summary>
public static class Resolution
{
    /// <summary>Resolves the top of Pending. Returns false if the trace is suspended waiting for
    /// a decision (G16); the caller then waits for the decision command.</summary>
    public static bool ResolveTop(TrueState state)
    {
        var id = state.Pending[^1];
        state.Pending.RemoveAt(state.Pending.Count - 1);
        var trace = state.Get<TraceObject>(id);
        state.Resolving = id;
        state.InstructionIndex = 0;
        state.EventCounter = 0;
        state.Note($"Resolving: {trace.Text}.", id);
        return Continue(state, trace);
    }

    /// <summary>Resolves a trace taken from Pending: its D130 top-ups first, then its
    /// instructions. Returns false if it is suspended waiting for a decision.</summary>
    public static bool Continue(TrueState state, TraceObject trace)
    {
        if (TopUps.Ask(state, trace))
            return false;
        var suspended = trace.Ability?.Builtin is { } b && b != BuiltinAbility.None
            ? ResolveBuiltin(state, trace, b)
            : RunInstructions(state, trace);
        if (suspended)
            return false;
        Finish(state, trace);
        return true;
    }

    /// <summary>The trace's final step: it crosses Now into Past (so a trigger looking at Past
    /// sees it), then the collected triggers enter Pending.</summary>
    public static void Finish(TrueState state, TraceObject trace)
    {
        state.Resolving = null;
        if (trace.Physical || trace.Duration <= 0)
        {
            Creation.CeaseToExist(state, trace.Id, "");
        }
        else
        {
            trace.Section = TraceSection.Past;
            state.Past.Add(trace.Id);
        }
        Triggers.Flush(state);
    }

    // ----------------------------------------------------------------------------- built-in verbs

    private static bool ResolveBuiltin(TrueState state, TraceObject trace, BuiltinAbility builtin)
    {
        if (trace.IllegalTargets.Count > 0)
        {
            Fizzle(state, trace, "its target wasn't topped up"); // D130; a built-in has one target
            return false;
        }
        if (builtin == BuiltinAbility.Attack)
            return Combat.ResolveAttack(state, trace);
        if (builtin == BuiltinAbility.Defend)
        {
            Combat.ResolveDefend(state, trace);
            return false;
        }

        if (trace.ActingPermanent is not { } actingId || state.Find<Permanent>(actingId) is not { } p)
        {
            Fizzle(state, trace, "its permanent is gone");
            return false;
        }

        switch (builtin)
        {
            case BuiltinAbility.Move:
            case BuiltinAbility.Ascend:
            case BuiltinAbility.Descend:
            {
                var hex = trace.Location!.Hex!.Value;
                var slice = trace.Location.Slice!.Value;
                if (p.Carrier is not null || p.Hex.DistanceTo(hex) > 1)
                {
                    Fizzle(state, trace, "the destination is out of reach");
                    break;
                }
                if (!Entry.CanCreatureEnter(state, state.Controller(p), hex, slice, p.Id, trueState: true))
                {
                    Fizzle(state, trace, "it can't enter there any more");
                    break;
                }
                p.Hex = hex;
                p.Slice = slice;
                trace.Notes.Add($"moved to {hex} {slice}");
                state.Note($"{state.NameOf(p.Id)} moves to {hex} {slice}.", p.Id);
                Consequences.Run(state);
                break;
            }
            case BuiltinAbility.Equip:
            {
                var itemId = trace.Targets[Activation.TargetKey][0].Object!.Value;
                if (state.Find<Permanent>(itemId) is not { Kind: PermanentKind.Item, Carrier: null } item
                    || item.Hex != p.Hex || item.Slice != Island.LooseSliceFor(p.Slice) || p.Carrier is not null)
                {
                    Fizzle(state, trace, "the Item is no longer there");
                    break;
                }
                item.Carrier = p.Id;
                trace.Notes.Add($"equipped {state.NameOf(item.Id)}");
                state.Note($"{state.NameOf(p.Id)} equips {state.NameOf(item.Id)}.", p.Id, item.Id);
                Consequences.Run(state);
                break;
            }
            case BuiltinAbility.Unequip:
            {
                var itemId = trace.Targets[Activation.TargetKey][0].Object!.Value;
                if (state.Find<Permanent>(itemId) is not { } item || item.Carrier != p.Id)
                {
                    Fizzle(state, trace, "it no longer carries that Item");
                    break;
                }
                item.Carrier = null;
                item.Hex = state.PositionOf(p);
                item.Slice = Island.LooseSliceFor(p.Slice);
                state.Note($"{state.NameOf(p.Id)} drops {state.NameOf(item.Id)}.", p.Id, item.Id);
                Consequences.Run(state);
                break;
            }
            case BuiltinAbility.Bond:
            {
                var hex = trace.Location!.Hex!.Value;
                if (!Network.BondCandidates(state, p).Contains(hex))
                {
                    Fizzle(state, trace, "that terrain can't be bonded any more");
                    break;
                }
                Network.Bond(state, p, hex);
                trace.Notes.Add($"bonded {hex}");
                Consequences.Run(state);
                break;
            }
            case BuiltinAbility.Draw:
            {
                if (trace.You is { } you)
                    Mind.Draw(state, you, 1);
                Consequences.Run(state);
                break;
            }
            case BuiltinAbility.Collapse:
            {
                Network.Collapse(state, p);
                Consequences.Run(state);
                break;
            }
        }
        return false;
    }

    private static void Fizzle(TrueState state, TraceObject trace, string why)
    {
        trace.Notes.Add($"fizzled: {why}");
        state.Note($"{trace.Text} fizzles — {why}.", trace.Id);
    }

    // ----------------------------------------------------------------------------- printed instructions

    /// <summary>D149/D150: the true instructions run in the true copy and its Champion's; the
    /// opponent's copy runs the Pretend's instead, step by step alongside.</summary>
    private static bool RunInstructions(TrueState state, TraceObject trace)
    {
        var pretenders = Copies.Champions(state).Where(w => Copies.ReadsPretendText(trace, w)).ToList();
        var steps = Math.Max(trace.Instructions.Count, trace.PretendInstructions?.Count ?? 0);
        for (var i = 0; i < steps && !state.IsOver; i++)
        {
            state.InstructionIndex = i;
            if (i < trace.Instructions.Count)
                trace.Notes.Add(Run(state, trace, trace.Instructions[i]));
            foreach (var who in pretenders.Where(_ => i < trace.PretendInstructions!.Count))
                Copies.RunPretend(state, trace, who, trace.PretendInstructions![i]);
            Consequences.Run(state);
        }
        return false;
    }

    private static readonly HashSet<string> TargetlessVerbs = new(StringComparer.Ordinal) { "draw", "shuffle", "add mana" };

    /// <summary>Runs one instruction; returns the note shown on the trace.</summary>
    private static string Run(TrueState state, TraceObject trace, Instruction ins)
    {
        if (TargetlessVerbs.Contains(ins.Verb) || ins.Verb == "mill" && ins.Target is null)
            return RunTargetless(state, trace, ins);
        if (ins.Verb == "create" && ins.Card is null)
            return CreateFromCard(state, trace, ins);

        var objects = BoundObjects(state, trace, ins.Target);
        if (objects is null)
            return $"{ins.Verb}: fizzled (target gone, hidden or no longer related)";
        objects = objects.Where(o => VerbApplies(state, ins, o)).ToList();
        if (objects.Count == 0)
            return $"{ins.Verb}: does nothing";

        // The second selection: where a copy or a move goes, who an Item is attached to.
        Permanent? to = null;
        if (ins.To is not null)
        {
            if (BoundObjects(state, trace, ins.To) is not [Permanent destination])
                return $"{ins.Verb}: fizzled (destination gone, hidden or no longer related)";
            to = destination;
        }

        var permanents = objects.OfType<Permanent>().ToList();
        var names = string.Join(", ", objects.Select(o => state.NameOf(o.Id)));
        switch (ins.Verb)
        {
            case "damage":
                foreach (var o in permanents)
                    Copies.Damage(state, o, ins.Amount, _ => ins.Amount, [], trace);
                state.Note($"{trace.Text}: {ins.Amount} damage to {names}.", objects.Select(o => o.Id).ToArray());
                return $"dealt {ins.Amount} damage to {names}";
            case "heal":
                foreach (var o in permanents)
                    Copies.ChangeLife(state, o, Math.Max(0, Math.Min(state.MaxLife(o), o.CurrentLife + ins.Amount) - o.CurrentLife), trace);
                return $"healed {names} by {ins.Amount}";
            case "quicken":
                foreach (var o in permanents)
                    Copies.ChangeAp(state, o, ins.Amount, trace);
                return $"{names} gain {ins.Amount} AP";
            case "slow":
                foreach (var o in permanents)
                    Copies.ChangeAp(state, o, -ins.Amount, trace);
                return $"{names} lose {ins.Amount} AP";
            case "power":
            case "health":
            case "ap":
                foreach (var o in permanents)
                    Modify(state, trace, o, ins);
                return $"{names}: {Signed(ins.Amount)} {ins.Verb}";
            case "gain":
            case "lose":
                return ChangeAbility(state, trace, permanents, ins);
            case "fell":
                foreach (var o in permanents)
                    state.ToFall.Add(o.Id);
                return $"felled {names}";
            case "destroy":
                foreach (var o in objects)
                    Creation.CeaseToExist(state, o.Id, $"{state.NameOf(o.Id)} is destroyed.");
                return $"destroyed {names}";
            case "darken":
                // D152: the terrain's observation ability for that Slice is replaced while this
                // trace exists (Darkness).
                foreach (var o in permanents)
                    state.Modifiers.Add(new Modifier(state.NewModifierId(), o.Id, Stats.DarkStat(Enum.Parse<Slice>(ins.Section!)), 1, trace.Id));
                return $"{names}: {ins.Section} becomes dark";
            case "unbond":
                foreach (var o in permanents)
                    o.Parent = null;
                state.Note($"{names} unbond.", objects.Select(o => o.Id).ToArray());
                return $"unbonded {names}";
            case "put":
                foreach (var o in permanents)
                {
                    if (ins.Section == nameof(CardSection.Hand))
                        Bounce(state, o);
                    else
                        Flicker(state, o);
                }
                return $"put {names} into {ins.Section}";
            case "move":
                return objects[0] is CardObject ? MoveCards(state, objects.Cast<CardObject>(), ins) : MovePermanent(state, permanents[0], to!);
            case "discard":
                foreach (var c in objects.Cast<CardObject>())
                    Mind.Move(state, c, CardSection.Discard, named: true);
                state.Note($"{names} discarded.", objects.Select(o => o.Id).ToArray());
                return $"discarded {names}";
            case "mill":
                foreach (var champion in permanents)
                    Mill(state, state.Controller(champion)!.Value, ins.Amount);
                return $"milled {ins.Amount} from {names}";
            case "copy":
                return Copy(state, trace, objects[0], to);
            case "attach":
                foreach (var item in permanents)
                    item.Carrier = to!.Id;
                state.Note($"{names} attached to {state.NameOf(to!.Id)}.", objects.Select(o => o.Id).ToArray());
                return $"attached {names} to {state.NameOf(to!.Id)}";
            case "detach":
                foreach (var item in permanents)
                {
                    var carrier = state.Get<Permanent>(item.Carrier!.Value);
                    item.Hex = state.PositionOf(carrier);
                    item.Slice = Island.LooseSliceFor(carrier.Slice);
                    item.Carrier = null;
                }
                return $"detached {names}";
            case "gain control":
                if (trace.You is not { } you || state.ChampionOf(you) is not { } yours)
                    return "gain control: fizzled (no Champion)";
                foreach (var o in permanents)
                {
                    o.Parent = yours.Id;
                    o.Behavior = null;
                }
                state.Note($"Champion {you} gains control of {names}.", objects.Select(o => o.Id).ToArray());
                return $"Champion {you} gains control of {names}";
            case "create":
                return Create(state, trace, ins, permanents[0]);
            case "raise":
                return Raise(state, trace, permanents[0]);
            default:
                return $"{ins.Verb}: unknown verb";
        }
    }

    private static string RunTargetless(TrueState state, TraceObject trace, Instruction ins)
    {
        // "You draw", "you shuffle", "you add": the walk to a Mind or a pool only reaches a
        // Champion (D28, D74); a trace with no Champion behind it fizzles this instruction.
        if (trace.You is not { } you)
            return $"{ins.Verb}: fizzled (no Champion)";
        switch (ins.Verb)
        {
            case "draw":
                Mind.Draw(state, you, Math.Max(1, ins.Amount));
                return $"Champion {you} draws {Math.Max(1, ins.Amount)}";
            case "mill":
                Mill(state, you, ins.Amount);
                return $"Champion {you} mills {ins.Amount}";
            case "shuffle":
                var section = Enum.Parse<CardSection>(ins.Section!);
                Mind.ShuffleSection(state, you, section);
                state.Note($"Champion {you} shuffles their {section}.");
                return $"Champion {you} shuffles their {section}";
            case "add mana":
                state.ChampionOf(you)!.Pool!.Add(ins.Mana!.Value, ins.Amount);
                return $"Champion {you} adds {ins.Amount} {ins.Mana} mana";
            default:
                return $"{ins.Verb}: unknown verb";
        }
    }

    /// <summary>The objects a selection is bound to, after the identity, visibility and relation
    /// checks; null if the instruction fizzles.</summary>
    private static List<GameObject>? BoundObjects(TrueState state, TraceObject trace, string? target)
    {
        if (target is null or "self" or "here")
        {
            if (trace.ActingPermanent is not { } a || state.Find<Permanent>(a) is not { } self)
                return null;
            // "here": the terrain the acting permanent stands on (its position, D105).
            return target == "here" ? [state.TerrainOf(state.PositionOf(self))] : [self];
        }
        if (!trace.Targets.TryGetValue(target, out var choices))
            return null;
        if (choices.Count == 0)
            return [];

        var spec = trace.Specs.FirstOrDefault(s => s.Name == target);
        var result = new List<GameObject>();
        foreach (var choice in choices)
        {
            if (trace.IllegalTargets.Contains(choice))
                continue; // not topped up (D130)
            if (choice.Object is not { } id || state.Find<GameObject>(id) is not { } o)
                continue; // identity: the same instance must still exist
            if (o is Permanent p && !state.KnowsWhere(trace.You, p))
                continue; // visibility: the trace's Champion believes it where it truly is (D159)
            if (o is Permanent q && spec is not null && !Targeting.KindMatches(q.Kind, spec.Kind))
            {
                // D159: the true state decides; a face that made it look right is contradicted.
                if (trace.You is { } you && Targeting.KindMatches(state.KindIn(q, you), spec.Kind))
                    Copies.LargeReveal(state, you, [q.Id], null);
                continue;
            }
            if (spec is not null && !Targeting.RelationHolds(state, spec, id, trace.Targets))
                continue; // a relation still in the text (D83)
            result.Add(o);
        }
        return result.Count == 0 ? null : result;
    }

    /// <summary>Verb applicability (D85 condition 4).</summary>
    private static bool VerbApplies(TrueState state, Instruction ins, GameObject o) => o switch
    {
        CardObject c => ins.Verb switch
        {
            "discard" => c.Section == CardSection.Hand,
            "move" or "copy" or "destroy" => true,
            _ => false,
        },
        TraceObject => ins.Verb is "copy" or "destroy",
        Permanent p => ins.Verb switch
        {
            "damage" or "heal" or "fell" => p.Kind.IsActor(),
            "quicken" or "slow" or "health" or "ap" => p.Kind.IsActor(),
            "power" => p.Kind.IsCreatureType(),
            "gain" or "lose" => p.Kind != PermanentKind.Remnant,
            "put" => p.Kind is not (PermanentKind.Champion or PermanentKind.Terrain or PermanentKind.Remnant),
            "move" or "copy" => p.Kind.IsCreatureType(),
            "unbond" or "darken" => p.Kind == PermanentKind.Terrain,
            "create" => p.Kind == PermanentKind.Terrain,
            "raise" => p.Kind == PermanentKind.Remnant,
            "destroy" => p.Kind is not (PermanentKind.Champion or PermanentKind.Terrain),
            "mill" => p.Kind == PermanentKind.Champion,
            "attach" => p.Kind == PermanentKind.Item,
            "detach" => p.Kind == PermanentKind.Item && p.Carrier is not null,
            // Units only: an Item follows its carrier, a Structure its terrain's bonder (ancestry.md).
            "gain control" => p.Kind is PermanentKind.Creature or PermanentKind.Companion,
            _ => false,
        },
        _ => false,
    };

    /// <summary>D143/D144: ±Power, ±Health, ±AP — a modifier carried by this trace, ending when it
    /// ceases to exist. ±Health and ±AP change max and current together, as exact mirrors.</summary>
    private static void Modify(TrueState state, TraceObject trace, Permanent p, Instruction ins)
    {
        var stat = ins.Verb switch { "health" => Stats.MaxLifeStat, "ap" => Stats.MaxApStat, _ => Stats.AttackStat };
        state.Modifiers.Add(new Modifier(state.NewModifierId(), p.Id, stat, ins.Amount, trace.Id, Copies: Copies.TrueCopies(state, trace)));
        if (stat == Stats.MaxLifeStat)
            Copies.ChangeLife(state, p, ins.Amount, trace);
        else if (stat == Stats.MaxApStat)
            Copies.ChangeAp(state, p, ins.Amount, trace);
    }

    /// <summary>Gain or lose an ability: a modifier carried by this trace (D144). The ability is a
    /// keyword ("Flying", "Ranged 1-3") or a card's ability ("card/ability-id"); a lost one may
    /// also be named by its ability id alone ("move").</summary>
    private static string ChangeAbility(TrueState state, TraceObject trace, List<Permanent> permanents, Instruction ins)
    {
        var name = ins.Ability!;
        var gain = ins.Verb == "gain";
        var cardAbility = name.Split('/') is [var card, var id] ? state.Content.Get(card).Abilities.First(a => a.Id == id) : null;
        foreach (var p in permanents)
        {
            Keyword? keyword = null;
            var ability = cardAbility;
            if (ability is null && (gain || state.Keywords(p).Any(k => k.Name == Keyword.Parse(name).Name)))
                keyword = Keyword.Parse(name);
            else
                ability ??= state.Abilities(p).FirstOrDefault(a => a.Id == name) ?? new AbilityDefinition { Id = name, Name = name };
            state.Modifiers.Add(new Modifier(state.NewModifierId(), p.Id, Stats.AbilityStat, gain ? 1 : -1, trace.Id, keyword, ability, Copies: Copies.TrueCopies(state, trace)));
        }
        return $"{string.Join(", ", permanents.Select(p => state.NameOf(p.Id)))} {ins.Verb} {cardAbility?.Name ?? name}";
    }

    /// <summary>D138: a move within a Mind — the same card, a new section of its owner's Mind, last in it.</summary>
    private static string MoveCards(TrueState state, IEnumerable<CardObject> cards, Instruction ins)
    {
        var to = Enum.Parse<CardSection>(ins.Section!);
        var names = new List<string>();
        foreach (var c in cards)
        {
            Mind.Move(state, c, to, named: true);
            names.Add(state.NameOf(c.Id));
        }
        return $"moved {string.Join(", ", names)} to {to}";
    }

    /// <summary>A move on the Island: the same permanent on a new terrain, in its own Slice if its
    /// card allows it, else its card's Slice; the ordinary entry check applies.</summary>
    private static string MovePermanent(TrueState state, Permanent p, Permanent terrain)
    {
        var expected = Activation.ExpectedSlice(state.Def(p));
        var slice = p.Slice == expected ? p.Slice : expected;
        if (!Entry.CanCreatureEnter(state, state.Controller(p), terrain.Hex, slice, p.Id, trueState: true))
            return "move: fizzled (no room)";
        p.Hex = terrain.Hex;
        p.Slice = slice;
        state.Note($"{state.NameOf(p.Id)} moves to {terrain.Hex} {slice}.", p.Id);
        return $"moved {state.NameOf(p.Id)} to {terrain.Hex} {slice}";
    }

    /// <summary>The top <paramref name="count"/> cards of a Library go to its Discard.</summary>
    private static void Mill(TrueState state, PlayerId player, int count)
    {
        var library = state.Player(player).Library;
        for (var i = 0; i < count && library.Count > 0; i++)
            Mind.Move(state, state.Get<CardObject>(library[0]), CardSection.Discard);
        state.Note($"Champion {player} mills {count}.");
    }

    /// <summary>Copy: a new object, the original stays (effect-form.md §Instructions). A creature's
    /// copy enters on the destination terrain like a creation (its parent and Behavior from this
    /// trace, D106); a card's goes last into your Hand; a trace's goes on top of Pending, and
    /// this trace's payer and Champion are its own.</summary>
    private static string Copy(TrueState state, TraceObject trace, GameObject original, Permanent? to)
    {
        switch (original)
        {
            case Permanent p:
            {
                var def = state.Def(p);
                var slice = Activation.ExpectedSlice(def);
                var controller = trace.Parent is { } parent && state.Exists(parent) ? state.Controller(parent) : null;
                if (!Entry.CanPermanentEnter(state, def, controller, to!.Hex, slice, trueState: true))
                    return "copy: fizzled (no room)";
                var copy = Creation.CreatePermanent(state, def, to.Hex, slice, trace.Parent, trace.Behavior, p.Id,
                    attackDelta: p.AttackDelta, maxLifeDelta: p.MaxLifeDelta);
                return $"copied {state.NameOf(p.Id)} as {state.NameOf(copy.Id)}";
            }
            case CardObject c:
            {
                if (trace.You is not { } you)
                    return "copy: fizzled (no Champion)";
                var copy = Creation.CreateCard(state, c.Definition, you, CardSection.Hand, c.Id, c.AttackDelta, c.MaxLifeDelta);
                return $"copied {state.NameOf(c.Id)} into Champion {you}'s Hand as {state.NameOf(copy.Id)}";
            }
            case TraceObject t:
            {
                var copy = new TraceObject
                {
                    Id = state.NewId(), Timestamp = state.NewTimestamp(), Source = t.Id,
                    Definition = t.Definition, Ability = t.Ability, ActingPermanent = t.ActingPermanent,
                    Parent = trace.Parent, Behavior = trace.Behavior, You = trace.You,
                    Physical = t.Physical, Duration = t.Duration, Speed = t.Speed,
                    Specs = t.Specs, Instructions = t.Instructions, Attack = t.Attack, Location = t.Location,
                    AttackDelta = t.AttackDelta, MaxLifeDelta = t.MaxLifeDelta,
                    Text = $"Copy of {t.Text}",
                };
                foreach (var (name, choices) in t.Targets)
                    copy.Targets[name] = choices.ToList();
                copy.IllegalTargets.UnionWith(t.IllegalTargets);
                Creation.CreateTrace(state, copy);
                return $"copied {state.NameOf(t.Id)} onto Pending";
            }
            default:
                return "copy: does nothing";
        }
    }

    private static string Signed(int amount) => amount >= 0 ? $"+{amount}" : $"{amount}";

    /// <summary>D69/D74 Bounce: Matter → Mind. A new Card in its controller's hand, read from the
    /// permanent's current state (max-tier changes survive, current values don't).</summary>
    private static void Bounce(TrueState state, Permanent p)
    {
        if (state.Controller(p) is not { } owner)
        {
            state.Note($"{state.NameOf(p.Id)} has no controller's hand to return to.", p.Id);
            return;
        }
        var name = state.NameOf(p.Id);
        Creation.CreateCard(state, p.Definition, owner, CardSection.Hand, p.Id, p.AttackDelta, p.MaxLifeDelta);
        Creation.CeaseToExist(state, p.Id, $"{name} returns to Champion {owner}'s hand.");
    }

    /// <summary>D69/D105/D142 Flicker: Matter → Aether. A trace creating its card's permanent, like
    /// a cast's but with the permanent as its source, targeting the permanent's position (Slice: its own if the card's Slice filter allows it,
    /// else the filter's).</summary>
    private static void Flicker(TrueState state, Permanent p)
    {
        var def = state.Def(p);
        var expected = Activation.ExpectedSlice(def);
        var slice = p.Slice == expected ? p.Slice : expected;
        var trace = new TraceObject
        {
            Id = state.NewId(),
            Timestamp = state.NewTimestamp(),
            Source = p.Id,
            Definition = p.Definition,
            Instructions = [Activation.CreateFromCard],
            Parent = p.Kind is PermanentKind.Creature or PermanentKind.Companion ? p.Parent : null,
            Behavior = p.Behavior,
            You = state.Controller(p),
            Speed = def.Speed,
            Duration = DefaultAbilities.Duration,
            Text = $"Flicker of {def.Name}",
            AttackDelta = p.AttackDelta,
            MaxLifeDelta = p.MaxLifeDelta,
        };
        trace.Targets[Activation.TargetKey] = [TargetChoice.ForLocation(state.PositionOf(p), slice)];
        var name = state.NameOf(p.Id);
        Creation.CeaseToExist(state, p.Id, $"{name} flickers into the Aether.");
        Creation.CreateTrace(state, trace);
    }

    /// <summary>"Create a [card] on target terrain" (D88: every created object references a real
    /// card). Parent and Behavior copy from the creating trace (D106).</summary>
    private static string Create(TrueState state, TraceObject trace, Instruction ins, Permanent terrain)
    {
        if (ins.Card is null || !state.Content.Contains(ins.Card))
            return "create: unknown card";
        var def = state.Content.Get(ins.Card);
        var slice = Activation.ExpectedSlice(def);
        var controller = trace.Parent is { } parent && state.Exists(parent) ? state.Controller(parent) : null;
        if (!Entry.CanPermanentEnter(state, def, controller, terrain.Hex, slice, trueState: true))
            return $"create {def.Name}: fizzled (no room)";
        var p = Creation.CreatePermanent(state, def, terrain.Hex, slice, trace.Parent, trace.Behavior, trace.Id);
        return $"created {state.NameOf(p.Id)}";
    }

    /// <summary>D142: a permanent card's Cast ability (and Flicker's trace) — create the permanent
    /// the trace's card defines, from the card's current values, at its target location (D105).</summary>
    private static string CreateFromCard(TrueState state, TraceObject trace, Instruction ins)
    {
        var def = state.Content.Get(trace.Definition!);
        var loc = trace.Targets[ins.Target!][0];
        var hex = loc.Hex!.Value;
        var slice = loc.Slice ?? Activation.ExpectedSlice(def);
        var controller = trace.Parent is { } parent && state.Exists(parent) ? state.Controller(parent) : null;
        if (!Entry.CanPermanentEnter(state, def, controller, hex, slice, trueState: true))
        {
            state.Note($"{trace.Text} fizzles — the location is no longer legal.", trace.Id);
            return "fizzled: the location is no longer legal";
        }
        var p = Creation.CreatePermanent(state, def, hex, slice, trace.Parent, trace.Behavior, trace.Id,
            attackDelta: trace.AttackDelta, maxLifeDelta: trace.MaxLifeDelta);
        Copies.EnterWithFace(state, p, trace);
        return $"created {state.NameOf(p.Id)}";
    }

    /// <summary>D110 raise: the Remnant ceases to exist, a permanent of its card is created at
    /// its location by the ordinary creation rules.</summary>
    private static string Raise(TrueState state, TraceObject trace, Permanent remnant)
    {
        var def = state.Def(remnant);
        var expected = Activation.ExpectedSlice(def);
        var slice = remnant.Slice == expected ? remnant.Slice : expected;
        var controller = trace.Parent is { } parent && state.Exists(parent) ? state.Controller(parent) : null;
        if (!Entry.CanPermanentEnter(state, def, controller, remnant.Hex, slice, trueState: true))
            return "raise: fizzled (no room)";
        var p = Creation.CreatePermanent(state, def, remnant.Hex, slice, trace.Parent, trace.Behavior, remnant.Id);
        Creation.CeaseToExist(state, remnant.Id, "");
        return $"raised {state.NameOf(p.Id)}";
    }
}
