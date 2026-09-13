namespace Leyline.RulesCore.State;

/// <summary>
/// D16/D37: the Mind domain's three zones for one player — Library ("future"), Hand
/// (present), Discard ("past", D37 — not a graveyard; a card discharges here the instant it's
/// cast). The Aether domain (Past/Pending/Future traces) lives on TrueState instead, since it
/// isn't per-player (see TrueState.Pending/Past/Future).
/// Library order is never shuffled — for a testing/debug tool, "draw exactly this card next"
/// (i.e. deterministic, author-controlled order) is more useful than realism.
/// </summary>
public sealed class PlayerState
{
    public required PlayerId Id { get; init; }

    /// <summary>D10: a single shared pool, spent on spells/units — refreshed (not banked) each
    /// Beginning phase per D21, to the sum of connected/producing terrain (D8).</summary>
    public int Mana { get; set; }

    /// <summary>Draw order: index 0 is the top (next card drawn). Never shuffled — see class comment.</summary>
    public List<CardDefinitionId> Library { get; } = [];

    public List<CardDefinitionId> Hand { get; } = [];

    public List<CardDefinitionId> Discard { get; } = [];
}
