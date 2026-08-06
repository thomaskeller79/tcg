namespace Leyline.RulesCore.State;

public readonly record struct PlayerId(int Value) : IComparable<PlayerId>
{
    public int CompareTo(PlayerId other) => Value.CompareTo(other.Value);
    public override string ToString() => $"P{Value}";
}

public readonly record struct ActorId(int Value) : IComparable<ActorId>
{
    public int CompareTo(ActorId other) => Value.CompareTo(other.Value);
    public override string ToString() => $"A{Value}";
}

public readonly record struct CombatId(int Value) : IComparable<CombatId>
{
    public int CompareTo(CombatId other) => Value.CompareTo(other.Value);
    public override string ToString() => $"C{Value}";
}

public readonly record struct StackItemId(int Value) : IComparable<StackItemId>
{
    public int CompareTo(StackItemId other) => Value.CompareTo(other.Value);
    public override string ToString() => $"S{Value}";
}

public readonly record struct ModifierId(int Value) : IComparable<ModifierId>
{
    public int CompareTo(ModifierId other) => Value.CompareTo(other.Value);
    public override string ToString() => $"M{Value}";
}
