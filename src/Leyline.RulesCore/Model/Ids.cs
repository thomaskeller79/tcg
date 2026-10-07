namespace Leyline.RulesCore.Model;

/// <summary>D93: every game object — Card, Trace, Permanent — carries one unique, monotonic ID
/// from a single shared counter. Never reused: an object that is re-created (bounced, flickered,
/// raised) is a new object with a new ID.</summary>
public readonly record struct ObjectId(int Value) : IComparable<ObjectId>
{
    public int CompareTo(ObjectId other) => Value.CompareTo(other.Value);
    public override string ToString() => $"#{Value}";
}

/// <summary>A Champion seat. Value 1 = Champion A, 2 = Champion B. "No PlayerId" (null) is used
/// throughout for the third controller value, Neutral (D54).</summary>
public readonly record struct PlayerId(int Value) : IComparable<PlayerId>
{
    public static readonly PlayerId A = new(1);
    public static readonly PlayerId B = new(2);

    public int CompareTo(PlayerId other) => Value.CompareTo(other.Value);
    public PlayerId Opponent => Value == 1 ? B : A;
    public override string ToString() => Value == 1 ? "A" : "B";
}

public readonly record struct ModifierId(int Value)
{
    public override string ToString() => $"M{Value}";
}
