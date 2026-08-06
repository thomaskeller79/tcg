namespace Leyline.RulesCore.Rng;

/// <summary>
/// Deterministic PRNG state (xorshift64*). Pure: <c>Next*</c> never mutates in place — it
/// returns the drawn value alongside the advanced state, so RNG consumption flows through
/// the same "produce a value, thread the new state forward" discipline as everything else
/// TrueState tracks. No <c>System.Random</c>/<c>Guid.NewGuid()</c> inside RulesCore — see
/// the M1 plan's determinism conventions.
/// </summary>
public readonly record struct RngState(ulong State)
{
    public static RngState FromSeed(ulong seed) => new(seed == 0 ? 0x9E3779B97F4A7C15UL : seed);

    public (uint Value, RngState Next) NextUInt32()
    {
        var x = State;
        x ^= x >> 12;
        x ^= x << 25;
        x ^= x >> 27;
        var result = (uint)((x * 0x2545F4914F6CDD1DUL) >> 32);
        return (result, new RngState(x));
    }

    /// <summary>Uniform integer in [0, exclusiveMax).</summary>
    public (int Value, RngState Next) NextInt(int exclusiveMax)
    {
        if (exclusiveMax <= 0)
            throw new ArgumentOutOfRangeException(nameof(exclusiveMax));
        var (raw, next) = NextUInt32();
        return ((int)(raw % (uint)exclusiveMax), next);
    }
}
