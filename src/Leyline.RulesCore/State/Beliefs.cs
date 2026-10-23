using Leyline.RulesCore.Model;

namespace Leyline.RulesCore.State;

/// <summary>D148/D154: one Champion's belief about one card. Its section is always known (the
/// true one); its content (<see cref="Definition"/>, null = `?`) and whether it knows the card's
/// position may not be. A card with neither is anonymous.</summary>
public sealed class CardBelief
{
    public string? Definition { get; set; }
    public bool PositionKnown { get; set; }

    public CardBelief Copy() => new() { Definition = Definition, PositionKnown = PositionKnown };
}

/// <summary>D152: one Champion's belief about where a permanent is (a terrain and a Slice, or
/// `?`: <see cref="Hex"/> null) and whether it observed it after the last instruction, so sight
/// lost can be told from never seen. A carried Item is where its carrier is believed to be.</summary>
public sealed class LocationBelief
{
    public HexCoord? Hex { get; set; }
    public Slice Slice { get; set; }
    public bool Observed { get; set; }
}

/// <summary>D149: one Champion's beliefs about a permanent's values. Its Power, max Health and
/// AP fold from its face (a Mimic's pretended card; null: the true one) and the modifiers in this
/// Champion's copy; its current Life and AP are the true ones plus an offset. <see cref="Causes"/>
/// are the traces and cards behind a false value, revealed with it (D151).</summary>
public sealed class ValueBelief
{
    public string? Face { get; set; }
    public int LifeOffset { get; set; }
    public int ApOffset { get; set; }
    public HashSet<ObjectId> Causes { get; } = [];
}

/// <summary>D151: what one Champion's copy predicted for one permanent an instruction damaged,
/// compared with what it sees once the instruction's consequences are done.</summary>
public sealed record BeliefCheck(PlayerId Who, ObjectId Target, int PredictedLife, IReadOnlyList<ObjectId> Reads, ObjectId? Trace);

/// <summary>D149: the Champions' beliefs, stored beside the true state. Track B item 25 builds
/// them in stages: cards (D154), permanents' locations (D152) and values (D150), and what each
/// Champion reads of a trace (its Pretend, on the trace itself).</summary>
public sealed class Beliefs
{
    private readonly Dictionary<(PlayerId Who, ObjectId Card), CardBelief> _cards = new();
    private readonly Dictionary<(PlayerId Who, ObjectId Permanent), LocationBelief> _locations = new();
    private readonly Dictionary<(PlayerId Who, ObjectId Permanent), ValueBelief> _values = new();

    public List<BeliefCheck> Checks { get; } = [];

    public ValueBelief Values(PlayerId who, ObjectId permanent) =>
        _values.TryGetValue((who, permanent), out var b) ? b : _values[(who, permanent)] = new ValueBelief();

    public LocationBelief Location(PlayerId who, ObjectId permanent) =>
        _locations.TryGetValue((who, permanent), out var b) ? b : _locations[(who, permanent)] = new LocationBelief();

    public CardBelief Card(PlayerId who, ObjectId card) =>
        _cards.TryGetValue((who, card), out var b) ? b : _cards[(who, card)] = new CardBelief();

    public void SetCard(PlayerId who, ObjectId card, CardBelief belief) => _cards[(who, card)] = belief;

    /// <summary>The object ceased to exist; a death or a destroy names it to everyone (D153).</summary>
    public void Forget(ObjectId id)
    {
        foreach (var key in _cards.Keys.Where(k => k.Card == id).ToList())
            _cards.Remove(key);
        foreach (var key in _locations.Keys.Where(k => k.Permanent == id).ToList())
            _locations.Remove(key);
        foreach (var key in _values.Keys.Where(k => k.Permanent == id).ToList())
            _values.Remove(key);
    }
}
