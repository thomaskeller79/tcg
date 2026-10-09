using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>An ability's cost as it applies right now — the printed cost plus the base-rule
/// prices that depend on context (economy.md, champions.md, companions.md), plus static
/// abilities on its subtypes (D129).</summary>
public static class Costs
{
    public static Cost Effective(TrueState state, Permanent source, AbilityDefinition ability, IReadOnlyList<IReadOnlyList<TargetChoice>> targets)
    {
        var cost = ability.Cost;
        var bondedRoot = source.Kind.IsRoot() && Network.IsRootConnected(state, source);

        switch (ability.Builtin)
        {
            case BuiltinAbility.Move:
            {
                // A connected Champion or Companion pays double — it can only move onto terrain it
                // bonded itself (D9, D126 realm lock).
                var dest = targets.Count > 0 && targets[0].Count > 0 ? targets[0][0].Hex : null;
                var staysConnected = bondedRoot && dest is { } d && state.IsOnBoard(d) && state.TerrainOf(d).Parent == source.Id;
                cost = cost with { Ap = cost.Ap * (staysConnected ? 2 : 1) };
                break;
            }
            case BuiltinAbility.Attack:
                // D49/D116: `3~AP`, doubled to `6~AP` while network-bonded.
                cost = cost with { Ap = cost.Ap * (bondedRoot ? 2 : 1) };
                break;
            case BuiltinAbility.Defend:
                // D116 Defender keyword: `1AP: Defend` instead of `1~AP`; D117 doubled while bonded.
                var defend = state.HasKeyword(source, Keyword.Defender) ? cost with { Flavor = ApFlavor.Plain } : cost;
                cost = defend with { Ap = defend.Ap * (bondedRoot ? 2 : 1) };
                break;
        }
        return cost with { Ap = cost.Ap + StaticSurcharge(state, source, ability, targets) };
    }

    /// <summary>D129: the AP that static abilities add to an ability with their subtype — from the
    /// terrain the source stands on (leaving), or from a permanent or terrain it targets (entering).
    /// Added after the doubling for bonded roots.</summary>
    private static int StaticSurcharge(TrueState state, Permanent source, AbilityDefinition ability, IReadOnlyList<IReadOnlyList<TargetChoice>> targets)
    {
        if (ability.Subtypes.Count == 0)
            return 0;
        var from = state.PositionOf(source);
        var total = 0;
        foreach (var holder in state.Permanents.Where(p => p.Kind != PermanentKind.Remnant))
        {
            foreach (var s in state.Def(holder).Statics.Where(s => ability.Subtypes.Contains(s.Subtype)))
            {
                var applies = s.Scope switch
                {
                    StaticScope.OnThis => holder.Kind == PermanentKind.Terrain && holder.Hex == from,
                    _ => targets.Any(list => list.Any(c => c.Object == holder.Id || (holder.Kind == PermanentKind.Terrain && c.Hex == holder.Hex))),
                };
                if (applies)
                    total += s.Ap;
            }
        }
        return total;
    }

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
