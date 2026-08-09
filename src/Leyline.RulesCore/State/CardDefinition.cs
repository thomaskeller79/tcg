namespace Leyline.RulesCore.State;

public readonly record struct CardDefinitionId(string Value)
{
    public override string ToString() => Value;
}

/// <summary>D17: card type is a data-driven tag. M1 slice: enough to gate what Cast* command
/// applies (only Creature/Rite are ever cast — Champion is a pre-game loadout, never drawn or
/// cast, per design-champions.md).</summary>
public enum CardType
{
    Creature,
    Champion,
    Rite,
}

/// <summary>
/// A provisional, M1-only content shape (see Leyline.Content.Json's schema note) — not the
/// final card schema, which depends on unfinished Track A design work. ManaCost/EffectId/
/// EffectAmount are trailing optional so every existing Creature/Champion call site keeps
/// compiling unchanged; Rite cards are the first real users of the Effect* pair — a small,
/// explicitly-scoped placeholder (damage/heal only, see Spells/RiteEffects.cs) standing in for
/// the general card-effect system design-continuous-effects.md flags as still undesigned.
/// </summary>
public sealed record CardDefinition(
    CardDefinitionId Id,
    string Name,
    int Attack,
    int Life,
    int MaxAp,
    IReadOnlyList<string> AbilityIds,
    CardType Type = CardType.Creature,
    int ManaCost = 0,
    string? EffectId = null,
    int EffectAmount = 0);

public interface ICardDefinitionRepository
{
    CardDefinition Get(CardDefinitionId id);
}
