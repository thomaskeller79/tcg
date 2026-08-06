using Leyline.RulesCore.Events;

namespace Leyline.RulesCore.Commands;

public sealed record CommandResult(bool Accepted, string? RejectionReason, IReadOnlyList<IEvent> Events)
{
    public static CommandResult Reject(string reason) => new(false, reason, []);
    public static CommandResult Accept(IReadOnlyList<IEvent> events) => new(true, null, events);
}
