using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>An ability's cost as it applies right now — the printed cost plus the base-rule
/// prices that depend on context (economy.md, champions.md, companions.md), plus static
/// abilities on its subtypes (D129).</summary>
public static class Costs
{
    /// <summary>The cost a caster pays at casting: the fixed cost plus the target-dependent
    /// surcharges, computed against <paramref name="viewer"/>'s view (D130); the true ones are
    /// compared at resolution.</summary>
    public static Cost Effective(TrueState state, Permanent source, AbilityDefinition ability, IReadOnlyList<IReadOnlyList<TargetChoice>> targets, PlayerId? viewer)
    {
        var cost = Fixed(state, source, ability);
        return cost with { Ap = cost.Ap + Surcharge(state, ability, targets, viewer) };
    }

    /// <summary>D135 C1: the cost that needs no target — paid before targets are chosen.</summary>
    public static Cost Fixed(TrueState state, Permanent source, AbilityDefinition ability)
    {
        var cost = ability.Cost;
        var bondedRoot = source.Kind.IsRoot() && Network.IsRootConnected(state, source);

        switch (ability.Builtin)
        {
            case BuiltinAbility.Move:
                // D134: a connected Champion or Companion has its own Move cost — it can only move
                // onto terrain it bonded itself (D9, D126 realm lock), so no destination changes it.
                if (bondedRoot)
                    cost = cost with { Ap = DefaultAbilities.ConnectedMoveAp };
                break;
            case BuiltinAbility.Attack:
                // D134: `6~AP` while connected, `3~AP` otherwise.
                if (bondedRoot)
                    cost = cost with { Ap = DefaultAbilities.ConnectedAttackAp };
                break;
            case BuiltinAbility.Defend:
                // D134: `2~AP` while connected; D116 Defender keyword: `1AP: Defend` instead of `1~AP`.
                if (bondedRoot)
                    cost = cost with { Ap = DefaultAbilities.ConnectedDefendAp };
                if (state.HasKeyword(source, Keyword.Defender))
                    cost = cost with { Flavor = ApFlavor.Plain };
                break;
        }
        // D129/D134: static abilities on the ability's subtypes from the terrain it stands on.
        return cost with { Ap = cost.Ap + StaticAp(state, state.TerrainOf(state.PositionOf(source)), StaticScope.OnThis, ability) };
    }

    /// <summary>D135 C4: the AP the chosen targets add, against <paramref name="viewer"/>'s view (D130).</summary>
    public static int Surcharge(TrueState state, AbilityDefinition ability, IReadOnlyList<IReadOnlyList<TargetChoice>> targets, PlayerId? viewer) =>
        targets.SelectMany(list => list).Sum(c => TargetSurcharge(state, ability, c, viewer, trueState: false));

    /// <summary>D129/D130: the AP static abilities add to an ability for one target — from the
    /// permanent or terrain it targets (entering it). At casting it is computed against the
    /// caster's view; at resolution against true state, to find what needs a top-up.</summary>
    public static int TargetSurcharge(TrueState state, AbilityDefinition ability, TargetChoice choice, PlayerId? viewer, bool trueState)
    {
        var holder = choice.Object is { } id ? state.Find<Permanent>(id)
            : choice.Hex is { } hex && state.IsOnBoard(hex) ? state.TerrainOf(hex)
            : null;
        if (holder is null || !(trueState || state.CanSee(viewer, holder)))
            return 0;
        return StaticAp(state, holder, StaticScope.TargetingThis, ability);
    }

    private static int StaticAp(TrueState state, Permanent holder, StaticScope scope, AbilityDefinition ability) =>
        holder.Kind == PermanentKind.Remnant
            ? 0
            : state.Def(holder).Statics.Where(s => s.Scope == scope && ability.Subtypes.Contains(s.Subtype)).Sum(s => s.Ap);

    /// <summary>Can this permanent pay the AP part (D116)? `*` also checks this cycle's use.</summary>
    public static bool CanPayAp(Permanent payer, Cost cost, string abilityId)
    {
        if (cost.Flavor == ApFlavor.OncePerCycle && payer.UsedThisCycle.Contains(abilityId))
            return false;
        return payer.CurrentAp >= cost.Ap;
    }

    public static void PayAp(TrueState state, Permanent payer, Cost cost, string abilityId)
    {
        switch (cost.Flavor)
        {
            case ApFlavor.Exhaust:
                payer.CurrentAp = 0;
                break;
            case ApFlavor.Done:
                payer.CurrentAp -= cost.Ap;
                payer.Locked = true;
                break;
            case ApFlavor.OncePerCycle:
                payer.CurrentAp -= cost.Ap;
                payer.UsedThisCycle.Add(abilityId);
                break;
            default:
                payer.CurrentAp -= cost.Ap;
                break;
        }
    }
}
