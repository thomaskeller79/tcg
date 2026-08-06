namespace Leyline.RulesCore.State;

/// <summary>Axial hex coordinate; the third cube coordinate (S) is derived, never stored.</summary>
public readonly record struct HexCoord(int Q, int R) : IComparable<HexCoord>
{
    public int S => -Q - R;

    private static readonly HexCoord[] Directions =
    [
        new(1, 0), new(1, -1), new(0, -1),
        new(-1, 0), new(-1, 1), new(0, 1),
    ];

    public IEnumerable<HexCoord> Neighbors()
    {
        foreach (var d in Directions)
            yield return this + d;
    }

    public int DistanceTo(HexCoord other)
    {
        var dq = Math.Abs(Q - other.Q);
        var dr = Math.Abs(R - other.R);
        var ds = Math.Abs(S - other.S);
        return Math.Max(dq, Math.Max(dr, ds));
    }

    public static HexCoord operator +(HexCoord a, HexCoord b) => new(a.Q + b.Q, a.R + b.R);
    public static HexCoord operator -(HexCoord a, HexCoord b) => new(a.Q - b.Q, a.R - b.R);

    // Canonical ordering (Q, then R) — everything observable through a Board query is
    // sorted through this rather than relying on dictionary/hash-set enumeration order.
    public int CompareTo(HexCoord other)
    {
        var q = Q.CompareTo(other.Q);
        return q != 0 ? q : R.CompareTo(other.R);
    }

    public override string ToString() => $"({Q},{R})";
}
