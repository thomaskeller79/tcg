namespace Leyline.RulesCore.State;

public readonly record struct CardDefinitionId(string Value)
{
    public override string ToString() => Value;
}

/// <summary>
/// A provisional, M1-only content shape (see Leyline.Content.Json's schema note) — not the
/// final card schema, which depends on unfinished Track A design work.
/// </summary>
public sealed record CardDefinition(
    CardDefinitionId Id,
    string Name,
    int Attack,
    int Life,
    int MaxAp,
    IReadOnlyList<string> AbilityIds);

public interface ICardDefinitionRepository
{
    CardDefinition Get(CardDefinitionId id);
}
