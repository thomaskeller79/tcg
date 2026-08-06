using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Terrain;

/// <summary>D8/D9: bonding a terrain node, gated by the same Channel-used-this-turn flag
/// Slice 4's "act as a creature" option will also gate.</summary>
public static class TerrainPipeline
{
    public static CommandResult Bond(TrueState state, EventPipeline pipeline, BondTerrainCommand cmd)
    {
        var champion = state.ActorsOwnedBy(cmd.Actor).OfType<ChampionState>().FirstOrDefault();
        if (champion is null)
            return CommandResult.Reject("You have no Champion.");
        if (champion.ChannelUsedThisTurn)
            return CommandResult.Reject("Channel already used this turn.");
        if (!Query.CanBondTo(cmd.Actor, cmd.Target, state))
            return CommandResult.Reject("Illegal bond target.");

        var events = new List<IEvent>();
        events.AddRange(pipeline.Process(new BondTerrainIntent(cmd.Actor, cmd.Target), state));
        events.AddRange(pipeline.Process(new ChannelUsedIntent(cmd.Actor), state));
        return CommandResult.Accept(events);
    }

    public static IReadOnlyList<BondTerrainCommand> LegalBonds(TrueState state, PlayerId player)
    {
        var champion = state.ActorsOwnedBy(player).OfType<ChampionState>().FirstOrDefault();
        if (champion is null || champion.ChannelUsedThisTurn)
            return [];

        return state.Board.AllCells
            .Where(cell => Query.CanBondTo(player, cell.Coord, state))
            .Select(cell => new BondTerrainCommand(player, cell.Coord))
            .ToList();
    }
}
