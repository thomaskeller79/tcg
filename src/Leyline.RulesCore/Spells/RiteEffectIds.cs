namespace Leyline.RulesCore.Spells;

/// <summary>The Rite-effect placeholder (see CardDefinition's doc comment): two hardcoded
/// effects, just enough to prove the cast-a-Rite pipeline end to end. Stands in for the
/// general card-effect system design-continuous-effects.md flags as still undesigned.</summary>
public static class RiteEffectIds
{
    public const string Damage = "rite.damage";
    public const string Heal = "rite.heal";
}
