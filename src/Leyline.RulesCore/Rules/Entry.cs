using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// Entering a Slice (D86, D119, D132): a creature-type permanent may enter only if there is a free
/// place and every occupant shares its controller. Only creature-type occupants of that one Slice
/// count — not Structures, Remnants, loose Items, or other Slices. Declaring checks both against
/// the actor's own view (<c>observer</c>); resolving checks only the capacity, against true state,
/// so a hidden occupant of another controller never blocks and the Slice becomes mixed.
/// </summary>
public static class Entry
{
    /// <summary>Whether a creature-type permanent controlled by <paramref name="controller"/> may
    /// enter (hex, slice). <paramref name="observer"/>: check against that observer's view
    /// (hidden occupants don't count); <paramref name="trueState"/> true: check only the capacity,
    /// against true state.</summary>
    public static bool CanCreatureEnter(TrueState state, PlayerId? controller, HexCoord hex, Slice slice, ObjectId? mover, bool trueState, PlayerId? observer = null)
    {
        if (state.IsVoid(hex))
            return false;
        var occupants = state.CreaturesIn(hex, slice).Where(p => p.Id != mover);
        if (!trueState)
            occupants = occupants.Where(p => state.CanSee(observer, p));
        var list = occupants.ToList();
        return list.Count < Island.SliceCapacity && (trueState || list.All(p => state.Controller(p) == controller));
    }

    /// <summary>D24/D40: a Structure needs its slot free.</summary>
    public static bool CanStructureEnter(TrueState state, HexCoord hex, Slice slot, bool trueState, PlayerId? observer = null)
    {
        if (state.IsVoid(hex))
            return false;
        var existing = state.StructureIn(hex, slot);
        return existing is null || (!trueState && !state.CanSee(observer, existing));
    }

    /// <summary>Any permanent of this card type entering at (hex, slice).</summary>
    public static bool CanPermanentEnter(TrueState state, CardDefinition card, PlayerId? controller, HexCoord hex, Slice slice, bool trueState, PlayerId? observer = null) =>
        card.Type switch
        {
            CardType.Structure => CanStructureEnter(state, hex, slice, trueState, observer),
            CardType.Item => !state.IsVoid(hex),
            _ => CanCreatureEnter(state, controller, hex, slice, null, trueState, observer),
        };
}
