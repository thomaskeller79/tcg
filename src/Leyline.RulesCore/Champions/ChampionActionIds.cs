namespace Leyline.RulesCore.Champions;

/// <summary>D9: Bond is a default ability the Champion has, same as CoreAbilities.Move/Attack —
/// gated through AbilityIds, not a hardcoded type-check, so a future card effect could grant
/// it to another actor.</summary>
public static class ChampionActionIds
{
    public const string Bond = "champion.bond";
    public const string Draw = "champion.draw";

    /// <summary>D9 (under test, 2026-08-08): free (0AP), drops every bond outright — the only
    /// way to become mobile again once "rooted" by an active network (Query.ResolveMoveCost).</summary>
    public const string CollapseNetwork = "champion.collapse";
}
