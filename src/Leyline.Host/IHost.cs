using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Perception;

namespace Leyline.Host;

/// <summary>
/// A1: the single abstract boundary — "commands in, this seat's View+Events out." M1 ships
/// only LocalHost; a future RemoteHost (M6) implements this same interface over a network
/// transport without client-facing code needing to change.
/// </summary>
public interface IHost
{
    IReadOnlyList<Command> LegalCommands(SeatId seat);
    HostResult Submit(SeatId seat, Command command);
    View CurrentView(SeatId seat);
}

public sealed record HostResult(bool Accepted, string? RejectionReason, View View, IReadOnlyList<ObservedEvent> Events);
