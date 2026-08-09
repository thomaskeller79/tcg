using Leyline.RulesCore.Perception;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.DebugUi;

public static class DebugStateMapper
{
    public static DebugStateDto Map(TrueState state)
    {
        var network = Query.ResolveNetworkStatus(state);
        var cells = state.Board.AllCells.Select(c =>
        {
            network.TryGetValue(c.Coord, out var status);
            return new DebugCellDto(
                c.Coord, c.Terrain, c.Ground.Occupants.ToList(), c.Below.Occupants.ToList(), c.Above.Occupants.ToList(),
                network.ContainsKey(c.Coord) ? status.Owner : null, status.Producing);
        }).ToList();

        var actors = state.AllActors.Select(a => ToActorDto(a, state)).ToList();
        var mana = state.Players.Select(p => new PlayerManaView(p.Id, p.Mana)).ToList();
        var zones = state.Players.Select(p => new DebugZonesDto(p.Id, p.Library.ToList(), p.Hand.ToList())).ToList();

        var combats = state.ActiveCombats.Select(c => new ActiveCombatDto(
            c.Id,
            c.Attacker,
            c.TargetHex,
            c.Defenders.ToList(),
            c.DamageAssignment?.Select(kv => new DamageAssignmentEntryDto(kv.Key, kv.Value)).ToList(),
            c.UndefendedTarget,
            c.Phase)).ToList();

        var window = state.ActiveWindow is { } w
            ? new PriorityWindowDto(w.Kind, w.Context, w.Order, w.CurrentPriority)
            : null;

        return new DebugStateDto(state.TurnNumber, state.ActivePlayer, state.CurrentPhase.Id, cells, actors, mana, zones, state.Winner, combats, window, BuildCardCatalog(state));
    }

    /// <summary>Every card definition referenced anywhere in the match right now (hand, library,
    /// or on the board) — enough for the client to show a hand card's mana cost and effect text.</summary>
    private static IReadOnlyList<CardCatalogEntryDto> BuildCardCatalog(TrueState state)
    {
        var ids = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var p in state.Players)
        {
            foreach (var id in p.Hand) ids.Add(id.Value);
            foreach (var id in p.Library) ids.Add(id.Value);
        }
        foreach (var a in state.AllActors.OfType<IHasCardDefinition>())
            ids.Add(a.Definition.Value);

        return ids.Select(idValue =>
        {
            var def = state.Content.Get(new CardDefinitionId(idValue));
            return new CardCatalogEntryDto(def.Id, def.Name, def.Type.ToString(), def.ManaCost, def.Attack, def.Life, def.MaxAp, def.EffectId, def.EffectAmount);
        }).ToList();
    }

    private static DebugActorDto ToActorDto(ActorState actor, TrueState state)
    {
        var (name, kind, maxLife) = actor is IHasCardDefinition d
            ? (state.Content.Get(d.Definition).Name, actor is ChampionState ? "Champion" : "Creature", state.Content.Get(d.Definition).Life)
            : ("?", "Unknown", actor.Life);
        return new DebugActorDto(
            actor.Id, actor.Owner, name, kind,
            Query.ResolveAttack(actor.Id, state), actor.Life, maxLife, actor.CurrentAp, Query.ResolveMaxAp(actor.Id, state),
            Query.ResolveAbilityIds(actor.Id, state).OrderBy(a => a, StringComparer.Ordinal).ToList(),
            actor.Position, actor.Layer);
    }
}
