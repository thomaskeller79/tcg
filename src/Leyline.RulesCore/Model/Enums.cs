namespace Leyline.RulesCore.Model;

/// <summary>D12/D78: a hex's three vertical occupancy spaces, bottom to top.</summary>
public enum Slice
{
    Root,
    Ground,
    Sky,
}

/// <summary>D51: the eight mana colors.</summary>
public enum Element
{
    Light,
    Fire,
    Metal,
    Earth,
    Darkness,
    Ice,
    Water,
    Air,
}

/// <summary>D17/D39/D103: the card types. Champion and Terrain never occupy a Mind zone.</summary>
public enum CardType
{
    Creature,
    Companion,
    Spell,
    Structure,
    Item,
    Champion,
    Terrain,
}

/// <summary>D45: when a card or ability may be played, checked live against Pending.</summary>
public enum Speed
{
    Slow,
    Quick,
    Reactive,
    Instant,
}

/// <summary>D116: the four AP cost flavors — `x`, `x!` (consume all remaining), `x~` (no more
/// abilities this turn), `x*` (once per own-turn cycle).</summary>
public enum ApFlavor
{
    Plain,
    Exhaust,
    Done,
    OncePerCycle,
}

/// <summary>D21/D54/D60: the four seats of a round.</summary>
public enum Seat
{
    ChampionA,
    NeutralA,
    ChampionB,
    NeutralB,
}

public enum Phase
{
    Setup,
    Beginning,
    Action,
    End,
}

/// <summary>D16/D37/D38: every object is in exactly one zone.</summary>
public enum Zone
{
    Library,
    Hand,
    Discard,
    Past,
    Pending,
    Future,
    Island,
}

/// <summary>D44/D103: Actors (Champion, Companion, Creature, Structure) and Objects (Item,
/// Remnant, Terrain) — everything on the Island.</summary>
public enum PermanentKind
{
    Champion,
    Companion,
    Creature,
    Structure,
    Item,
    Remnant,
    Terrain,
}

/// <summary>D109: what created a trace.</summary>
public enum TraceKind
{
    Permanent,
    Spell,
    Ability,
}

public static class SeatExtensions
{
    public static PlayerId? Champion(this Seat seat) => seat switch
    {
        Seat.ChampionA => PlayerId.A,
        Seat.ChampionB => PlayerId.B,
        _ => null,
    };

    public static Seat SeatOf(this PlayerId player) => player == PlayerId.A ? Seat.ChampionA : Seat.ChampionB;

    /// <summary>D60/D82: the neutral seat paired with a Champion in turn order.</summary>
    public static Seat NeutralSeatAfter(this PlayerId player) => player == PlayerId.A ? Seat.NeutralA : Seat.NeutralB;

    public static bool IsCreatureType(this CardType type) =>
        type is CardType.Creature or CardType.Companion or CardType.Champion;

    public static bool IsActorType(this CardType type) =>
        type is CardType.Creature or CardType.Companion or CardType.Champion or CardType.Structure;

    public static bool IsPermanentCard(this CardType type) =>
        type is CardType.Creature or CardType.Companion or CardType.Structure or CardType.Item;

    /// <summary>Creature-type permanents occupy a Slice and fight (D119): Creature, Companion, Champion.</summary>
    public static bool IsCreatureType(this PermanentKind kind) =>
        kind is PermanentKind.Creature or PermanentKind.Companion or PermanentKind.Champion;

    public static bool IsActor(this PermanentKind kind) =>
        kind is PermanentKind.Creature or PermanentKind.Companion or PermanentKind.Champion or PermanentKind.Structure;

    /// <summary>Champion and Companion are network roots and hold a mana pool (D8, D22).</summary>
    public static bool IsRoot(this PermanentKind kind) =>
        kind is PermanentKind.Champion or PermanentKind.Companion;

    public static PermanentKind ToPermanentKind(this CardType type) => type switch
    {
        CardType.Creature => PermanentKind.Creature,
        CardType.Companion => PermanentKind.Companion,
        CardType.Structure => PermanentKind.Structure,
        CardType.Item => PermanentKind.Item,
        CardType.Champion => PermanentKind.Champion,
        CardType.Terrain => PermanentKind.Terrain,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "A Spell never becomes a permanent."),
    };
}
