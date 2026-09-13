namespace Leyline.RulesCore.State;

/// <summary>
/// Vertical occupancy level on a hex (D12, renamed from "Layer" D41). Air is present for
/// structural completeness (no flyer type exists in M1 — see PLAN.md M1 scope) so it isn't a
/// reshape later.
/// </summary>
public enum Level
{
    Surface,
    Air,
    Underground,
}
