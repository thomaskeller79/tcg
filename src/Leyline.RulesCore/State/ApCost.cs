namespace Leyline.RulesCore.State;

/// <summary>
/// D10's "!" cost flavor: a plain cost requires exactly <see cref="Required"/> AP; an
/// exhausting cost requires at least that much, then consumes ALL of the actor's remaining
/// AP (e.g. "3!AP: Attack" — a creature with 5 AP can still attack, but is left with 0).
/// </summary>
public readonly record struct ApCost(int Required, bool ExhaustsRemaining)
{
    public static ApCost Fixed(int amount) => new(amount, false);
    public static ApCost Exhaust(int required) => new(required, true);

    public bool IsAffordable(int currentAp) => currentAp >= Required;

    public int Apply(int currentAp) => ExhaustsRemaining ? 0 : currentAp - Required;
}
