using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Modifiers;

/// <summary>
/// Minimal direct entry point for adding/removing a query modifier, following the
/// per-subsystem pipeline pattern (Combat/Champions/Terrain). Not wired into
/// RulesEngine.Apply — there is no Command type for it yet (that's the not-yet-designed
/// Rite-casting pipeline's job, which will call these same primitives after its own
/// legality/cost/targeting validation). Exists so the add/remove mechanism is independently
/// usable and testable now.
/// </summary>
public static class ModifierPipeline
{
    /// <summary>Allocates the ModifierId here (mirrors CombatPipeline allocating CombatId
    /// itself) and hands it to the caller-supplied factory so the modifier can embed its own
    /// stable id at construction time.</summary>
    public static CommandResult AddModifier(TrueState state, EventPipeline pipeline, Func<ModifierId, IModifier> buildModifier)
    {
        var id = state.AllocateModifierId();
        var events = pipeline.Process(new AddModifierIntent(buildModifier(id)), state);
        return CommandResult.Accept(events);
    }

    public static CommandResult RemoveModifier(TrueState state, EventPipeline pipeline, ModifierId id) =>
        CommandResult.Accept(pipeline.Process(new RemoveModifierIntent(id), state));
}
