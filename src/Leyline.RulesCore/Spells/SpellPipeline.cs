using Leyline.RulesCore.Aether;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Spells;

/// <summary>
/// D20: casting is a mana-only cost, unrelated to any actor's AP ("payable multiple times/turn
/// as mana allows") — sorcery-speed (gated by the same active-player/Action-phase check every
/// other RulesEngine.LegalCommands entry already gets; every card in M1 is Slow, D45, so "Pending
/// empty" already follows from that same gate — see PriorityWindow's doc comment). Cost is paid
/// and the card discharges into Discard immediately (D46, D37); what the cast actually DOES
/// (summon a Creature, resolve a Spell's effect) is deferred into a Trace on Pending and only
/// happens once both players pass (AetherPipeline.Pass) — see PendingCreatureCast/PendingSpellCast.
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

        var events = new List<IEvent>();
        events.AddRange(pipeline.Process(new HandCardRemovedIntent(cmd.Actor, cmd.Card), state));
        events.AddRange(pipeline.Process(new CardDischargedIntent(cmd.Actor, cmd.Card), state));
        events.AddRange(pipeline.Process(new ManaChangeIntent(cmd.Actor, player.Mana - def.ManaCost), state));

        EnterPending(state, cmd.Actor, new PendingCreatureCast(cmd.Actor, cmd.Card, cmd.Target));
        return CommandResult.Accept(events);
    }

    public static CommandResult CastSpell(TrueState state, EventPipeline pipeline, CastSpellCommand cmd)
    {
        var player = state.Players.First(p => p.Id == cmd.Actor);
        if (!player.Hand.Contains(cmd.Card))
            return CommandResult.Reject("That card is not in your hand.");

        var def = state.Content.Get(cmd.Card);
        if (def.Type != CardType.Spell)
            return CommandResult.Reject($"{cmd.Card} is not a Spell card.");
        if (player.Mana < def.ManaCost)
            return CommandResult.Reject("Not enough mana.");
        if (state.FindActor(cmd.Target) is null || !Query.IsVisibleTo(cmd.Target, cmd.Actor, state))
            return CommandResult.Reject("Illegal spell target.");
        if (def.EffectId is not (SpellEffectIds.Damage or SpellEffectIds.Heal))
            return CommandResult.Reject($"{cmd.Card} has no recognized spell effect.");

        var events = new List<IEvent>();
        events.AddRange(pipeline.Process(new HandCardRemovedIntent(cmd.Actor, cmd.Card), state));
        events.AddRange(pipeline.Process(new CardDischargedIntent(cmd.Actor, cmd.Card), state));
        events.AddRange(pipeline.Process(new ManaChangeIntent(cmd.Actor, player.Mana - def.ManaCost), state));

        EnterPending(state, cmd.Actor, new PendingSpellCast(cmd.Actor, cmd.Card, cmd.Target));
        return CommandResult.Accept(events);
    }

    /// <summary>Pushes a cast's Trace onto Pending and opens the response window for it — the
    /// opponent gets first priority (same shape as Combat's OpenPriorityWindow), then it comes
    /// back to the caster before both passing lets it resolve.</summary>
    private static void EnterPending(TrueState state, PlayerId caster, IPendingResolution resolution)
    {
        var opponent = state.Players.Select(p => p.Id).First(id => id != caster);
        var traceId = state.AllocateTraceId();
        state.Pending.Push(new Trace(traceId, caster, resolution));
        AetherPipeline.OpenPriorityWindow(state, PriorityWindowKind.CastResolution, traceId, [opponent, caster]);
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

    public static IReadOnlyList<CastSpellCommand> LegalCastSpellCommands(TrueState state, PlayerId player)
    {
        var playerState = state.Players.First(p => p.Id == player);
        var targets = state.AllActors.Where(a => Query.IsVisibleTo(a.Id, player, state)).Select(a => a.Id).ToList();

        return playerState.Hand.Distinct()
            .Select(state.Content.Get)
            .Where(def => def.Type == CardType.Spell && playerState.Mana >= def.ManaCost)
            .SelectMany(def => targets.Select(t => new CastSpellCommand(player, def.Id, t)))
            .ToList();
    }

    internal static bool CanSummonTo(PlayerId player, HexCoord target, TrueState state) =>
        Query.ResolveConnectedProducingTerrain(player, state).Contains(target)
        && Query.CanOccupyLevel(player, target, Level.Surface, state);
}
