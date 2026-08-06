using Leyline.RulesCore.Abilities;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Queries;

/// <summary>
/// Every derived rule value goes through here — see IQueryModifier's doc comment. M1 ships
/// zero real modifiers, so every call below currently just returns its baseline, but the
/// call sites (Combat, legal-command enumeration, ...) never read a raw stat directly.
/// </summary>
public static class Query
{
    private static TResult Fold<TResult>(string queryKind, ActorId? subject, TResult baseline, TrueState state)
    {
        var ctx = new QueryContext(queryKind, subject);
        var result = baseline;
        foreach (var modifier in state.ActiveModifiers
                     .OfType<IQueryModifier<TResult>>()
                     .Where(m => m.QueryKind == queryKind && m.AppliesTo(ctx, state)))
        {
            result = modifier.Resolve(ctx, result, state);
        }
        return result;
    }

    public static IReadOnlySet<string> ResolveAbilityIds(ActorId actor, TrueState state)
    {
        var baseline = state.GetActor(actor) switch
        {
            CreatureState c => (IReadOnlySet<string>)new HashSet<string>(state.Content.Get(c.Definition).AbilityIds),
            ChampionState c => new HashSet<string>(state.Content.Get(c.Definition).AbilityIds),
            _ => new HashSet<string>(),
        };
        return Fold("AbilityIds", actor, baseline, state);
    }

    public static int ResolveMaxAp(ActorId actor, TrueState state)
    {
        var baseline = state.GetActor(actor) switch
        {
            CreatureState c => state.Content.Get(c.Definition).MaxAp,
            ChampionState c => state.Content.Get(c.Definition).MaxAp,
            _ => 0,
        };
        return Fold("MaxAp", actor, baseline, state);
    }

    public static int ResolveAttack(ActorId actor, TrueState state)
    {
        var baseline = state.GetActor(actor) switch
        {
            CreatureState c => state.Content.Get(c.Definition).Attack,
            ChampionState c => state.Content.Get(c.Definition).Attack,
            _ => 0,
        };
        return Fold("Attack", actor, baseline, state);
    }

    public static ApCost ResolveMoveCost(ActorId actor, HexCoord destination, TrueState state)
    {
        var cell = state.Board.TryGetCell(destination);
        var baseline = ApCost.Fixed(cell?.MoveCost ?? 1);
        return Fold("MoveCost", actor, baseline, state);
    }

    public static ApCost ResolveAttackCost(ActorId actor, TrueState state) =>
        Fold("AttackCost", actor, ApCost.Exhaust(3), state);

    public static bool CanDefend(ActorId actor, TrueState state)
    {
        var defender = state.GetActor(actor);
        var baseline = state.Config.DefendRule switch
        {
            DefendRuleVariant.Exhaust => defender.CurrentAp >= 1,
            DefendRuleVariant.DeleteDefendOnce => true,
            _ => throw new ArgumentOutOfRangeException(),
        };
        return Fold("CanDefend", actor, baseline, state);
    }

    public static ApCost ResolveDefendCost(ActorId actor, TrueState state)
    {
        var baseline = state.Config.DefendRule switch
        {
            DefendRuleVariant.Exhaust => ApCost.Exhaust(1),
            DefendRuleVariant.DeleteDefendOnce => ApCost.Fixed(0),
            _ => throw new ArgumentOutOfRangeException(),
        };
        return Fold("DefendCost", actor, baseline, state);
    }

    public static IReadOnlyList<HexCoord> ResolveLegalMoveTargets(ActorId actor, TrueState state)
    {
        var actorState = state.GetActor(actor);
        if (!ResolveAbilityIds(actor, state).Contains(CoreAbilities.Move))
            return [];

        var targets = new List<HexCoord>();
        foreach (var coord in state.Board.AdjacentCoords(actorState.Position))
        {
            var cell = state.Board.GetCell(coord);
            if (!cell.LayerOf(actorState.Layer).HasRoom)
                continue;
            if (ResolveMoveCost(actor, coord, state).IsAffordable(actorState.CurrentAp))
                targets.Add(coord);
        }
        return targets.OrderBy(c => c).ToList();
    }

    /// <summary>
    /// M1 scope: Ground + Below only, adjacency-range only (Above/flying and Ranged aren't
    /// implemented). D19's initiation-legality matrix, reduced to the layers that exist here.
    /// </summary>
    public static IReadOnlyList<HexCoord> ResolveLegalAttackTargets(ActorId actor, TrueState state)
    {
        var actorState = state.GetActor(actor);
        if (!ResolveAbilityIds(actor, state).Contains(CoreAbilities.Attack))
            return [];
        if (!ResolveAttackCost(actor, state).IsAffordable(actorState.CurrentAp))
            return [];

        var targets = new List<HexCoord>();
        foreach (var coord in state.Board.AdjacentCoords(actorState.Position))
        {
            var cell = state.Board.GetCell(coord);
            var hasValidEnemyTarget = cell.GroundAndBelowOccupants
                .Select(state.GetActor)
                .Any(o => o.Owner != actorState.Owner
                          && CanInitiateAttack(actorState.Layer, o.Layer)
                          && IsVisibleTo(o.Id, actorState.Owner, state));
            if (hasValidEnemyTarget)
                targets.Add(coord);
        }
        return targets.OrderBy(c => c).ToList();
    }

    /// <summary>D19 initiation-legality matrix, reduced to Ground/Below (no Flyer type in M1).
    /// Sub→Ground is explicitly marked "tentative, balance" in the source decision.</summary>
    private static bool CanInitiateAttack(Layer attacker, Layer target) => (attacker, target) switch
    {
        (Layer.Ground, Layer.Ground) => true,
        (Layer.Ground, Layer.Below) => true, // gated separately by IsVisibleTo ("only if located")
        (Layer.Below, Layer.Ground) => true, // D19: tentative, balance
        (Layer.Below, Layer.Below) => true, // gated separately by IsVisibleTo
        _ => false, // Above/Flyer not implemented in M1
    };

    /// <summary>
    /// D12/D19: the below layer is hidden by default. Perception is just another query axis
    /// (design-asymmetric-information.md) — this is the one rule Perception's ViewProjector
    /// and Combat's targeting both consult, so there's exactly one definition of "can you see
    /// this" in the engine.
    /// </summary>
    public static bool IsVisibleTo(ActorId subject, PlayerId observer, TrueState state)
    {
        var actor = state.GetActor(subject);
        var baseline = actor.Layer != Layer.Below || actor.Owner == observer || actor.Located;
        return Fold("Visibility", subject, baseline, state);
    }

    /// <summary>
    /// D8: the set of this player's bonded terrain currently producing mana — reachable from
    /// their Champion's current position through a chain of bonded, enemy-free cells.
    /// Recomputed fresh on every call (no incremental cache — the board is tiny). An enemy
    /// occupying any cell on the only path pauses everything behind it (positional, reversible
    /// denial — D8), without ever touching the permanent Bonded set itself.
    /// </summary>
    public static IReadOnlySet<HexCoord> ResolveConnectedProducingTerrain(PlayerId player, TrueState state)
    {
        var champion = FindChampion(player, state);
        if (champion is null)
            return new SortedSet<HexCoord>();

        return state.Board.ReachableFrom(
            champion.Position,
            coord => champion.Network.Bonded.Contains(coord) && !IsEnemyOccupied(state, player, coord));
    }

    /// <summary>Single generic mana unit per connected/producing node (locked M1 scope — no 8-color system).</summary>
    public static int ResolveManaProduction(PlayerId player, TrueState state) =>
        Fold("ManaProduction", null, ResolveConnectedProducingTerrain(player, state).Count, state);

    /// <summary>
    /// D8: a target is bondable if it's an unbonded, non-enemy-occupied terrain cell adjacent
    /// either to the Champion directly or to the currently-producing network. Deliberately
    /// reuses ResolveConnectedProducingTerrain's enemy-free-path rule for new bonding too —
    /// one BFS rule instead of two (see the M1 plan's flagged interpretation call).
    /// </summary>
    public static bool CanBondTo(PlayerId player, HexCoord target, TrueState state)
    {
        var cell = state.Board.TryGetCell(target);
        if (cell?.Terrain is null)
            return false;

        var champion = FindChampion(player, state);
        if (champion is null || champion.Network.Bonded.Contains(target) || IsEnemyOccupied(state, player, target))
            return false;

        if (state.Board.AdjacentCoords(champion.Position).Contains(target))
            return true;

        var producing = ResolveConnectedProducingTerrain(player, state);
        return state.Board.AdjacentCoords(target).Any(producing.Contains);
    }

    private static ChampionState? FindChampion(PlayerId player, TrueState state) =>
        state.AllActors.OfType<ChampionState>().FirstOrDefault(c => c.Owner == player);

    private static bool IsEnemyOccupied(TrueState state, PlayerId player, HexCoord coord)
    {
        var cell = state.Board.GetCell(coord);
        return cell.Ground.Occupants.Concat(cell.Below.Occupants).Concat(cell.Above.Occupants)
            .Select(state.GetActor)
            .Any(a => a.Owner != player);
    }
}
