using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// Creating and removing objects (object-properties.md). Nothing ever moves between zones: a
/// transition creates a new object from its predecessor's current state (D69). An object that
/// leaves every zone ceases to exist, and links through it are joined up (D97); a removed node
/// updates its subtree in the same step (ancestry.md §Removing an inner node).
/// </summary>
public static class Creation
{
    public static CardObject CreateCard(TrueState state, string definition, PlayerId owner, CardSection section, ObjectId? source = null, int attackDelta = 0, int maxLifeDelta = 0)
    {
        var card = new CardObject
        {
            Id = state.NewId(),
            Timestamp = state.NewTimestamp(),
            Definition = definition,
            Owner = owner,
            Section = section,
            Source = source,
            AttackDelta = attackDelta,
            MaxLifeDelta = maxLifeDelta,
        };
        state.Add(card);
        state.Player(owner).Section(section).Add(card.Id);
        // D149: a card made from a predecessor copies each belief once (a bounced permanent is
        // known to everyone, D155); a new one is known to its owner outside the Library, and to
        // everyone in Discard. Everyone sees it arrive, except in a Library (D154).
        foreach (var who in state.Players.Select(p => p.Id))
        {
            var belief = (source is { } s ? state.Find<GameObject>(s) : null) switch
            {
                CardObject original => state.Beliefs.Card(who, original.Id).Copy(),
                Permanent => new CardBelief { Definition = definition },
                _ => new CardBelief { Definition = section == CardSection.Discard || (who == owner && section != CardSection.Library) ? definition : null },
            };
            belief.PositionKnown = section != CardSection.Library;
            state.Beliefs.SetCard(who, card.Id, belief);
        }
        return card;
    }

    /// <summary>A new permanent enters the Island (D89): 0 Activation Points unless Haste X
    /// (D114), current Life at max. Fires its "enters the Island" triggers.</summary>
    public static Permanent CreatePermanent(
        TrueState state,
        CardDefinition def,
        HexCoord hex,
        Slice slice,
        ObjectId? parent,
        BehaviorAssignment? behavior,
        ObjectId? source,
        int? timestamp = null,
        int attackDelta = 0,
        int maxLifeDelta = 0)
    {
        var kind = def.Type.ToPermanentKind();
        var p = new Permanent
        {
            Id = state.NewId(),
            Timestamp = timestamp ?? state.NewTimestamp(),
            Definition = def.Id,
            Kind = kind,
            Hex = hex,
            Slice = kind == PermanentKind.Structure ? Island.SlotOf(slice) : slice,
            Parent = kind is PermanentKind.Creature or PermanentKind.Companion ? parent : null,
            Behavior = behavior,
            Source = source,
            AttackDelta = attackDelta,
            MaxLifeDelta = maxLifeDelta,
            Pool = kind.IsRoot() ? new ManaPool() : null,
        };
        state.Add(p);
        Observation.Enter(state, p);
        p.CurrentLife = state.MaxLife(p);
        p.CurrentAp = def.HasKeyword(Keyword.Haste) ? HasteAp(state, p, def) : 0;
        state.Note($"{state.NameOf(p.Id)} enters the Island at {hex} {p.Slice}.", p.Id);
        Triggers.Fire(state, TriggerEvent.EntersIsland, p);
        return p;
    }

    private static int HasteAp(TrueState state, Permanent p, CardDefinition def)
    {
        var x = def.KeywordValue(Keyword.Haste);
        var max = state.MaxAp(p);
        return x <= 0 ? max : Math.Min(x, max);
    }

    /// <summary>D110: a fallen Creature or Companion leaves a Remnant on its terrain — a new
    /// object keeping only the name. Root stays Root; Ground and Sky become Ground.</summary>
    public static Permanent CreateRemnant(TrueState state, Permanent fallen)
    {
        var remnant = new Permanent
        {
            Id = state.NewId(),
            Timestamp = state.NewTimestamp(),
            Definition = fallen.Definition,
            Kind = PermanentKind.Remnant,
            Hex = state.PositionOf(fallen),
            Slice = Island.LooseSliceFor(fallen.Slice),
            Source = fallen.Id,
        };
        state.Add(remnant);
        Observation.Enter(state, remnant, fallen);
        return remnant;
    }

    public static TraceObject CreateTrace(TrueState state, TraceObject trace)
    {
        state.Add(trace);
        trace.Section = TraceSection.Pending;
        state.Pending.Add(trace.Id);
        return trace;
    }

    /// <summary>Removes an object from every zone (D97) and runs the subtree update for a
    /// removed permanent (D83, D107, D110). Remnants and fall triggers are the caller's
    /// (Consequences) job; <paramref name="reason"/> is the log line, empty for none.</summary>
    public static void CeaseToExist(TrueState state, ObjectId id, string reason)
    {
        var obj = state.Find<GameObject>(id);
        if (obj is null)
            return;

        if (obj is Permanent p)
            UpdateSubtree(state, p);

        if (obj is CardObject card)
            state.Player(card.Owner).Section(card.Section).Remove(id);
        state.Beliefs.Forget(id);
        if (obj is TraceObject trace)
            state.Section(trace.Section).Remove(id);
        state.Modifiers.RemoveAll(m => m.Subject == id);

        // D144: the modifiers it was the vessel of end. D143: ±Health and ±AP were exact mirrors
        // on max and current, so ending one takes back what it gave current.
        foreach (var m in state.Modifiers.Where(m => m.Vessel == id).ToList())
        {
            state.Modifiers.Remove(m);
            var subject = state.Get<Permanent>(m.Subject);
            if (m.Stat == Stats.MaxLifeStat)
                subject.CurrentLife -= m.Delta;
            else if (m.Stat == Stats.MaxApStat)
                subject.CurrentAp = Math.Max(0, subject.CurrentAp - m.Delta);
        }

        // D97: whatever had it as `source` takes over its `source`.
        foreach (var other in state.AllObjects.Where(o => o.Source == id).ToList())
            other.Source = obj.Source;

        state.Remove(id);
        if (reason.Length > 0)
            state.Note(reason, id);
    }

    private static void UpdateSubtree(TrueState state, Permanent p)
    {
        // Creature ceases to exist → its Items drop on its terrain (D27, D105).
        foreach (var item in state.CarriedBy(p.Id))
        {
            item.Hex = state.PositionOf(p);
            item.Slice = Island.LooseSliceFor(p.Slice);
            item.Carrier = null;
        }

        if (!p.Kind.IsRoot())
            return;

        // A root ceasing to exist → its bonded terrain reverts to unbonded (D22).
        foreach (var terrain in Network.BondedBy(state, p.Id).ToList())
            terrain.Parent = null;

        // D82/D83: a still-Champion-controlled Companion ceases to exist → each creature it
        // funded becomes Neutral, Aggressive toward the other Champion, acting in the neutral
        // turn paired with its former controller. Traces it parented lose payer and controller.
        var formerController = state.Controller(p);
        foreach (var child in state.Permanents.Where(c => c.Parent == p.Id && c.Kind is PermanentKind.Creature or PermanentKind.Companion))
        {
            child.Parent = null;
            if (formerController is { } former)
            {
                child.Behavior = new BehaviorAssignment(BehaviorAssignment.Aggressive, former.Opponent, former.NeutralSeatAfter());
                state.Note($"{state.NameOf(child.Id)} becomes Neutral ({child.Behavior}).", child.Id);
            }
        }
        foreach (var trace in state.AllObjects.OfType<TraceObject>().Where(t => t.Parent == p.Id))
            trace.Parent = null;
    }
}
