namespace Leyline.RulesCore.State;

/// <summary>Any board actor backed by a CardDefinition (Creature, Champion, later Structure) —
/// lets Query.cs read stats/abilities once instead of per-concrete-type.</summary>
public interface IHasCardDefinition
{
    CardDefinitionId Definition { get; }
}
