using Leyline.RulesCore.Events;

namespace Leyline.RulesCore.State;

/// <summary>
/// What a queued Trace does once it resolves — defined per-source (SpellPipeline's
/// PendingCreatureCast/PendingSpellCast today) so the Aether zone itself stays source-agnostic
/// about what put a trace there. D33: an illegal target at resolution fizzles only that
/// instruction, not the whole trace — implementations re-check legality here rather than assume
/// the Pending-time snapshot still holds.
/// </summary>
public interface IPendingResolution
{
    IEnumerable<EventIntent> Resolve(TrueState state);
    string Describe(TrueState state);
}

/// <summary>An Aether record left by a resolved spell/ability (glossary "Trace"). While still
/// queued to resolve it sits in the Pending zone; once it resolves and crosses Now it becomes a
/// PastTrace.</summary>
public sealed record Trace(TraceId Id, PlayerId Controller, IPendingResolution Resolution);

/// <summary>A Trace that has resolved and crossed Now into Past — a historical record, fading
/// after Duration rounds (D50; default 5, placeholder — see TrueState.DefaultTraceDuration).</summary>
public sealed record PastTrace(TraceId Id, PlayerId Controller, string Description, int CreatedAtRound, int FadesAtRound);

/// <summary>D38: the Aether's "immediate future" zone — traces about to resolve, next to Now,
/// resolving LIFO (today's stack, unchanged in mechanism, glossary "Pending"). Empty in M1
/// outside an open cast/combat priority window — no persistent Pending content survives past
/// both players passing.</summary>
public sealed class PendingZone
{
    private readonly List<Trace> _items = [];

    public IReadOnlyList<Trace> Items => _items;
    public bool IsEmpty => _items.Count == 0;

    public void Push(Trace trace) => _items.Add(trace);

    public Trace Pop()
    {
        var top = _items[^1];
        _items.RemoveAt(_items.Count - 1);
        return top;
    }
}
