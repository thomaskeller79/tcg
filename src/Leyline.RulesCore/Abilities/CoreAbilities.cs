namespace Leyline.RulesCore.Abilities;

/// <summary>The core abilities every actor has by default (D10's replaceable defaults).</summary>
public static class CoreAbilities
{
    public const string Move = "core.move";
    public const string Attack = "core.attack";

    /// <summary>D15/D65: Defend's once-per-round tracking key — costs `0*AP`
    /// (Query.CanUseOncePerTurnAction), same `*` flavor as Bond/Draw, but reset only on the
    /// controller's own turn (ResetOncePerTurnActionsEffect), which with Neutral turns (D60) in
    /// the cycle now comes around once per round rather than once per engine turn. Not gated
    /// through AbilityIds like Move/Attack: every actor can defend by default (D10), so this
    /// constant only exists as the OncePerTurnActionsUsed key, not a presence check.</summary>
    public const string Defend = "core.defend";
}
