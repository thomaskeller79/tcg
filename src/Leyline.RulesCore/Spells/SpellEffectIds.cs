namespace Leyline.RulesCore.Spells;

/// <summary>The Spell-effect placeholder (see CardDefinition's doc comment): two hardcoded
/// effects, just enough to prove the cast-a-Spell pipeline end to end. Stands in for the
/// general card-effect system design-continuous-effects.md flags as still undesigned.</summary>
public static class SpellEffectIds
{
    public const string Damage = "spell.damage";
    public const string Heal = "spell.heal";
}
