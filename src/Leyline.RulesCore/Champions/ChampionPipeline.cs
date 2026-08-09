using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Champions;

/// <summary>D9: Draw is a default ability the Champion has (ChampionActionIds.Draw, gated
/// through AbilityIds like Move/Attack/Bond), costing `5*AP` — mirrors TerrainPipeline.Bond's
/// shape exactly (AP cost + once-per-turn gate, independent checks per design-economy.md).</summary>
public static class ChampionPipeline
{
    public static CommandResult DrawCard(TrueState state, EventPipeline pipeline, DrawCardCommand cmd)
    {
        var champion = state.ActorsOwnedBy(cmd.Actor).OfType<ChampionState>().FirstOrDefault();
        if (champion is null)
            return CommandResult.Reject("You have no Champion.");
        if (!CanDraw(champion, cmd.Actor, state))
            return CommandResult.Reject("Draw is not available right now.");

        var player = state.Players.First(p => p.Id == cmd.Actor);
        var card = player.Library[0];
        var cost = Query.ResolveDrawCost(champion.Id, state);

        var events = new List<IEvent>();
        events.AddRange(pipeline.Process(new CardDrawnIntent(cmd.Actor, card), state));
        events.AddRange(pipeline.Process(new ApChangeIntent(champion.Id, cost.Apply(champion.CurrentAp)), state));
        events.AddRange(pipeline.Process(new OncePerTurnActionUsedIntent(champion.Id, ChampionActionIds.Draw), state));
        return CommandResult.Accept(events);
    }

    public static IReadOnlyList<DrawCardCommand> LegalDraws(TrueState state, PlayerId player)
    {
        var champion = state.ActorsOwnedBy(player).OfType<ChampionState>().FirstOrDefault();
        return champion is not null && CanDraw(champion, player, state)
            ? [new DrawCardCommand(player)]
            : [];
    }

    private static bool CanDraw(ChampionState champion, PlayerId player, TrueState state)
    {
        var libraryHasCards = state.Players.First(p => p.Id == player).Library.Count > 0;
        return libraryHasCards
            && Query.ResolveAbilityIds(champion.Id, state).Contains(ChampionActionIds.Draw)
            && Query.CanUseOncePerTurnAction(champion.Id, ChampionActionIds.Draw, state)
            && Query.ResolveDrawCost(champion.Id, state).IsAffordable(champion.CurrentAp);
    }

    /// <summary>D9 (under test, 2026-08-08): free (0AP), unlimited — self-limiting anyway,
    /// since collapsing an already-empty network is never legal (nothing left to drop).</summary>
    public static CommandResult CollapseNetwork(TrueState state, EventPipeline pipeline, CollapseNetworkCommand cmd)
    {
        var champion = state.ActorsOwnedBy(cmd.Actor).OfType<ChampionState>().FirstOrDefault();
        if (champion is null)
            return CommandResult.Reject("You have no Champion.");
        if (!CanCollapse(champion, state))
            return CommandResult.Reject("There is no network to collapse.");

        var cost = Query.ResolveCollapseCost(champion.Id, state);
        var events = new List<IEvent>();
        events.AddRange(pipeline.Process(new NetworkCollapsedIntent(champion.Id), state));
        events.AddRange(pipeline.Process(new ApChangeIntent(champion.Id, cost.Apply(champion.CurrentAp)), state));
        return CommandResult.Accept(events);
    }

    public static IReadOnlyList<CollapseNetworkCommand> LegalCollapses(TrueState state, PlayerId player)
    {
        var champion = state.ActorsOwnedBy(player).OfType<ChampionState>().FirstOrDefault();
        return champion is not null && CanCollapse(champion, state)
            ? [new CollapseNetworkCommand(player)]
            : [];
    }

    private static bool CanCollapse(ChampionState champion, TrueState state) =>
        champion.Network.Bonded.Count > 0
        && Query.ResolveAbilityIds(champion.Id, state).Contains(ChampionActionIds.CollapseNetwork);
}
