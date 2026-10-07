using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Perception;

namespace Leyline.Host;

/// <summary>
/// A1: the single abstract boundary — "commands in, this seat's View out." Only LocalHost exists
/// today; a future RemoteHost (M6) implements the same interface over a network transport.
/// </summary>
public interface IHost
{
    IReadOnlyList<Command> LegalCommands(SeatId seat);
    HostResult Submit(SeatId seat, Command command);
    View CurrentView(SeatId seat);
}

public sealed record HostResult(bool Accepted, string? Error, View View);
