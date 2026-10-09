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
        var suspended = trace.Kind switch
        {
            TraceKind.Permanent => ResolvePermanentTrace(state, trace),
            _ when trace.Ability?.Builtin is { } b && b != BuiltinAbility.None => ResolveBuiltin(state, trace, b),
            _ => RunInstructions(state, trace),
        };
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
            trace.Zone = Zone.Past;
            trace.RoundResolved = state.Round;
            state.Past.Add(trace.Id);
        }
        Triggers.Flush(state);
    }

    private static bool ResolvePermanentTrace(TrueState state, TraceObject trace)
    {
        var def = state.Content.Get(trace.Definition!);
        var loc = trace.Location!;
        var hex = loc.Hex!.Value;
        var slice = loc.Slice ?? Activation.ExpectedSlice(def);
        var controller = trace.Parent is { } parent && state.Exists(parent) ? state.Controller(parent) : null;
        if (!Entry.CanPermanentEnter(state, def, controller, hex, slice, trueState: true))
        {
            trace.Notes.Add("fizzled: the location is no longer legal");
            state.Note($"{trace.Text} fizzles — the location is no longer legal.", trace.Id);
            return false;
        }
        var p = Creation.CreatePermanent(state, def, hex, slice, trace.Parent, trace.Behavior, trace.Id,
            attackDelta: trace.AttackDelta, maxLifeDelta: trace.MaxLifeDelta);
        trace.Notes.Add($"created {state.NameOf(p.Id)}");
        Consequences.Run(state);
        return false;
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

    private static bool RunInstructions(TrueState state, TraceObject trace)
    {
        for (var i = 0; i < trace.Instructions.Count && !state.IsOver; i++)
        {
            state.InstructionIndex = i;
            var instruction = trace.Instructions[i];
            var note = Run(state, trace, instruction);
            trace.Notes.Add(note);
            Consequences.Run(state);
        }
        return false;
    }

    private static readonly HashSet<string> TargetlessVerbs = new(StringComparer.Ordinal) { "draw" };

    /// <summary>Runs one instruction; returns the note shown on the trace.</summary>
    private static string Run(TrueState state, TraceObject trace, Instruction ins)
    {
        if (TargetlessVerbs.Contains(ins.Verb))
            return RunTargetless(state, trace, ins);

        var objects = BoundObjects(state, trace, ins);
        if (objects is null)
            return $"{ins.Verb}: fizzled (target gone, hidden or no longer related)";
        objects = objects.Where(o => VerbApplies(state, ins, o)).ToList();
        if (objects.Count == 0)
            return $"{ins.Verb}: does nothing";

        var names = string.Join(", ", objects.Select(o => state.NameOf(o.Id)));
        switch (ins.Verb)
        {
            case "damage":
                foreach (var o in objects)
                    o.CurrentLife -= ins.Amount;
                state.Note($"{trace.Text}: {ins.Amount} damage to {names}.", objects.Select(o => o.Id).ToArray());
                return $"dealt {ins.Amount} damage to {names}";
            case "heal":
                foreach (var o in objects)
                    o.CurrentLife = Math.Min(state.MaxLife(o), o.CurrentLife + ins.Amount);
                return $"healed {names} by {ins.Amount}";
            case "gainAp":
                foreach (var o in objects)
                    o.CurrentAp += ins.Amount;
                return $"{names} gain {ins.Amount} AP";
            case "buff":
                foreach (var o in objects)
                    Buff(state, o, ins);
                return $"{names}: {(ins.Amount >= 0 ? "+" : "")}{ins.Amount} {ins.Stat}{(ins.UntilEndOfTurn ? " until end of turn" : "")}";
            case "fell":
                foreach (var o in objects)
                    state.ToFall.Add(o.Id);
                return $"felled {names}";
            case "destroy":
                foreach (var o in objects)
                    Creation.CeaseToExist(state, o.Id, $"{state.NameOf(o.Id)} is destroyed.");
                return $"destroyed {names}";
            case "unbond":
                foreach (var o in objects)
                    o.Parent = null;
                state.Note($"{names} unbond.", objects.Select(o => o.Id).ToArray());
                return $"unbonded {names}";
            case "bounce":
                foreach (var o in objects)
                    Bounce(state, o);
                return $"returned {names} to a hand";
            case "flicker":
                foreach (var o in objects)
                    Flicker(state, o);
                return $"flickered {names}";
            case "create":
                return Create(state, trace, ins, objects[0]);
            case "raise":
                return Raise(state, trace, objects[0]);
            default:
                return $"{ins.Verb}: unknown verb";
        }
    }

    private static string RunTargetless(TrueState state, TraceObject trace, Instruction ins)
    {
        switch (ins.Verb)
        {
            case "draw":
                // "You draw": the walk to a Hand only reaches a Champion (D28, D74); a trace with
                // no Champion behind it fizzles this instruction.
                if (trace.You is not { } you)
                    return "draw: fizzled (no Champion)";
                Mind.Draw(state, you, Math.Max(1, ins.Amount));
                return $"Champion {you} draws {Math.Max(1, ins.Amount)}";
            default:
                return $"{ins.Verb}: unknown verb";
        }
    }

    /// <summary>The objects an instruction acts on, after the identity, visibility and relation
    /// checks; null if the instruction fizzles.</summary>
    private static List<Permanent>? BoundObjects(TrueState state, TraceObject trace, Instruction ins)
    {
        if (ins.Target is null or "self" or "here")
        {
            if (trace.ActingPermanent is not { } a || state.Find<Permanent>(a) is not { } self)
                return null;
            // "here": the terrain the acting permanent stands on (its position, D105).
            return ins.Target == "here" ? [state.TerrainOf(state.PositionOf(self))] : [self];
        }
        if (!trace.Targets.TryGetValue(ins.Target, out var choices))
            return null;
        if (choices.Count == 0)
            return [];

        var spec = trace.Specs.FirstOrDefault(s => s.Name == ins.Target);
        var result = new List<Permanent>();
        foreach (var choice in choices)
        {
            if (trace.IllegalTargets.Contains(choice))
                continue; // not topped up (D130)
            if (choice.Object is not { } id || state.Find<Permanent>(id) is not { } p)
                continue; // identity: the same instance must still exist
            if (!state.CanSee(trace.You, p))
                continue; // visibility, in the live view of the trace's Champion
            if (spec is not null && !Targeting.RelationHolds(state, spec, id, trace.Targets))
                continue; // a relation still in the text (D83)
            result.Add(p);
        }
        return result.Count == 0 ? null : result;
    }

    /// <summary>Verb applicability (D85 condition 4).</summary>
    private static bool VerbApplies(TrueState state, Instruction ins, Permanent p) => ins.Verb switch
    {
        "damage" or "heal" or "fell" => p.Kind.IsActor(),
        "gainAp" => p.Kind.IsActor(),
        "buff" => ins.Stat == "Life" ? p.Kind.IsActor() : p.Kind.IsCreatureType(),
        "bounce" or "flicker" => p.Kind is not (PermanentKind.Champion or PermanentKind.Terrain or PermanentKind.Remnant),
        "unbond" => p.Kind == PermanentKind.Terrain,
        "create" => p.Kind == PermanentKind.Terrain,
        "raise" => p.Kind == PermanentKind.Remnant,
        "destroy" => p.Kind is not (PermanentKind.Champion or PermanentKind.Terrain),
        _ => true,
    };

    private static void Buff(TrueState state, Permanent p, Instruction ins)
    {
        if (ins.UntilEndOfTurn)
        {
            state.Modifiers.Add(new Modifier(state.NewModifierId(), p.Id, ins.Stat == "Life" ? "MaxLife" : "Attack", ins.Amount, UntilEndOfTurn: true));
            if (ins.Stat == "Life")
                p.CurrentLife += ins.Amount;
            return;
        }
        if (ins.Stat == "Life")
        {
            // A permanent max-Life raise also raises current Life (object-properties.md §3).
            p.MaxLifeDelta += ins.Amount;
            p.CurrentLife += ins.Amount;
        }
        else
        {
            p.AttackDelta += ins.Amount;
        }
    }

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
        Creation.CreateCard(state, p.Definition, owner, Zone.Hand, p.Id, p.AttackDelta, p.MaxLifeDelta);
        Creation.CeaseToExist(state, p.Id, $"{name} returns to Champion {owner}'s hand.");
    }

    /// <summary>D69/D105 Flicker: Matter → Aether. A new Permanent trace targeting the
    /// permanent's position (Slice: its own if the card's Slice filter allows it, else the filter's).</summary>
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
            Kind = TraceKind.Permanent,
            Definition = p.Definition,
            Location = TargetChoice.ForLocation(state.PositionOf(p), slice),
            Parent = p.Kind is PermanentKind.Creature or PermanentKind.Companion ? p.Parent : null,
            Behavior = p.Behavior,
            You = state.Controller(p),
            Speed = def.Speed,
            Duration = 5,
            Text = $"Flicker of {def.Name}",
            AttackDelta = p.AttackDelta,
            MaxLifeDelta = p.MaxLifeDelta,
        };
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
