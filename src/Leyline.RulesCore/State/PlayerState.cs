namespace Leyline.RulesCore.State;

public sealed class PlayerState
{
    public required PlayerId Id { get; init; }

    /// <summary>D10: a single shared pool, spent on spells/units — refreshed (not banked) each
    /// Beginning phase per D21, to the sum of connected/producing terrain (D8).</summary>
    public int Mana { get; set; }
}
