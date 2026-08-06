namespace Leyline.RulesCore.State;

public enum DefendRuleVariant
{
    /// <summary>V1 (D15): defending costs 1!AP, at most once per turn, gated by available AP.</summary>
    Exhaust,

    /// <summary>V2 (D15): defending is free and unlimited; Life is the only defensive budget.</summary>
    DeleteDefendOnce,
}

public sealed record MatchConfig(DefendRuleVariant DefendRule);
