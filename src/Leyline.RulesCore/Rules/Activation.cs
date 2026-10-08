using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>A complete cast or activation as the player decides it: the source (a card in Hand,
/// or a permanent), the ability (<see cref="DefaultAbilities.Cast"/> for a card), and one choice
/// list per target selection, in order, and the mana it spends (D125; null = the payment is fully
/// automatic). <see cref="Actor"/> null = a Neutral permanent's Behavior.</summary>
public sealed record ActivationRequest(PlayerId? Actor, ObjectId Source, string AbilityId, IReadOnlyList<IReadOnlyList<TargetChoice>> Targets, IReadOnlyList<ManaUnit>? Mana = null);

/// <summary>The outcome of checking a request: a draft ready to commit, an error, or — D68 — a
/// location target that is legal in the actor's view but not in true state.</summary>
public sealed record BuildResult(ActivationDraft? Draft, string? Error, int? RedirectIndex)
{
    public static BuildResult Fail(string error) => new(null, error, null);
}

/// <summary>
/// Casting and activating (D46, D70, D91, D94; interaction-stack.md §Casting and activating).
/// One procedure for every card and every activated ability, physical ones included (D75):
/// legality → pay → target → enter Pending. A request is the player's whole sequence of
/// decisions; the steps are still checked in order, and the transaction only becomes real when
/// the trace enters Pending (an unsubmitted request is the abort). Only a D68 redirect commits
/// the cost before the trace exists.
/// </summary>
public static class Activation
{
    public const string TargetKey = "target";

    public static BuildResult Build(TrueState state, ActivationRequest req) =>
        state.Find<GameObject>(req.Source) switch
        {
            CardObject card => BuildCast(state, req, card),
            Permanent p => BuildAbility(state, req, p),
            _ => BuildResult.Fail("No such source."),
        };

    // ----------------------------------------------------------------------------- casting

    private static BuildResult BuildCast(TrueState state, ActivationRequest req, CardObject card)
    {
        if (req.Actor is not { } actor || card.Zone != Zone.Hand || card.Owner != actor)
            return BuildResult.Fail("That card is not in your hand.");
        if (req.AbilityId != DefaultAbilities.Cast)
            return BuildResult.Fail("A card in hand can only be cast.");
        var def = state.Def(card);
        if (!def.Type.IsPermanentCard() && def.Type != CardType.Spell)
            return BuildResult.Fail($"{def.Name} can't be cast.");
        if (!Speeds.Allows(state, actor.SeatOf(), def.Speed))
            return BuildResult.Fail($"{def.Speed} speed doesn't allow casting now.");
        if (state.ChampionOf(actor) is not { } champion || champion.Pool is null)
            return BuildResult.Fail("You have no Champion.");
        // Casting is paid from the Champion's pool only (D20, D22).
        if (ChoosePayment(champion.Pool, def.Cost.Mana, req.Mana, out var plan) is { } payError)
            return BuildResult.Fail(payError);

        int? redirect = null;
        if (def.Type.IsPermanentCard())
        {
            if (req.Targets is not [[{ Hex: { } hex } choice]])
                return BuildResult.Fail("Choose one location.");
            var slice = ExpectedSlice(def);
            if (choice.Slice is { } s && s != slice)
                return BuildResult.Fail($"{def.Name} enters in {slice}.");
            if (!state.IsOnBoard(hex) || state.IsVoid(hex) || Network.TerrainController(state, state.TerrainOf(hex)) != actor)
                return BuildResult.Fail("You can only cast a permanent onto a terrain you control.");
            if (!Entry.CanPermanentEnter(state, def, actor, hex, slice, trueState: false, observer: actor))
                return BuildResult.Fail("No room there.");
            if (!Entry.CanPermanentEnter(state, def, actor, hex, slice, trueState: true))
                redirect = 0;
        }
        else if (!ValidatePrinted(state, actor, null, def.Targets, req.Targets))
        {
            return BuildResult.Fail("Illegal targets.");
        }

        var draft = new ActivationDraft
        {
            Actor = actor,
            Source = card.Id,
            IsCast = true,
            Ability = null,
            Card = def,
            Targets = Normalize(req.Targets, def.Type.IsPermanentCard() ? ExpectedSlice(def) : null),
            Cost = def.Cost,
            ManaPlan = plan,
            ManaPayer = champion.Id,
            ApPayer = null,
            Parent = champion.Id,
            Behavior = null,
        };
        return new BuildResult(draft, null, redirect);
    }

    /// <summary>D125: the payment a request names must be one the semi-automatic rule leaves
    /// open; a request may leave it out only when that rule leaves exactly one.</summary>
    private static string? ChoosePayment(ManaPool pool, IReadOnlyList<ManaPip> pips, IReadOnlyList<ManaUnit>? chosen, out IReadOnlyList<ManaUnit> plan)
    {
        plan = [];
        if (pips.Count == 0)
            return null;
        var options = pool.PaymentOptions(pips);
        if (options.Count == 0)
            return "Not enough mana.";
        if (chosen is null)
        {
            if (options.Count > 1)
                return "Choose how to pay.";
            plan = options[0];
            return null;
        }
        var key = chosen.OrderBy(u => (int)u.Colors).ToList();
        if (options.FirstOrDefault(o => o.SequenceEqual(key)) is not { } match)
            return "That payment isn't open.";
        plan = match;
        return null;
    }

    /// <summary>The Slice filter (D32, D110): Flying → Sky, otherwise Ground; a Structure takes
    /// its slot (Ground or Root).</summary>
    public static Slice ExpectedSlice(CardDefinition def) =>
        def.Type == CardType.Structure ? Island.SlotOf(def.SliceFilter) : def.SliceFilter;

    private static List<List<TargetChoice>> Normalize(IReadOnlyList<IReadOnlyList<TargetChoice>> targets, Slice? locationSlice) =>
        targets.Select(list => list.Select(c => locationSlice is { } s && c.Hex is not null && !c.HasEntity && c.Slice is null ? c with { Slice = s } : c).ToList()).ToList();

    // ----------------------------------------------------------------------------- abilities

    private static BuildResult BuildAbility(TrueState state, ActivationRequest req, Permanent p)
    {
        var ability = state.Abilities(p).FirstOrDefault(a => a.Id == req.AbilityId && !a.IsTriggered);
        if (ability is null)
            return BuildResult.Fail("No such ability.");

        var controller = state.Controller(p);
        if (req.Actor is { } actor)
        {
            if (controller != actor)
                return BuildResult.Fail("You don't control that.");
        }
        else if (controller is not null || p.Behavior is null)
        {
            return BuildResult.Fail("Only a Neutral permanent's Behavior acts without a Champion.");
        }

        if (p.Kind == PermanentKind.Item && p.Carrier is null)
            return BuildResult.Fail("A loose Item has no usable ability.");

        var seat = req.Actor?.SeatOf() ?? p.Behavior!.NeutralSeat;
        if (!Speeds.Allows(state, seat, ability.Speed))
            return BuildResult.Fail($"{ability.Speed} speed doesn't allow that now.");

        if (state.ApPayer(p) is not { } apPayer)
            return BuildResult.Fail("Nothing can pay for that.");
        if (p.Locked || apPayer.Locked)
            return BuildResult.Fail("It can't activate abilities until the end of this turn.");

        var observer = req.Actor;
        var (error, redirect) = ValidateAbilityTargets(state, observer, controller, p, ability, req.Targets);
        if (error is not null)
            return BuildResult.Fail(error);

        var cost = Costs.Effective(state, p, ability, req.Targets);
        IReadOnlyList<ManaUnit> plan = [];
        Permanent? manaPayer = null;
        if (cost.Mana.Count > 0)
        {
            manaPayer = state.ManaPayer(p);
            if (manaPayer?.Pool is null)
                return BuildResult.Fail("Not enough mana.");
            if (ChoosePayment(manaPayer.Pool, cost.Mana, req.Mana, out plan) is { } payError)
                return BuildResult.Fail(payError);
        }
        if (!Costs.CanPayAp(apPayer, cost, ability.Id))
            return BuildResult.Fail(cost.Flavor == ApFlavor.OncePerCycle && apPayer.UsedThisCycle.Contains(ability.Id)
                ? "Already used this cycle."
                : "Not enough Activation Points.");
        if (cost.Life > 0 && apPayer.CurrentLife < cost.Life)
            return BuildResult.Fail("Not enough Life.");

        var draft = new ActivationDraft
        {
            Actor = req.Actor,
            Source = p.Id,
            IsCast = false,
            Ability = ability,
            Card = null,
            Targets = Normalize(req.Targets, null),
            Cost = cost,
            ManaPlan = plan,
            ManaPayer = manaPayer?.Id,
            ApPayer = apPayer.Id,
            // G20: the trace's parent is the payment walk's root from the acting permanent.
            Parent = state.ManaPayer(p)?.Id,
            Behavior = controller is null ? p.Behavior : null,
        };
        return new BuildResult(draft, null, redirect);
    }

    private static (string? Error, int? Redirect) ValidateAbilityTargets(
        TrueState state, PlayerId? observer, PlayerId? controller, Permanent p, AbilityDefinition ability, IReadOnlyList<IReadOnlyList<TargetChoice>> targets)
    {
        switch (ability.Builtin)
        {
            case BuiltinAbility.Move:
            {
                if (!p.Kind.IsCreatureType() || p.Carrier is not null)
                    return ("Only a creature moves.", null);
                if (targets is not [[{ Hex: { } hex } choice]])
                    return ("Choose one destination.", null);
                if (choice.Slice is { } s && s != p.Slice)
                    return ("A move stays in its Slice.", null);
                if (!state.IsOnBoard(hex) || hex.DistanceTo(p.Hex) != 1 || state.IsVoid(hex))
                    return ("Not an adjacent terrain.", null);
                // D9/D122 realm lock: a connected Champion moves only onto terrain it bonded.
                if (p.Kind == PermanentKind.Champion && Network.IsRootConnected(state, p) && state.TerrainOf(hex).Parent != p.Id)
                    return ("Your Champion is confined to its own bonded terrain — collapse the network first.", null);
                if (!Entry.CanCreatureEnter(state, controller, hex, p.Slice, p.Id, trueState: false, observer))
                    return ("You can't enter that Slice.", null);
                return (null, Entry.CanCreatureEnter(state, controller, hex, p.Slice, p.Id, trueState: true) ? null : 0);
            }
            case BuiltinAbility.Ascend:
            case BuiltinAbility.Descend:
            {
                var from = ability.Builtin == BuiltinAbility.Ascend ? Slice.Root : Slice.Ground;
                var to = ability.Builtin == BuiltinAbility.Ascend ? Slice.Ground : Slice.Root;
                if (p.Slice != from || p.Carrier is not null)
                    return ($"Only from {from}.", null);
                if (targets.Count != 0)
                    return ("Takes no target.", null);
                if (!Entry.CanCreatureEnter(state, controller, p.Hex, to, p.Id, trueState: false, observer))
                    return ("You can't enter that Slice.", null);
                return (null, Entry.CanCreatureEnter(state, controller, p.Hex, to, p.Id, trueState: true) ? null : 0);
            }
            case BuiltinAbility.Attack:
            {
                if (targets is not [[{ HasEntity: true, Hex: { } hex, Slice: { } slice } choice]])
                    return ("Choose a terrain, a Slice and an entity.", null);
                var legal = Combat.AttackCandidates(state, p, observer).Any(c => c.Hex == hex && c.Slice == slice && c.Entity == choice.Entity);
                return legal ? (null, null) : ("Not a legal attack target.", null);
            }
            case BuiltinAbility.Defend:
            {
                if (targets is not [[{ Object: { } attackId }]] || state.Find<TraceObject>(attackId) is not { } attack)
                    return ("Choose an attack to defend.", null);
                return Combat.CanDefend(state, p, attack) ? (null, null) : ("It can't defend that attack.", null);
            }
            case BuiltinAbility.Equip:
            {
                if (targets is not [[{ Object: { } itemId }]] || state.Find<Permanent>(itemId) is not { Kind: PermanentKind.Item, Carrier: null } item)
                    return ("Choose a loose Item.", null);
                // D105: for Equip, Ground and Sky count as one Slice.
                if (p.Carrier is not null || item.Hex != p.Hex || item.Slice != Island.LooseSliceFor(p.Slice) || !state.CanSee(observer, item))
                    return ("The Item must share this location.", null);
                return (null, null);
            }
            case BuiltinAbility.Unequip:
            {
                if (targets is not [[{ Object: { } itemId }]] || state.Find<Permanent>(itemId)?.Carrier != p.Id)
                    return ("Choose an Item this carries.", null);
                return (null, null);
            }
            case BuiltinAbility.Bond:
            {
                if (!p.Kind.IsRoot())
                    return ("Only a Champion or Companion bonds.", null);
                if (targets is not [[{ Hex: { } hex }]])
                    return ("Choose a terrain.", null);
                return Network.BondCandidates(state, p).Contains(hex) ? (null, null) : ("That terrain can't be bonded now.", null);
            }
            case BuiltinAbility.Draw:
            {
                if (p.Kind != PermanentKind.Champion || controller is not { } player || targets.Count != 0)
                    return ("Only a Champion draws.", null);
                return state.Player(player).Library.Count > 0 ? (null, null) : ("Your Library is empty.", null);
            }
            case BuiltinAbility.Collapse:
            {
                if (!p.Kind.IsRoot() || targets.Count != 0)
                    return ("Only a root collapses its network.", null);
                return Network.BondedBy(state, p.Id).Any() ? (null, null) : ("There is no network to collapse.", null);
            }
            default:
                return ValidatePrinted(state, controller, p, ability.Targets, targets) ? (null, null) : ("Illegal targets.", null);
        }
    }

    private static bool ValidatePrinted(TrueState state, PlayerId? you, Permanent? source, IReadOnlyList<TargetSpec> specs, IReadOnlyList<IReadOnlyList<TargetChoice>> targets)
    {
        if (targets.Count != specs.Count)
            return false;
        var bound = new Dictionary<string, List<TargetChoice>>(StringComparer.Ordinal);
        for (var i = 0; i < specs.Count; i++)
        {
            if (!Targeting.IsValid(state, you, source, specs[i], targets[i], bound))
                return false;
            bound[specs[i].Name] = targets[i].ToList();
        }
        return true;
    }

    // ----------------------------------------------------------------------------- commit

    /// <summary>Pays the cost (D70 step 1). The card leaves the Hand as payment starts and
    /// arrives in Discard when it ends (D37, D94).</summary>
    public static void PayCost(TrueState state, ActivationDraft draft)
    {
        if (draft.CostCommitted)
            return;
        if (draft.ManaPayer is { } payerId && draft.ManaPlan.Count > 0)
            state.Get<Permanent>(payerId).Pool!.Spend(draft.ManaPlan);
        if (draft.ApPayer is { } apId && draft.Ability is not null)
        {
            var apPayer = state.Get<Permanent>(apId);
            Costs.PayAp(state, apPayer, draft.Cost, draft.Ability.Id);
            if (draft.Cost.Life > 0)
                apPayer.CurrentLife -= draft.Cost.Life;
        }
        if (draft.IsCast)
        {
            var card = state.Get<CardObject>(draft.Source);
            var zones = state.Player(card.Owner);
            zones.Hand.Remove(card.Id);
            zones.Discard.Add(card.Id);
            card.Zone = Zone.Discard;
        }
        draft.CostCommitted = true;
    }

    /// <summary>Steps 1 (if not yet paid) and 4: the trace enters Pending, with any triggers the
    /// payment fired on top of it (D94).</summary>
    public static TraceObject Commit(TrueState state, ActivationDraft draft)
    {
        PayCost(state, draft);
        var trace = Creation.CreateTrace(state, BuildTrace(state, draft));
        state.Note($"{trace.Text} enters Pending.", trace.Id);
        Consequences.Run(state);
        Triggers.Flush(state);
        return trace;
    }

    private static TraceObject BuildTrace(TrueState state, ActivationDraft draft)
    {
        if (draft.IsCast)
        {
            var card = state.Get<CardObject>(draft.Source);
            var def = draft.Card!;
            if (def.Type.IsPermanentCard())
            {
                var location = draft.Targets[0][0];
                return new TraceObject
                {
                    Id = state.NewId(), Timestamp = state.NewTimestamp(), Source = card.Id,
                    Kind = TraceKind.Permanent, Definition = def.Id, Location = location,
                    Parent = draft.Parent, You = draft.Actor, Speed = def.Speed, Duration = 5,
                    Text = $"Cast {def.Name} at {location}", PaidCost = draft.Cost.ToString(),
                    AttackDelta = card.AttackDelta, MaxLifeDelta = card.MaxLifeDelta,
                };
            }
            var spell = new TraceObject
            {
                Id = state.NewId(), Timestamp = state.NewTimestamp(), Source = card.Id,
                Kind = TraceKind.Spell, Definition = def.Id, Parent = draft.Parent, You = draft.Actor,
                Speed = def.Speed, Duration = def.Duration, Specs = def.Targets, Instructions = def.Instructions,
                Text = $"Cast {def.Name}", PaidCost = draft.Cost.ToString(),
            };
            for (var i = 0; i < def.Targets.Count; i++)
                spell.Targets[def.Targets[i].Name] = draft.Targets[i];
            return spell;
        }

        var p = state.Get<Permanent>(draft.Source);
        var ability = draft.Ability!;
        AttackInfo? attack = null;
        TargetChoice? loc = null;
        switch (ability.Builtin)
        {
            case BuiltinAbility.Attack:
                var a = draft.Targets[0][0];
                attack = new AttackInfo { Hex = a.Hex!.Value, Slice = a.Slice!.Value, Entity = a.Entity };
                break;
            case BuiltinAbility.Move:
            case BuiltinAbility.Bond:
                loc = draft.Targets[0][0] with { Slice = ability.Builtin == BuiltinAbility.Move ? p.Slice : null };
                break;
            case BuiltinAbility.Ascend:
                loc = TargetChoice.ForLocation(p.Hex, Slice.Ground);
                break;
            case BuiltinAbility.Descend:
                loc = TargetChoice.ForLocation(p.Hex, Slice.Root);
                break;
        }

        var trace = new TraceObject
        {
            Id = state.NewId(), Timestamp = state.NewTimestamp(), Source = p.Id,
            Kind = TraceKind.Ability, Ability = ability, ActingPermanent = p.Id,
            Parent = draft.Parent, Behavior = draft.Behavior, You = draft.Actor,
            Physical = ability.Physical, Duration = ability.Physical ? 0 : ability.Duration, Speed = ability.Speed,
            Specs = ability.Targets, Instructions = ability.Instructions,
            Attack = attack, Location = loc,
            Text = $"{state.NameOf(p.Id)}: {ability.Name}", PaidCost = draft.Cost.ToString(),
        };
        if (ability.Builtin == BuiltinAbility.None)
        {
            for (var i = 0; i < ability.Targets.Count; i++)
                trace.Targets[ability.Targets[i].Name] = draft.Targets[i];
        }
        else if (draft.Targets.Count > 0)
        {
            trace.Targets[TargetKey] = draft.Targets[0];
        }
        return trace;
    }

    // ----------------------------------------------------------------------------- enumeration

    /// <summary>Every cast and activation the actor may declare now, enumerated against its own
    /// view only — a request that would hit a hidden occupant is offered like any other and
    /// discovers the problem only when declared (D68), so the list never leaks hidden facts.</summary>
    public static IEnumerable<ActivationRequest> LegalFor(TrueState state, PlayerId actor)
    {
        var zones = state.Player(actor);
        foreach (var cardId in zones.Hand.ToList())
        {
            var card = state.Get<CardObject>(cardId);
            var def = state.Def(card);
            if (!Speeds.Allows(state, actor.SeatOf(), def.Speed))
                continue;
            if (state.ChampionOf(actor)?.Pool is not { } pool)
                continue;
            var payments = Payments(pool.PaymentOptions(def.Cost.Mana), def.Cost.Mana);
            if (payments.Count == 0)
                continue;

            IEnumerable<IReadOnlyList<IReadOnlyList<TargetChoice>>> options;
            if (def.Type.IsPermanentCard())
            {
                var slice = ExpectedSlice(def);
                options = state.Hexes
                    .Where(h => !state.IsVoid(h) && Network.TerrainController(state, state.TerrainOf(h)) == actor)
                    .Select(h => (IReadOnlyList<IReadOnlyList<TargetChoice>>)[[TargetChoice.ForLocation(h, slice)]]);
            }
            else if (def.Type == CardType.Spell)
            {
                options = PrintedOptions(state, actor, null, def.Targets);
            }
            else
            {
                continue;
            }

            foreach (var targets in options)
            foreach (var mana in payments)
            {
                var req = new ActivationRequest(actor, cardId, DefaultAbilities.Cast, targets, mana);
                if (Build(state, req).Error is null)
                    yield return req;
            }
        }

        foreach (var p in state.Permanents.Where(p => p.Kind != PermanentKind.Terrain && p.Kind != PermanentKind.Remnant).ToList())
        {
            if (state.Controller(p) != actor)
                continue;
            foreach (var req in RequestsFor(state, actor, p))
                yield return req;
        }
    }

    /// <summary>The requests one permanent could make — used for a Champion's permanents and,
    /// with <paramref name="actor"/> null, by a Neutral permanent's Behavior.</summary>
    public static IEnumerable<ActivationRequest> RequestsFor(TrueState state, PlayerId? actor, Permanent p)
    {
        if (p.Kind == PermanentKind.Item && p.Carrier is null)
            yield break;
        var seat = actor?.SeatOf() ?? p.Behavior?.NeutralSeat;
        if (seat is null)
            yield break;

        foreach (var ability in state.Abilities(p).Where(a => !a.IsTriggered))
        {
            if (!Speeds.Allows(state, seat.Value, ability.Speed))
                continue;
            foreach (var targets in BuiltinOptions(state, actor, p, ability))
            {
                var cost = Costs.Effective(state, p, ability, targets);
                var payments = cost.Mana.Count == 0 || state.ManaPayer(p)?.Pool is not { } pool
                    ? Automatic
                    : Payments(pool.PaymentOptions(cost.Mana), cost.Mana);
                foreach (var mana in payments)
                {
                    var req = new ActivationRequest(actor, p.Id, ability.Id, targets, mana);
                    if (Build(state, req).Error is null)
                        yield return req;
                }
            }
        }
    }

    private static IEnumerable<IReadOnlyList<IReadOnlyList<TargetChoice>>> BuiltinOptions(TrueState state, PlayerId? actor, Permanent p, AbilityDefinition ability)
    {
        switch (ability.Builtin)
        {
            case BuiltinAbility.Move:
                return state.Neighbors(p.Hex).Select(h => One(TargetChoice.ForLocation(h, p.Slice)));
            case BuiltinAbility.Attack:
                return Combat.AttackCandidates(state, p, actor).Select(One);
            case BuiltinAbility.Defend:
                return Combat.DefendableAttacks(state, p).Select(t => One(TargetChoice.ForObject(t.Id)));
            case BuiltinAbility.Equip:
                return state.LooseAt(p.Hex).Where(i => i.Kind == PermanentKind.Item).Select(i => One(TargetChoice.ForObject(i.Id)));
            case BuiltinAbility.Unequip:
                return state.CarriedBy(p.Id).Select(i => One(TargetChoice.ForObject(i.Id)));
            case BuiltinAbility.Bond:
                return p.Kind.IsRoot() ? Network.BondCandidates(state, p).Select(h => One(TargetChoice.ForLocation(h))) : [];
            case BuiltinAbility.Draw:
            case BuiltinAbility.Collapse:
            case BuiltinAbility.Ascend:
            case BuiltinAbility.Descend:
                return [[]];
            default:
                return PrintedOptions(state, state.Controller(p), p, ability.Targets);
        }
    }

    private static IReadOnlyList<IReadOnlyList<TargetChoice>> One(TargetChoice c) => [[c]];

    /// <summary>The payment field of each enumerated request: null when the payment is automatic
    /// (or there is no mana cost), else one request per open payment (D125).</summary>
    private static IReadOnlyList<IReadOnlyList<ManaUnit>?> Payments(IReadOnlyList<IReadOnlyList<ManaUnit>> options, IReadOnlyList<ManaPip> pips) =>
        pips.Count == 0 || options.Count == 1 ? Automatic : options;

    private static readonly IReadOnlyList<IReadOnlyList<ManaUnit>?> Automatic = [null];

    private static IEnumerable<IReadOnlyList<IReadOnlyList<TargetChoice>>> PrintedOptions(TrueState state, PlayerId? you, Permanent? source, IReadOnlyList<TargetSpec> specs)
    {
        IEnumerable<IReadOnlyList<IReadOnlyList<TargetChoice>>> Recurse(int index, List<IReadOnlyList<TargetChoice>> chosen, Dictionary<string, List<TargetChoice>> bound)
        {
            if (index == specs.Count)
            {
                yield return chosen.ToList();
                yield break;
            }
            foreach (var option in Targeting.Options(state, you, source, specs[index], bound).ToList())
            {
                chosen.Add(option);
                bound[specs[index].Name] = option;
                foreach (var result in Recurse(index + 1, chosen, bound))
                    yield return result;
                bound.Remove(specs[index].Name);
                chosen.RemoveAt(chosen.Count - 1);
            }
        }
        return Recurse(0, [], new Dictionary<string, List<TargetChoice>>(StringComparer.Ordinal));
    }
}
