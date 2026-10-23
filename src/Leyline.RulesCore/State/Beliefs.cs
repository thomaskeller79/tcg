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

/// <summary>D149: the Champions' beliefs, stored beside the true state. Track B item 25 builds
/// them in stages; so far they cover cards (D154) and permanents' locations (D152). Every other
/// value, and every trace, is true in every copy until Pretend (D155).</summary>
public sealed class Beliefs
{
    private readonly Dictionary<(PlayerId Who, ObjectId Card), CardBelief> _cards = new();
    private readonly Dictionary<(PlayerId Who, ObjectId Permanent), LocationBelief> _locations = new();

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
    }
}
