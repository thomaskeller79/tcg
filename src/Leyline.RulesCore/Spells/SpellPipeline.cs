using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Spells;

/// <summary>
/// D20: casting is a mana-only cost, unrelated to any actor's AP ("payable multiple times/turn
/// as mana allows") — sorcery-speed (gated by the same active-player/Action-phase check every
/// other RulesEngine.LegalCommands entry already gets), no priority window (M1's window stays
/// locked to Combat-declare only, per PriorityWindow's own doc comment). A Creature spell
/// summons onto a bonded AND connected/producing terrain cell (D20's "realm is the deployment
/// zone as well as the economy" — reuses Query.ResolveConnectedProducingTerrain, the exact set
/// mana itself draws from) with room on the Ground layer (Query.CanOccupyLayer, the same rule
/// Move now enforces). A Rite spell resolves one of two hardcoded effects (RiteEffectIds) and
/// leaves no permanent — see CardDefinition's doc comment for what's deliberately not built
/// (the full Aether trace/fade model, D16).
/// </summary>
public static class SpellPipeline
{
    public static CommandResult CastCreature(TrueState state, EventPipeline pipeline, CastCreatureCommand cmd)
    {
        var player = state.Players.First(p => p.Id == cmd.Actor);
        if (!player.Hand.Contains(cmd.Card))
            return CommandResult.Reject("That card is not in your hand.");

        var def = state.Content.Get(cmd.Card);
        if (def.Type != CardType.Creature)
            return CommandResult.Reject($"{cmd.Card} is not a Creature card.");
        if (player.Mana < def.ManaCost)
            return CommandResult.Reject("Not enough mana.");
        if (!CanSummonTo(cmd.Actor, cmd.Target, state))
            return CommandResult.Reject("Illegal summoning target.");

        var newActorId = state.AllocateActorId();
        var events = new List<IEvent>();
        events.AddRange(pipeline.Process(new HandCardRemovedIntent(cmd.Actor, cmd.Card), state));
        events.AddRange(pipeline.Process(new ManaChangeIntent(cmd.Actor, player.Mana - def.ManaCost), state));
        events.AddRange(pipeline.Process(new CreatureSummonedIntent(newActorId, cmd.Actor, cmd.Card, cmd.Target), state));
        return CommandResult.Accept(events);
    }

    public static CommandResult CastRite(TrueState state, EventPipeline pipeline, CastRiteCommand cmd)
    {
        var player = state.Players.First(p => p.Id == cmd.Actor);
        if (!player.Hand.Contains(cmd.Card))
            return CommandResult.Reject("That card is not in your hand.");

        var def = state.Content.Get(cmd.Card);
        if (def.Type != CardType.Rite)
            return CommandResult.Reject($"{cmd.Card} is not a Rite card.");
        if (player.Mana < def.ManaCost)
            return CommandResult.Reject("Not enough mana.");
        if (state.FindActor(cmd.Target) is null || !Query.IsVisibleTo(cmd.Target, cmd.Actor, state))
            return CommandResult.Reject("Illegal rite target.");

        var effectIntent = def.EffectId switch
        {
            RiteEffectIds.Damage => (EventIntent)new DamageIntent(CasterChampionId(cmd.Actor, state), cmd.Target, def.EffectAmount),
            RiteEffectIds.Heal => new HealIntent(cmd.Target, def.EffectAmount),
            _ => null,
        };
        if (effectIntent is null)
            return CommandResult.Reject($"{cmd.Card} has no recognized rite effect.");

        var events = new List<IEvent>();
        events.AddRange(pipeline.Process(new HandCardRemovedIntent(cmd.Actor, cmd.Card), state));
        events.AddRange(pipeline.Process(new ManaChangeIntent(cmd.Actor, player.Mana - def.ManaCost), state));
        events.AddRange(pipeline.Process(effectIntent, state));
        return CommandResult.Accept(events);
    }

    public static IReadOnlyList<CastCreatureCommand> LegalCastCreatureCommands(TrueState state, PlayerId player)
    {
        var playerState = state.Players.First(p => p.Id == player);
        var targets = state.Board.AllCells
            .Select(c => c.Coord)
            .Where(coord => CanSummonTo(player, coord, state))
            .ToList();

        return playerState.Hand.Distinct()
            .Select(state.Content.Get)
            .Where(def => def.Type == CardType.Creature && playerState.Mana >= def.ManaCost)
            .SelectMany(def => targets.Select(t => new CastCreatureCommand(player, def.Id, t)))
            .ToList();
    }

    public static IReadOnlyList<CastRiteCommand> LegalCastRiteCommands(TrueState state, PlayerId player)
    {
        var playerState = state.Players.First(p => p.Id == player);
        var targets = state.AllActors.Where(a => Query.IsVisibleTo(a.Id, player, state)).Select(a => a.Id).ToList();

        return playerState.Hand.Distinct()
            .Select(state.Content.Get)
            .Where(def => def.Type == CardType.Rite && playerState.Mana >= def.ManaCost)
            .SelectMany(def => targets.Select(t => new CastRiteCommand(player, def.Id, t)))
            .ToList();
    }

    private static bool CanSummonTo(PlayerId player, HexCoord target, TrueState state) =>
        Query.ResolveConnectedProducingTerrain(player, state).Contains(target)
        && Query.CanOccupyLayer(player, target, Layer.Ground, state);

    private static ActorId CasterChampionId(PlayerId player, TrueState state) =>
        state.AllActors.OfType<ChampionState>().First(c => c.Owner == player).Id;
}
