namespace Leyline.RulesCore.State;

/// <summary>
/// Vertical occupancy layer on a hex (D12). Above is present for structural completeness
/// (no flyer type exists in M1 — see PLAN.md M1 scope) so it isn't a reshape later.
/// </summary>
public enum Layer
{
    Ground,
    Below,
    Above,
}
