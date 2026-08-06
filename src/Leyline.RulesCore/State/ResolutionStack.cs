namespace Leyline.RulesCore.State;

public sealed record StackItem(StackItemId Id, PlayerId Controller, object Payload);

/// <summary>LIFO region for instant-speed responses (design-interaction-stack.md). Empty in M1 — no
/// instant-speed content exists yet — but real: state-based checks run after every pop.</summary>
public sealed class ResolutionStack
{
    private readonly List<StackItem> _items = [];

    public IReadOnlyList<StackItem> Items => _items;
    public bool IsEmpty => _items.Count == 0;

    public void Push(StackItem item) => _items.Add(item);

    public StackItem Pop()
    {
        var top = _items[^1];
        _items.RemoveAt(_items.Count - 1);
        return top;
    }
}
