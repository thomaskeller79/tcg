using Leyline.RulesCore.Abilities;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Queries;

public readonly record struct TerrainNetworkStatus(PlayerId Owner, bool Producing);

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
        IReadOnlySet<string> baseline = state.GetActor(actor) is IHasCardDefinition d
            ? new HashSet<string>(state.Content.Get(d.Definition).AbilityIds)
            : new HashSet<string>();
        return Fold("AbilityIds", actor, baseline, state);
    }

    public static int ResolveMaxAp(ActorId actor, TrueState state)
    {
        var baseline = state.GetActor(actor) is IHasCardDefinition d ? state.Content.Get(d.Definition).MaxAp : 0;
        return Fold("MaxAp", actor, baseline, state);
    }

    public static int ResolveAttack(ActorId actor, TrueState state)
    {
        var baseline = state.GetActor(actor) is IHasCardDefinition d ? state.Content.Get(d.Definition).Attack : 0;
        return Fold("Attack", actor, baseline, state);
    }

    /// <summary>
    /// D9 (under test, tightened 2026-08-08): a Champion currently connected to its network is
    /// "rooted" — moving costs `2AP` (destination legality is separately restricted to hexes
    /// that are themselves part of the bonded network, see ResolveLegalMoveTargets — a rooted
    /// Champion can reposition within its own territory but can never step outside it). Once
    /// disconnected — never bonded, fully blockaded, or the network was deliberately Collapsed
    /// — it moves freely like any creature, `1AP`, no restriction. This supersedes the flat
    /// terrain-MoveCost baseline for the Champion specifically (still used by every other actor).
    /// </summary>
    public static ApCost ResolveMoveCost(ActorId actor, HexCoord destination, TrueState state)
    {
        ApCost baseline;
        if (state.GetActor(actor) is ChampionState champion)
        {
            var connected = ResolveConnectedProducingTerrain(champion.Owner, state).Count > 0;
            baseline = ApCost.Fixed(connected ? 2 : 1);
        }
        else
        {
            var cell = state.Board.TryGetCell(destination);
            baseline = ApCost.Fixed(cell?.MoveCost ?? 1);
        }
        return Fold("MoveCost", actor, baseline, state);
    }

    /// <summary>D9 (under test): the Champion's free (0AP) Collapse-the-network ability.</summary>
    public static ApCost ResolveCollapseCost(ActorId actor, TrueState state) =>
        Fold("CollapseCost", actor, ApCost.Fixed(0), state);

    public static ApCost ResolveAttackCost(ActorId actor, TrueState state) =>
        Fold("AttackCost", actor, ApCost.Exhaust(3), state);

    /// <summary>D15 (resolved 2026-08-09): defending costs `0*AP` — free, but at most once per
    /// turn per actor (Query.CanUseOncePerTurnAction, same `*` flavor as Bond/Draw), completely
    /// decoupled from remaining AP. Replaces the earlier Exhaust/DeleteDefendOnce config toggle:
    /// Exhaust (defending costs `1!AP`) created a real bug — an actor that spent its whole turn
    /// (e.g. by attacking, itself `!`-costed) was left unable to defend for the *opponent's
    /// entire following turn* (AP only refreshes on its own controller's Beginning phase),
    /// punishing whoever attacked first. `0*AP` fixes that (Defend never checks AP at all) while
    /// keeping a real per-turn limit (unlike a flat "always free, unlimited" rule) — a card can
    /// still deliberately spend a creature's Defend for the turn as a side effect of a strong
    /// ability (emit OncePerTurnActionUsedIntent(actor, CoreAbilities.Defend) from that
    /// ability's own effect), the MTG "tap cost" flavor, without that being a base-rule default.</summary>
    public static bool CanDefend(ActorId actor, TrueState state) =>
        Fold("CanDefend", actor, CanUseOncePerTurnAction(actor, CoreAbilities.Defend, state), state);

    public static ApCost ResolveDefendCost(ActorId actor, TrueState state) =>
        Fold("DefendCost", actor, ApCost.Fixed(0), state);

    /// <summary>D9's `2*AP` Bond cost — the AP half of the `*` flavor; the once-per-turn half
    /// is CanUseOncePerTurnAction below (they're independent, see design-economy.md's `*`
    /// notation).</summary>
    public static ApCost ResolveBondCost(ActorId actor, TrueState state) =>
        Fold("BondCost", actor, ApCost.Fixed(2), state);

    /// <summary>D9's `5*AP` Draw cost — same `*` shape as Bond, see ResolveBondCost.</summary>
    public static ApCost ResolveDrawCost(ActorId actor, TrueState state) =>
        Fold("DrawCost", actor, ApCost.Fixed(5), state);

    /// <summary>The once-per-turn half of the `*` cost flavor: true until this actionId has
    /// been spent this turn, regardless of AP remaining or refilled since.</summary>
    public static bool CanUseOncePerTurnAction(ActorId actor, string actionId, TrueState state)
    {
        var baseline = !state.GetActor(actor).OncePerTurnActionsUsed.Contains(actionId);
        return Fold($"OncePerTurn:{actionId}", actor, baseline, state);
    }

    public static IReadOnlyList<HexCoord> ResolveLegalMoveTargets(ActorId actor, TrueState state)
    {
        var actorState = state.GetActor(actor);
        if (!ResolveAbilityIds(actor, state).Contains(CoreAbilities.Move))
            return [];

        // D9 (under test, tightened 2026-08-08): a Champion currently connected to its network
        // may only step onto hexes that are themselves part of that network — it's confined to
        // its own bonded territory until it Collapses. Not a concern for creatures, or for a
        // Champion that's already disconnected (nothing to protect).
        var mustStayConnected = actorState is ChampionState champion
            && ResolveConnectedProducingTerrain(champion.Owner, state).Count > 0;

        var targets = new List<HexCoord>();
        foreach (var coord in state.Board.AdjacentCoords(actorState.Position))
        {
            if (!CanOccupyLayer(actorState.Owner, coord, actorState.Layer, state))
                continue;
            if (mustStayConnected && !WouldStayConnected(actorState.Owner, coord, state))
                continue;
            if (ResolveMoveCost(actor, coord, state).IsAffordable(actorState.CurrentAp))
                targets.Add(coord);
        }
        return targets.OrderBy(c => c).ToList();
    }

    /// <summary>D9 (under test, tightened 2026-08-08): is `candidatePosition` itself one of the
    /// Champion's own bonded, enemy-free cells? A rooted Champion may reposition anywhere
    /// within its bonded territory, but may never step onto ground it hasn't claimed — that
    /// would mean walking "outside the network" while still connected, which the tightened rule
    /// disallows outright (Collapse first, then move freely).</summary>
    private static bool WouldStayConnected(PlayerId player, HexCoord candidatePosition, TrueState state)
    {
        var champion = FindChampion(player, state);
        return champion is not null && IsBondedAndReachable(champion, player, candidatePosition, state);
    }

    /// <summary>
    /// D12: a layer's capacity-3 room is for guarding allies (D4's "declare defenders"
    /// gang-up) — it was never meant to let opposing creatures share a contested hex. A layer
    /// with any enemy occupant has no room for you, regardless of raw capacity; used by both
    /// movement and (future) summoning legality, so there's one definition of "can I stand
    /// here" in the engine.
    /// </summary>
    public static bool CanOccupyLayer(PlayerId player, HexCoord target, Layer layer, TrueState state)
    {
        var cell = state.Board.TryGetCell(target);
        if (cell is null)
            return false;

        var occupancy = cell.LayerOf(layer);
        return occupancy.HasRoom && occupancy.Occupants.All(id => state.GetActor(id).Owner == player);
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
    /// their Champion's current position through a chain of bonded, enemy-free cells, PLUS the
    /// Champion's own tile if that's bonded (trivially "reachable" at distance 0 — the shared
    /// BFS helper only tests neighbors of the root, so the root itself needs this explicit
    /// check; see IsBondedAndReachable). Recomputed fresh on every call (no incremental cache —
    /// the board is tiny). An enemy occupying any cell on the only path pauses everything behind
    /// it (positional, reversible denial — D8), without ever touching the permanent Bonded set
    /// itself.
    /// </summary>
    public static IReadOnlySet<HexCoord> ResolveConnectedProducingTerrain(PlayerId player, TrueState state)
    {
        var champion = FindChampion(player, state);
        if (champion is null)
            return new SortedSet<HexCoord>();

        var reachable = (SortedSet<HexCoord>)state.Board.ReachableFrom(
            champion.Position,
            coord => champion.Network.Bonded.Contains(coord) && !IsEnemyOccupied(state, player, coord));

        if (IsBondedAndReachable(champion, player, champion.Position, state))
            reachable.Add(champion.Position);

        return reachable;
    }

    /// <summary>Is `coord` itself one of the Champion's bonded, enemy-free cells? Standing
    /// directly on a bonded tile counts as "reachable" (distance 0) even though the shared BFS
    /// (Board.ReachableFrom) only tests a root's neighbors, never the root itself.</summary>
    private static bool IsBondedAndReachable(ChampionState champion, PlayerId player, HexCoord coord, TrueState state) =>
        champion.Network.Bonded.Contains(coord) && !IsEnemyOccupied(state, player, coord);

    /// <summary>Single generic mana unit per connected/producing node (locked M1 scope — no 8-color system).</summary>
    public static int ResolveManaProduction(PlayerId player, TrueState state) =>
        Fold("ManaProduction", null, ResolveConnectedProducingTerrain(player, state).Count, state);

    /// <summary>
    /// D8 (revised, under test 2026-08-08): a target is bondable if it's unbonded and not
    /// enemy-occupied — no terrain-type restriction; a Champion may bond any hex on the board.
    /// Before any network exists (nothing bonded yet), the *only* legal target is the Champion's
    /// own current tile — the network must be rooted at home before it can spread. Once a
    /// network exists, a target is bondable if it's adjacent either to the Champion directly or
    /// to the currently-producing network (deliberately reuses ResolveConnectedProducingTerrain's
    /// enemy-free-path rule — one BFS rule instead of two, see the M1 plan's flagged
    /// interpretation call).
    /// </summary>
    public static bool CanBondTo(PlayerId player, HexCoord target, TrueState state)
    {
        var cell = state.Board.TryGetCell(target);
        if (cell is null)
            return false;

        var champion = FindChampion(player, state);
        if (champion is null || champion.Network.Bonded.Contains(target) || IsEnemyOccupied(state, player, target))
            return false;

        if (champion.Network.Bonded.Count == 0)
            return target == champion.Position;

        if (state.Board.AdjacentCoords(champion.Position).Contains(target))
            return true;

        var producing = ResolveConnectedProducingTerrain(player, state);
        return state.Board.AdjacentCoords(target).Any(producing.Contains);
    }

    /// <summary>D8: mana network topology is public information (needed for positional
    /// denial) — every bonded cell across both players, tagged with who bonded it and whether
    /// it's currently producing (vs. reversibly paused by enemy blockage,
    /// ResolveConnectedProducingTerrain). Bonding is exclusive and permanent (no un-bond), so
    /// a cell has at most one owner. Used to render both players' networks on the map.</summary>
    public static IReadOnlyDictionary<HexCoord, TerrainNetworkStatus> ResolveNetworkStatus(TrueState state)
    {
        var result = new Dictionary<HexCoord, TerrainNetworkStatus>();
        foreach (var player in state.Players)
        {
            var champion = FindChampion(player.Id, state);
            if (champion is null)
                continue;

            var producing = ResolveConnectedProducingTerrain(player.Id, state);
            foreach (var coord in champion.Network.Bonded)
                result[coord] = new TerrainNetworkStatus(player.Id, producing.Contains(coord));
        }
        return result;
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
