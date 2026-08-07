using Leyline.RulesCore.Champions;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Terrain;

/// <summary>D8/D9: Bond is a default ability the Champion has (ChampionActionIds.Bond, gated
/// through AbilityIds like Move/Attack), costing `2*AP` — 2 AP plus D9's `*` once-per-turn
/// flavor (Query.CanUseOncePerTurnAction), independent checks per design-economy.md.</summary>
public static class TerrainPipeline
{
    public static CommandResult Bond(TrueState state, EventPipeline pipeline, BondTerrainCommand cmd)
    {
        var champion = state.ActorsOwnedBy(cmd.Actor).OfType<ChampionState>().FirstOrDefault();
        if (champion is null)
            return CommandResult.Reject("You have no Champion.");
        if (!CanBond(champion, state))
            return CommandResult.Reject("Bond is not available right now.");
        if (!Query.CanBondTo(cmd.Actor, cmd.Target, state))
            return CommandResult.Reject("Illegal bond target.");

        var cost = Query.ResolveBondCost(champion.Id, state);
        var events = new List<IEvent>();
        events.AddRange(pipeline.Process(new BondTerrainIntent(cmd.Actor, cmd.Target), state));
        events.AddRange(pipeline.Process(new ApChangeIntent(champion.Id, cost.Apply(champion.CurrentAp)), state));
        events.AddRange(pipeline.Process(new OncePerTurnActionUsedIntent(champion.Id, ChampionActionIds.Bond), state));
        return CommandResult.Accept(events);
    }

    public static IReadOnlyList<BondTerrainCommand> LegalBonds(TrueState state, PlayerId player)
    {
        var champion = state.ActorsOwnedBy(player).OfType<ChampionState>().FirstOrDefault();
        if (champion is null || !CanBond(champion, state))
            return [];

        return state.Board.AllCells
            .Where(cell => Query.CanBondTo(player, cell.Coord, state))
            .Select(cell => new BondTerrainCommand(player, cell.Coord))
            .ToList();
    }

    private static bool CanBond(ChampionState champion, TrueState state) =>
        Query.ResolveAbilityIds(champion.Id, state).Contains(ChampionActionIds.Bond)
        && Query.CanUseOncePerTurnAction(champion.Id, ChampionActionIds.Bond, state)
        && Query.ResolveBondCost(champion.Id, state).IsAffordable(champion.CurrentAp);
}
