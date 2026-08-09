namespace Leyline.RulesCore.State;

/// <summary>
/// D16 (partial slice): Hand and Library only — enough to draw and cast. The full Aether
/// model (unified stack/graveyard, traces, fade windows, un-summon vs kill) is a separate,
/// much larger design surface that's explicitly not built here; Rites resolve their effect
/// and simply vanish (no permanent, no tracked trace) rather than leaving an Aether record.
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
}
