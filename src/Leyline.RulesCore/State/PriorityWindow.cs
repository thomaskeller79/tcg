namespace Leyline.RulesCore.State;

public enum PriorityWindowKind
{
    /// <summary>M1's one response window (locked scope decision): opens when a Combat is declared.</summary>
    CombatDeclare,
}

public sealed class PriorityWindow
{
    public required PriorityWindowKind Kind { get; init; }
    public required CombatId Context { get; init; }
    public required IReadOnlyList<PlayerId> Order { get; init; }
    public int CurrentIndex { get; set; }
    public int ConsecutivePasses { get; set; }

    public PlayerId CurrentPriority => Order[CurrentIndex % Order.Count];
}
