using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>
/// Printed target selections (effect-form.md, prototype scope D104): chosen by "you" at cast,
/// against the chooser's own view. A target's own qualifiers are consumed at binding; a relation
/// to another target (RelativeTo) stays in the trace and is rechecked at resolution (D83).
/// </summary>
public static class Targeting
{
    public static bool KindMatches(Permanent p, TargetKind kind) => kind switch
    {
        TargetKind.Creature => p.Kind.IsCreatureType(),
        TargetKind.Actor => p.Kind.IsActor(),
        TargetKind.Permanent => p.Kind != PermanentKind.Terrain,
        TargetKind.Terrain => p.Kind == PermanentKind.Terrain,
        TargetKind.Remnant => p.Kind == PermanentKind.Remnant,
        TargetKind.Item => p.Kind == PermanentKind.Item,
        TargetKind.Structure => p.Kind == PermanentKind.Structure,
        _ => false,
    };

    /// <summary>Does object <paramref name="id"/> satisfy the selection's own qualifiers, seen by
    /// <paramref name="you"/>? Relations are checked separately (<see cref="RelationHolds"/>).</summary>
    public static bool Qualifies(TrueState state, PlayerId? you, Permanent? source, TargetSpec spec, ObjectId id)
    {
        if (state.Find<Permanent>(id) is not { } p || !KindMatches(p, spec.Kind))
            return false;
        if (p.Kind == PermanentKind.Terrain && state.Def(p).IsVoid)
            return false;
        if (!state.CanSee(you, p))
            return false;
        switch (spec.Control)
        {
            case ControlFilter.You when state.Controller(p) != you:
            case ControlFilter.NotYou when state.Controller(p) == you:
                return false;
        }
        if (spec.WithinOfSource is { } within && source is not null && state.PositionOf(p).DistanceTo(state.PositionOf(source)) > within)
            return false;
        return true;
    }

    public static bool RelationHolds(TrueState state, TargetSpec spec, ObjectId id, IReadOnlyDictionary<string, List<TargetChoice>> bound)
    {
        if (spec.RelativeTo is null)
            return true;
        if (!bound.TryGetValue(spec.RelativeTo, out var others) || others.Count == 0)
            return false;
        if (state.Find<Permanent>(id) is not { } p)
            return false;
        return others.All(o => o.Object is { } oid
                               && state.Find<Permanent>(oid) is { } other
                               && state.PositionOf(other).DistanceTo(state.PositionOf(p)) <= (spec.WithinOfTarget ?? 0));
    }

    public static IEnumerable<ObjectId> Candidates(TrueState state, PlayerId? you, Permanent? source, TargetSpec spec, IReadOnlyDictionary<string, List<TargetChoice>> bound) =>
        state.Permanents
            .Where(p => Qualifies(state, you, source, spec, p.Id) && RelationHolds(state, spec, p.Id, bound))
            .Select(p => p.Id);

    /// <summary>Every legal choice list for one selection (count min..max; repeats never within
    /// one selection). Enumerates at most two picks per selection.</summary>
    public static IEnumerable<List<TargetChoice>> Options(TrueState state, PlayerId? you, Permanent? source, TargetSpec spec, IReadOnlyDictionary<string, List<TargetChoice>> bound)
    {
        var candidates = Candidates(state, you, source, spec, bound).ToList();
        if (spec.Min == 0)
            yield return [];
        if (spec.Max >= 1 && spec.Min <= 1)
        {
            foreach (var c in candidates)
                yield return [TargetChoice.ForObject(c)];
        }
        if (spec.Max >= 2 && spec.Min <= 2)
        {
            for (var i = 0; i < candidates.Count; i++)
                for (var j = i + 1; j < candidates.Count; j++)
                    yield return [TargetChoice.ForObject(candidates[i]), TargetChoice.ForObject(candidates[j])];
        }
    }

    /// <summary>Validates a full choice list for a selection.</summary>
    public static bool IsValid(TrueState state, PlayerId? you, Permanent? source, TargetSpec spec, IReadOnlyList<TargetChoice> choices, IReadOnlyDictionary<string, List<TargetChoice>> bound)
    {
        if (choices.Count < spec.Min || choices.Count > spec.Max)
            return false;
        if (choices.Any(c => c.Object is null) || choices.Select(c => c.Object).Distinct().Count() != choices.Count)
            return false;
        return choices.All(c => Qualifies(state, you, source, spec, c.Object!.Value) && RelationHolds(state, spec, c.Object.Value, bound));
    }
}
