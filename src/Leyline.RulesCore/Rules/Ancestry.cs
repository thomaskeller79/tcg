using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// ancestry.md: one parent-link structure over every domain, and its two walks — Payment (stop at
/// the first node holding the resource) and Controller (climb to the root: a Champion, or Neutral).
/// Only the Terrain → bonder edge can be inactive (D83): while no unknotted path connects the
/// terrain to its bonder, neither walk crosses it.
/// </summary>
public static class Ancestry
{
    /// <summary>The Champion permanent a player plays.</summary>
    public static Permanent? ChampionOf(this TrueState state, PlayerId player) =>
        state.Player(player).Champion is { } id ? state.Find<Permanent>(id) : null;

    /// <summary>Which player a Champion permanent belongs to.</summary>
    public static PlayerId? PlayerOfChampion(this TrueState state, ObjectId championId) =>
        state.Players.FirstOrDefault(p => p.Champion == championId)?.Id;

    /// <summary>The parent link a walk follows from this object, or null at a root.
    /// <paramref name="ignoreEdgeActivity"/> treats the Bond edge as always active — used only
    /// for deciding which side a Knotting permanent is on, where the live answer would depend on
    /// the knot itself (G23).</summary>
    public static ObjectId? ParentOf(this TrueState state, GameObject obj, bool ignoreEdgeActivity = false)
    {
        switch (obj)
        {
            case CardObject card:
                return state.Player(card.Owner).Champion;
            case TraceObject trace:
                return trace.Parent is { } tp && state.Exists(tp) ? tp : null;
            case Permanent p:
                switch (p.Kind)
                {
                    case PermanentKind.Champion:
                        return null;
                    case PermanentKind.Creature:
                    case PermanentKind.Companion:
                        return p.Parent is { } pp && state.Exists(pp) ? pp : null;
                    case PermanentKind.Terrain:
                        if (p.Parent is not { } bonder || !state.Exists(bonder))
                            return null;
                        return ignoreEdgeActivity || Network.IsFlowing(state, p) ? bonder : null;
                    case PermanentKind.Structure:
                    case PermanentKind.Remnant:
                        return p.Carrier is null && state.TerrainAt.TryGetValue(p.Hex, out var t) ? t : null;
                    case PermanentKind.Item:
                        return p.Carrier;
                }
                break;
        }
        return null;
    }

    /// <summary>Climb to the root: a Champion → that player; anything else → Neutral (null).</summary>
    public static PlayerId? Controller(this TrueState state, GameObject obj, bool ignoreEdgeActivity = false)
    {
        GameObject current = obj;
        for (var guard = 0; guard < 64; guard++)
        {
            if (current is Permanent { Kind: PermanentKind.Champion } champion)
                return state.PlayerOfChampion(champion.Id);
            if (state.ParentOf(current, ignoreEdgeActivity) is not { } parent)
                return null;
            current = state.Get<GameObject>(parent);
        }
        throw new InvalidOperationException($"Ancestry cycle at {obj.Id}.");
    }

    public static PlayerId? Controller(this TrueState state, ObjectId id) => state.Controller(state.Get<GameObject>(id));

    /// <summary>Payment walk for mana (D58, D74): the first Champion or Companion up the chain
    /// (inclusive), or null when the walk runs off the top.</summary>
    public static Permanent? ManaPayer(this TrueState state, GameObject obj)
    {
        GameObject current = obj;
        for (var guard = 0; guard < 64; guard++)
        {
            if (current is Permanent p && p.Kind.IsRoot() && p.Pool is not null)
                return p;
            if (state.ParentOf(current) is not { } parent)
                return null;
            current = state.Get<GameObject>(parent);
        }
        return null;
    }

    /// <summary>Payment walk for Activation Points and Life: held by every Actor, so it stops at
    /// the object itself; an Item or Terrain climbs to the first Actor (D58, D87).</summary>
    public static Permanent? ApPayer(this TrueState state, GameObject obj)
    {
        GameObject current = obj;
        for (var guard = 0; guard < 64; guard++)
        {
            if (current is Permanent p && p.Kind.IsActor())
                return p;
            if (state.ParentOf(current) is not { } parent)
                return null;
            current = state.Get<GameObject>(parent);
        }
        return null;
    }

    /// <summary>"Hand/Library access is held only by a Champion" — the walk for "you draw".</summary>
    public static PlayerId? HandHolder(this TrueState state, GameObject obj) => state.Controller(obj);

    /// <summary>D83: whoever makes this object's choices — its controller, otherwise its
    /// Behavior (represented as null + a Behavior), otherwise nobody (dormant).</summary>
    public static bool IsDormant(this TrueState state, Permanent p) =>
        state.Controller(p) is null && p.Behavior is null;
}
