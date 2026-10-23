using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>Queried values (pillar 5: never read a raw stat). Each folds the printed value, the
/// permanent's own max-tier changes, its carried Items and the active modifiers. With a
/// <c>copy</c>, the value in that Champion's copy (D149): its face and the modifiers in it.</summary>
public static class Stats
{
    /// <summary>The stats a modifier can change (D143): Power, max Health, max AP.</summary>
    public const string AttackStat = "Attack";
    public const string MaxLifeStat = "MaxLife";
    public const string MaxApStat = "MaxAp";

    /// <summary>Darkness on one Slice of a terrain (D152): its observation ability is replaced.</summary>
    public static string DarkStat(Slice slice) => $"Dark:{slice}";

    /// <summary>A gained (+1) or lost (−1) keyword or ability; the latest change wins.</summary>
    public const string AbilityStat = "Ability";

    public static int Attack(this TrueState state, Permanent p, PlayerId? copy = null)
    {
        if (!p.Kind.IsCreatureType())
            return 0;
        var value = state.DefIn(p, copy).Attack + p.AttackDelta + state.CarriedBy(p.Id).Sum(i => state.Def(i).CarrierAttack);
        return Math.Max(0, Fold(state, p.Id, AttackStat, value, copy));
    }

    public static int MaxLife(this TrueState state, Permanent p, PlayerId? copy = null)
    {
        if (!p.Kind.IsActor())
            return 0;
        var value = state.DefIn(p, copy).Life + p.MaxLifeDelta + state.CarriedBy(p.Id).Sum(i => state.Def(i).CarrierLife);
        return Fold(state, p.Id, MaxLifeStat, value, copy);
    }

    public static int MaxAp(this TrueState state, Permanent p, PlayerId? copy = null) =>
        p.Kind.IsActor() ? Math.Max(0, Fold(state, p.Id, MaxApStat, state.DefIn(p, copy).Ap, copy)) : 0;

    public static bool HasKeyword(this TrueState state, Permanent p, string keyword) =>
        state.Keywords(p).Any(k => k.Name == keyword);

    public static int KeywordValue(this TrueState state, Permanent p, string keyword) =>
        state.Keywords(p).FirstOrDefault(k => k.Name == keyword)?.Value ?? 0;

    /// <summary>Its printed keywords, then the gained and lost ones in append order.</summary>
    public static IReadOnlyList<Keyword> Keywords(this TrueState state, Permanent p, PlayerId? copy = null)
    {
        if (p.Kind == PermanentKind.Remnant)
            return [];
        var keywords = state.DefIn(p, copy).Keywords.ToList();
        foreach (var m in AbilityChanges(state, p.Id, copy))
        {
            if (m.Keyword is not { } k)
                continue;
            keywords.RemoveAll(x => x.Name == k.Name);
            if (m.Delta > 0)
                keywords.Add(k);
        }
        return keywords;
    }

    /// <summary>The activated and triggered abilities of a permanent (D35), then the gained and
    /// lost ones in append order. Remnant has none (D110); Terrain has no activated ones (D87).</summary>
    public static IReadOnlyList<AbilityDefinition> Abilities(this TrueState state, Permanent p, PlayerId? copy = null)
    {
        if (p.Kind == PermanentKind.Remnant)
            return [];
        var abilities = DefaultAbilities.For(state.DefIn(p, copy)).ToList();
        foreach (var m in AbilityChanges(state, p.Id, copy))
        {
            if (m.Ability is not { } a)
                continue;
            abilities.RemoveAll(x => x.Id == a.Id);
            if (m.Delta > 0)
                abilities.Add(a);
        }
        return abilities;
    }

    private static IEnumerable<Modifier> AbilityChanges(TrueState state, ObjectId subject, PlayerId? copy) =>
        state.Modifiers.Where(m => m.Subject == subject && m.Stat == AbilityStat && m.In(copy));

    /// <summary>The card a permanent is in this copy: a Mimic's face for the opponent (D150).</summary>
    public static CardDefinition DefIn(this TrueState state, Permanent p, PlayerId? copy) =>
        copy is { } who && p.Kind is not (PermanentKind.Terrain or PermanentKind.Remnant) && state.Beliefs.Values(who, p.Id).Face is { } face
            ? state.Content.Get(face)
            : state.Def(p);

    /// <summary>The kind a permanent is in this copy (a Mimic may seem a Structure, D159).</summary>
    public static PermanentKind KindIn(this TrueState state, Permanent p, PlayerId? copy) =>
        p.Kind is PermanentKind.Creature or PermanentKind.Companion or PermanentKind.Structure or PermanentKind.Item
            ? state.DefIn(p, copy).Type.ToPermanentKind()
            : p.Kind;

    /// <summary>Current Life and AP in this copy: the true ones plus the copy's offset.</summary>
    public static int LifeIn(this TrueState state, Permanent p, PlayerId? copy) =>
        p.CurrentLife + (copy is { } who ? state.Beliefs.Values(who, p.Id).LifeOffset : 0);

    public static int ApIn(this TrueState state, Permanent p, PlayerId? copy) =>
        p.CurrentAp + (copy is { } who ? state.Beliefs.Values(who, p.Id).ApOffset : 0);

    /// <summary>D127: the distances its attack reaches — Melee 0–1 (D120), Ranged a-b from a to b.</summary>
    public static (int Min, int Max) Reach(this TrueState state, Permanent p)
    {
        if (state.Keywords(p).FirstOrDefault(k => k.Name == Keyword.Ranged) is not { } ranged)
            return (0, 1);
        return (ranged.Value, ranged.To ?? ranged.Value);
    }

    private static int Fold(TrueState state, ObjectId subject, string stat, int baseline, PlayerId? copy)
    {
        var value = baseline;
        foreach (var m in state.Modifiers)
        {
            if (m.Subject == subject && m.Stat == stat && m.In(copy))
                value += m.Delta;
        }
        return value;
    }
}
