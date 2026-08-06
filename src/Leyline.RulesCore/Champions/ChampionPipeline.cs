using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Champions;

/// <summary>
/// D9's second Channel option: act as a creature. Grants the Champion its AP for the turn
/// and reuses the exact same Move/Attack machinery as CreatureState (design-economy.md:
/// "one movement/combat model for every entity") — this command only grants AP; Move/Attack
/// need zero changes to recognize a Champion as a legal actor.
/// </summary>
public static class ChampionPipeline
{
    public static CommandResult ChannelAct(TrueState state, EventPipeline pipeline, ChannelActCommand cmd)
    {
        var champion = state.ActorsOwnedBy(cmd.Actor).OfType<ChampionState>().FirstOrDefault();
        if (champion is null)
            return CommandResult.Reject("You have no Champion.");
        if (champion.ChannelUsedThisTurn)
            return CommandResult.Reject("Channel already used this turn.");

        var events = new List<IEvent>();
        events.AddRange(pipeline.Process(new ApChangeIntent(champion.Id, Query.ResolveMaxAp(champion.Id, state)), state));
        events.AddRange(pipeline.Process(new ChannelUsedIntent(cmd.Actor), state));
        return CommandResult.Accept(events);
    }
}
