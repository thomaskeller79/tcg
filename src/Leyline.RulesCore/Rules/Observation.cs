using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// D152: the terrains' observation abilities, checked after every instruction. Where a Champion
/// observes a permanent, its location belief becomes true; where it observed it before and doesn't
/// now, the belief becomes `?` (sight lost); where it observes the Slice it believes the permanent
/// is in and the permanent isn't there, `?` too (seen missing). Otherwise a belief stays, so a move
/// nobody sees keeps the opponent's belief. Observation corrects silently: no contradiction.
/// </summary>
public static class Observation
{
    public static void Update(TrueState state)
    {
        foreach (var who in state.Players.Select(p => p.Id))
        {
            foreach (var p in state.Permanents.Where(p => p.Kind != PermanentKind.Terrain && p.Carrier is null))
            {
                var b = state.Beliefs.Location(who, p.Id);
                var observes = state.CanSee(who, p);
                if (observes)
                    (b.Hex, b.Slice) = (p.Hex, p.Slice);
                else if (b.Observed)
                    b.Hex = null;
                else if (b.Hex is { } h && state.ObservesSlice(who, h, b.Slice) && (h != p.Hex || b.Slice != p.Slice))
                    b.Hex = null;
                b.Observed = observes;
            }
        }
    }

    /// <summary>A new permanent enters every belief (no hidden creation, D155): at its location for
    /// those who observe it, else at `?`. A Remnant copies its creature's beliefs once (D149).</summary>
    public static void Enter(TrueState state, Permanent p, Permanent? predecessor = null)
    {
        if (p.Kind == PermanentKind.Terrain)
            return;
        foreach (var who in state.Players.Select(x => x.Id))
        {
            var b = state.Beliefs.Location(who, p.Id);
            var known = predecessor is not null ? state.Beliefs.Location(who, predecessor.Id).Hex is not null : state.CanSee(who, p);
            b.Hex = known ? p.Hex : null;
            b.Slice = p.Slice;
            b.Observed = state.CanSee(who, p);
        }
    }
}
