using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Events;

/// <summary>The only thing allowed to mutate TrueState — see TrueState's own doc comment.</summary>
public interface IEvent
{
    void Apply(TrueState state);
}

public sealed record DamageEvent(ActorId Source, ActorId Target, int Amount) : IEvent
{
    public void Apply(TrueState state)
    {
        var target = state.FindActor(Target);
        if (target is not null)
            target.Life -= Amount;
    }
}

public sealed record ActorMovedEvent(ActorId Actor, HexCoord From, HexCoord To) : IEvent
{
    public void Apply(TrueState state)
    {
        var actor = state.FindActor(Actor);
        if (actor is null)
            return;
        state.Board.GetCell(From).LayerOf(actor.Layer).Remove(Actor);
        state.Board.GetCell(To).LayerOf(actor.Layer).Add(Actor);
        actor.Position = To;
    }
}

public sealed record ActorApChangedEvent(ActorId Actor, int NewAp) : IEvent
{
    public void Apply(TrueState state)
    {
        var actor = state.FindActor(Actor);
        if (actor is not null)
            actor.CurrentAp = NewAp;
    }
}

public sealed record ActorDestroyedEvent(ActorId Actor) : IEvent
{
    public void Apply(TrueState state) => state.RemoveActor(Actor);
}

public sealed record CombatDeclaredEvent(CombatId Combat, ActorId Attacker, HexCoord TargetHex) : IEvent
{
    public void Apply(TrueState state) =>
        state.ActiveCombats.Add(new CombatState { Id = Combat, Attacker = Attacker, TargetHex = TargetHex });
}

public sealed record DefendersDeclaredEvent(CombatId Combat, IReadOnlyList<ActorId> Defenders) : IEvent
{
    public void Apply(TrueState state)
    {
        var combat = state.GetCombat(Combat);
        combat.Defenders.Clear();
        combat.Defenders.AddRange(Defenders);
        combat.Phase = Defenders.Count switch
        {
            0 => CombatPhase.AwaitingUndefendedChoice,
            1 => CombatPhase.AwaitingWindow,
            _ => CombatPhase.AwaitingAssignment,
        };
    }
}

public sealed record DamageAssignedEvent(CombatId Combat, IReadOnlyDictionary<ActorId, int> Assignment) : IEvent
{
    public void Apply(TrueState state)
    {
        var combat = state.GetCombat(Combat);
        combat.DamageAssignment = new Dictionary<ActorId, int>(Assignment);
        combat.Phase = CombatPhase.AwaitingWindow;
    }
}

public sealed record UndefendedTargetChosenEvent(CombatId Combat, ActorId Target) : IEvent
{
    public void Apply(TrueState state)
    {
        var combat = state.GetCombat(Combat);
        combat.UndefendedTarget = Target;
        combat.Phase = CombatPhase.AwaitingWindow;
    }
}

public sealed record CombatResolvedEvent(CombatId Combat) : IEvent
{
    public void Apply(TrueState state) => state.ActiveCombats.RemoveAll(c => c.Id == Combat);
}

public sealed record PhaseChangedEvent(int NewPhaseIndex) : IEvent
{
    public void Apply(TrueState state) => state.CurrentPhaseIndex = NewPhaseIndex;
}

public sealed record TurnAdvancedEvent(int NewTurnNumber, PlayerId NewActivePlayer) : IEvent
{
    public void Apply(TrueState state)
    {
        state.TurnNumber = NewTurnNumber;
        state.ActivePlayer = NewActivePlayer;
    }
}

/// <summary>D9: killing the enemy Champion wins — the win check is an evaluated effect
/// (ChampionDeathCheck), never a hardcoded `if` in Combat.</summary>
public sealed record MatchEndedEvent(PlayerId Loser) : IEvent
{
    public void Apply(TrueState state) => state.Winner = state.Players.Select(p => p.Id).First(id => id != Loser);
}

/// <summary>D8: bonding is permanent — only the mana draw is conditional (Query.ResolveConnectedProducingTerrain).</summary>
public sealed record TerrainBondedEvent(PlayerId Player, HexCoord Target) : IEvent
{
    public void Apply(TrueState state)
    {
        var champion = state.AllActors.OfType<ChampionState>().FirstOrDefault(c => c.Owner == Player);
        champion?.Network.Bond(Target);
    }
}

/// <summary>D9's `*` cost flavor: marks an action id as spent for the actor's current turn.</summary>
public sealed record OncePerTurnActionUsedEvent(ActorId Actor, string ActionId) : IEvent
{
    public void Apply(TrueState state) => state.FindActor(Actor)?.OncePerTurnActionsUsed.Add(ActionId);
}

public sealed record OncePerTurnActionsResetEvent(ActorId Actor) : IEvent
{
    public void Apply(TrueState state) => state.FindActor(Actor)?.OncePerTurnActionsUsed.Clear();
}

/// <summary>D21: mana refreshes to a computed value each Beginning phase — no banking.</summary>
public sealed record ManaChangedEvent(PlayerId Player, int NewMana) : IEvent
{
    public void Apply(TrueState state)
    {
        var player = state.Players.First(p => p.Id == Player);
        player.Mana = NewMana;
    }
}

public sealed record ActorRevealedEvent(ActorId Actor) : IEvent
{
    public void Apply(TrueState state)
    {
        var actor = state.FindActor(Actor);
        if (actor is not null)
            actor.Located = true;
    }
}

public sealed record ActorConcealedEvent(ActorId Actor) : IEvent
{
    public void Apply(TrueState state)
    {
        var actor = state.FindActor(Actor);
        if (actor is not null)
            actor.Located = false;
    }
}

public sealed record AddModifierEvent(IModifier Modifier) : IEvent
{
    public void Apply(TrueState state) => state.ActiveModifiers.Add(Modifier);
}

public sealed record RemoveModifierEvent(ModifierId Modifier) : IEvent
{
    public void Apply(TrueState state) => state.ActiveModifiers.RemoveAll(m => m.Id == Modifier);
}

/// <summary>D9's `5*AP` Draw action: moves the top (index 0) of Library into Hand. The pipeline
/// peeks Library[0] and passes it explicitly rather than having Apply re-peek, so the intent/
/// event is self-describing (what was drawn is visible in the replay log, not inferred).</summary>
public sealed record CardDrawnEvent(PlayerId Player, CardDefinitionId Card) : IEvent
{
    public void Apply(TrueState state)
    {
        var player = state.Players.First(p => p.Id == Player);
        player.Library.RemoveAt(0);
        player.Hand.Add(Card);
    }
}

/// <summary>Casting (Creature or Rite) removes the cast card from Hand — shared by both,
/// since M1 doesn't track a per-instance card identity, just "one fewer of this definition."</summary>
public sealed record HandCardRemovedEvent(PlayerId Player, CardDefinitionId Card) : IEvent
{
    public void Apply(TrueState state) => state.Players.First(p => p.Id == Player).Hand.Remove(Card);
}

/// <summary>D20: summoning a Creature spell — CurrentAp starts at 0 (summoning sickness, D14's
/// pessimistic default: it can't act until its own next Beginning-phase refresh), same
/// zero-until-refresh pattern the Champion itself uses at match start.</summary>
public sealed record CreatureSummonedEvent(ActorId NewActor, PlayerId Owner, CardDefinitionId Definition, HexCoord Position) : IEvent
{
    public void Apply(TrueState state)
    {
        var def = state.Content.Get(Definition);
        state.AddActor(new CreatureState
        {
            Id = NewActor,
            Owner = Owner,
            Definition = Definition,
            Position = Position,
            Layer = Layer.Ground,
            Located = true,
            Life = def.Life,
            CurrentAp = 0,
        });
    }
}

/// <summary>The Rite-effect placeholder's "heal" half (design-continuous-effects.md flagged
/// this as not existing yet) — mirrors DamageEvent but adds. No overheal cap: nothing in the
/// docs establishes one, and inventing an uncited rule here would be worse than leaving it open.</summary>
public sealed record HealEvent(ActorId Target, int Amount) : IEvent
{
    public void Apply(TrueState state)
    {
        var target = state.FindActor(Target);
        if (target is not null)
            target.Life += Amount;
    }
}

/// <summary>D9 (under test, 2026-08-08): the Champion's free "Collapse the network" ability —
/// drops every bond outright, the only way to become mobile again once "rooted" by an active
/// network (Query.ResolveMoveCost/ResolveLegalMoveTargets).</summary>
public sealed record NetworkCollapsedEvent(ActorId Champion) : IEvent
{
    public void Apply(TrueState state)
    {
        if (state.FindActor(Champion) is ChampionState champion)
            champion.Network.Collapse();
    }
}
