using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Rules;

/// <summary>An ability's cost as it applies right now — the printed cost plus the base-rule
/// prices that depend on context (economy.md, champions.md, companions.md).</summary>
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
                // G9: the destination terrain's move cost. A connected Champion or Companion pays
                // double — it can only move onto terrain it bonded itself (D9, D126 realm lock).
                var dest = targets.Count > 0 && targets[0].Count > 0 ? targets[0][0].Hex : null;
                var onBoard = dest is { } d && state.IsOnBoard(d);
                var moveCost = onBoard ? state.Def(state.TerrainOf(dest!.Value)).MoveCost : cost.Ap;
                var staysConnected = bondedRoot && onBoard && state.TerrainOf(dest!.Value).Parent == source.Id;
                return cost with { Ap = moveCost * (staysConnected ? 2 : 1) };
            }
            case BuiltinAbility.Attack:
                // D49/D116: `3~AP`, doubled to `6~AP` while network-bonded.
                return cost with { Ap = cost.Ap * (bondedRoot ? 2 : 1) };
            case BuiltinAbility.Defend:
                // D116 Defender keyword: `1AP: Defend` instead of `1~AP`; D117 doubled while bonded.
                var defend = state.HasKeyword(source, Keyword.Defender) ? cost with { Flavor = ApFlavor.Plain } : cost;
                return defend with { Ap = defend.Ap * (bondedRoot ? 2 : 1) };
            default:
                return cost;
        }
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
