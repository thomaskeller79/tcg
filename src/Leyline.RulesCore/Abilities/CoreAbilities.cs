namespace Leyline.RulesCore.Abilities;

/// <summary>The core abilities every actor has by default (D10's replaceable defaults).</summary>
public static class CoreAbilities
{
    public const string Move = "core.move";
    public const string Attack = "core.attack";

    /// <summary>D15 (resolved 2026-08-09): Defend's once-per-turn tracking key — costs `0*AP`
    /// (Query.CanUseOncePerTurnAction), same `*` flavor as Bond/Draw. Not gated through
    /// AbilityIds like Move/Attack: every actor can defend by default (D10), so this constant
    /// only exists as the OncePerTurnActionsUsed key, not a presence check.</summary>
    public const string Defend = "core.defend";
}
