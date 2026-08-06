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

/// <summary>D9: bonding and "act as a creature" share one Channel-used flag.</summary>
public sealed record ChannelUsedEvent(PlayerId Player) : IEvent
{
    public void Apply(TrueState state)
    {
        var champion = state.AllActors.OfType<ChampionState>().FirstOrDefault(c => c.Owner == Player);
        if (champion is not null)
            champion.ChannelUsedThisTurn = true;
    }
}

public sealed record ChannelResetEvent(ActorId Champion) : IEvent
{
    public void Apply(TrueState state)
    {
        if (state.FindActor(Champion) is ChampionState champion)
            champion.ChannelUsedThisTurn = false;
    }
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
