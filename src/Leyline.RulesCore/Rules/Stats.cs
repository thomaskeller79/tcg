using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>Queried values (pillar 5: never read a raw stat). Each folds the printed value, the
/// permanent's own max-tier changes, its carried Items and the active modifiers.</summary>
public static class Stats
{
    public static int Attack(this TrueState state, Permanent p)
    {
        if (!p.Kind.IsCreatureType())
            return 0;
        var value = state.Def(p).Attack + p.AttackDelta + state.CarriedBy(p.Id).Sum(i => state.Def(i).CarrierAttack);
        return Math.Max(0, Fold(state, p.Id, "Attack", value));
    }

    public static int MaxLife(this TrueState state, Permanent p)
    {
        if (!p.Kind.IsActor())
            return 0;
        var value = state.Def(p).Life + p.MaxLifeDelta + state.CarriedBy(p.Id).Sum(i => state.Def(i).CarrierLife);
        return Fold(state, p.Id, "MaxLife", value);
    }

    public static int MaxAp(this TrueState state, Permanent p) =>
        p.Kind.IsActor() ? Math.Max(0, Fold(state, p.Id, "MaxAp", state.Def(p).Ap)) : 0;

    public static bool HasKeyword(this TrueState state, Permanent p, string keyword) =>
        p.Kind != PermanentKind.Remnant && state.Def(p).HasKeyword(keyword);

    public static int KeywordValue(this TrueState state, Permanent p, string keyword) =>
        p.Kind == PermanentKind.Remnant ? 0 : state.Def(p).KeywordValue(keyword);

    /// <summary>The activated and triggered abilities of a permanent (D35). Remnant has none
    /// (D110); Terrain has no activated ones (D87).</summary>
    public static IReadOnlyList<AbilityDefinition> Abilities(this TrueState state, Permanent p) =>
        p.Kind == PermanentKind.Remnant ? [] : DefaultAbilities.For(state.Def(p));

    /// <summary>D127: the distances its attack reaches — Melee 0–1 (D120), Ranged a-b from a to b.</summary>
    public static (int Min, int Max) Reach(this TrueState state, Permanent p)
    {
        if (p.Kind == PermanentKind.Remnant || state.Def(p).Keywords.FirstOrDefault(k => k.Name == Keyword.Ranged) is not { } ranged)
            return (0, 1);
        return (ranged.Value, ranged.To ?? ranged.Value);
    }

    private static int Fold(TrueState state, ObjectId subject, string stat, int baseline)
    {
        var value = baseline;
        foreach (var m in state.Modifiers)
        {
            if (m.Subject == subject && m.Stat == stat)
                value += m.Delta;
        }
        return value;
    }
}
