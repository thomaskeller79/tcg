using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Perception;

/// <summary>
/// (TrueState, observer) → View. M1's only redaction rule: below-layer occupants not owned
/// by the observer, and not "located" (D19), are hidden — via Query.IsVisibleTo, the same
/// visibility rule Combat's targeting consults (perception is just another query axis).
/// </summary>
public static class ViewProjector
{
    public static View Project(TrueState state, PlayerId observer)
    {
        var network = Query.ResolveNetworkStatus(state);
        var cells = state.Board.AllCells.Select(c =>
        {
            network.TryGetValue(c.Coord, out var status);
            return new CellView(
                c.Coord,
                c.Terrain,
                c.Ground.Occupants.ToList(),
                VisibleOccupants(c.Below, observer, state),
                c.Above.Occupants.ToList(),
                network.ContainsKey(c.Coord) ? status.Owner : null,
                status.Producing);
        }).ToList();

        var actors = state.AllActors
            .Where(a => Query.IsVisibleTo(a.Id, observer, state))
            .Select(a => ToActorView(a, state))
            .ToList();

        var mana = state.Players.Select(p => new PlayerManaView(p.Id, p.Id == observer ? p.Mana : null)).ToList();
        var hands = state.Players
            .Select(p => new HandView(p.Id, p.Hand.Count, p.Id == observer ? p.Hand.ToList() : null))
            .ToList();

        return new View(
            observer,
            state.TurnNumber,
            state.ActivePlayer,
            state.CurrentPhase.Id,
            cells,
            actors,
            mana,
            hands,
            state.Winner,
            state.ActiveWindow is { } window && window.CurrentPriority == observer);
    }

    public static IReadOnlyList<ObservedEvent> ProjectEvents(IReadOnlyList<IEvent> trueEvents, PlayerId observer, TrueState state) =>
        trueEvents.Select(e => new ObservedEvent(e)).ToList();

    private static IReadOnlyList<ActorId> VisibleOccupants(LayerOccupancy layer, PlayerId observer, TrueState state) =>
        layer.Occupants.Where(id => Query.IsVisibleTo(id, observer, state)).ToList();

    private static ActorView ToActorView(ActorState actor, TrueState state)
    {
        var (name, kind, maxLife) = actor is IHasCardDefinition d
            ? (state.Content.Get(d.Definition).Name, actor is ChampionState ? "Champion" : "Creature", state.Content.Get(d.Definition).Life)
            : ("?", "Unknown", actor.Life);
        return new ActorView(
            actor.Id, actor.Owner, name, kind,
            Query.ResolveAttack(actor.Id, state), actor.Life, maxLife, actor.CurrentAp, Query.ResolveMaxAp(actor.Id, state),
            Query.ResolveAbilityIds(actor.Id, state).OrderBy(a => a, StringComparer.Ordinal).ToList(),
            actor.Position, actor.Layer);
    }
}
