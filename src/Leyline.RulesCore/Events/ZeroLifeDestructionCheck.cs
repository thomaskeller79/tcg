using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Events;

/// <summary>D14: 0 Life → destroyed. Champion death (win-check) is a separate check, added in Slice 2.</summary>
public sealed class ZeroLifeDestructionCheck : IStateBasedCheck
{
    public IEnumerable<IEvent> Evaluate(TrueState state) =>
        state.AllActors.Where(a => a.Life <= 0).Select(a => new ActorDestroyedEvent(a.Id));
}
