namespace Leyline.RulesCore.Champions;

/// <summary>D9: Bond is a default ability the Champion has, same as CoreAbilities.Move/Attack —
/// gated through AbilityIds, not a hardcoded type-check, so a future card effect could grant
/// it to another actor.</summary>
public static class ChampionActionIds
{
    public const string Bond = "champion.bond";
}
