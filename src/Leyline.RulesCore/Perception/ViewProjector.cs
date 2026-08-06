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
        var cells = state.Board.AllCells.Select(c => new CellView(
            c.Coord,
            c.Terrain,
            c.Ground.Occupants.ToList(),
            VisibleOccupants(c.Below, observer, state),
            c.Above.Occupants.ToList()
        )).ToList();

        var actors = state.AllActors
            .Where(a => Query.IsVisibleTo(a.Id, observer, state))
            .Select(a => new ActorView(a.Id, a.Owner, a.Life, a.CurrentAp, a.Position, a.Layer))
            .ToList();

        return new View(
            observer,
            state.TurnNumber,
            state.ActivePlayer,
            state.CurrentPhase.Id,
            cells,
            actors,
            state.ActiveWindow is { } window && window.CurrentPriority == observer);
    }

    public static IReadOnlyList<ObservedEvent> ProjectEvents(IReadOnlyList<IEvent> trueEvents, PlayerId observer, TrueState state) =>
        trueEvents.Select(e => new ObservedEvent(e)).ToList();

    private static IReadOnlyList<ActorId> VisibleOccupants(LayerOccupancy layer, PlayerId observer, TrueState state) =>
        layer.Occupants.Where(id => Query.IsVisibleTo(id, observer, state)).ToList();
}
