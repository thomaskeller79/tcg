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

/// <summary>D149: the Champions' beliefs, stored beside the true state. Track B item 25 builds
/// them in stages; so far they cover cards (D154). Traces are true in every copy until Pretend
/// (D155).</summary>
public sealed class Beliefs
{
    private readonly Dictionary<(PlayerId Who, ObjectId Card), CardBelief> _cards = new();

    public CardBelief Card(PlayerId who, ObjectId card) =>
        _cards.TryGetValue((who, card), out var b) ? b : _cards[(who, card)] = new CardBelief();

    public void SetCard(PlayerId who, ObjectId card, CardBelief belief) => _cards[(who, card)] = belief;

    public void Forget(ObjectId card)
    {
        foreach (var key in _cards.Keys.Where(k => k.Card == card).ToList())
            _cards.Remove(key);
    }
}
