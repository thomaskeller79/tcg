namespace Leyline.RulesCore.State;

public enum PriorityWindowKind
{
    /// <summary>Opens when a Combat is declared (D19's one locked M1 window).</summary>
    CombatDeclare,

    /// <summary>Opens when a Creature/Spell is cast (D45/D46): the cast's Trace sits in Pending,
    /// respondable, until both players pass and it resolves. Reuses the same Pass/Pending
    /// machinery as CombatDeclare — see CombatPipeline.Pass.</summary>
    CastResolution,
}

/// <summary>Context is CombatId for CombatDeclare, TraceId for CastResolution — Pass switches on
/// Kind to know which. A closed union would be cleaner than object but isn't worth a new type
/// for two cases; see CombatPipeline.Pass's switch.</summary>
public sealed class PriorityWindow
{
    public required PriorityWindowKind Kind { get; init; }
    public required object Context { get; init; }
    public required IReadOnlyList<PlayerId> Order { get; init; }
    public int CurrentIndex { get; set; }
    public int ConsecutivePasses { get; set; }

    public PlayerId CurrentPriority => Order[CurrentIndex % Order.Count];
}
