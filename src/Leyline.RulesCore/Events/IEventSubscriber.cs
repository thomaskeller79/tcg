using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Events;

/// <summary>Triggered-ability hook (pillar 5, mutation kind #5). Nothing subscribes in M1.</summary>
public interface IEventSubscriber
{
    bool ListensFor(IEvent evt, TrueState state);
    StackItem CreateResponse(IEvent evt, TrueState state);
}
