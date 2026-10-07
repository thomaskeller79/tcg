using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Model;
using Leyline.RulesCore.Perception;
using Leyline.RulesCore.State;

namespace Leyline.Host;

/// <summary>
/// In-process Host for hotseat/solo play. Every call routes through ViewProjector and never
/// hands out a raw TrueState reference (architecture.md §5.4).
/// </summary>
public sealed class LocalHost : IHost
{
    private readonly TrueState _state;
    private readonly IReadOnlyDictionary<SeatId, PlayerId> _seats;

    public LocalHost(TrueState state, IReadOnlyDictionary<SeatId, PlayerId> seats)
    {
        _state = state;
        _seats = seats;
    }

    public static LocalHost TwoSeats(TrueState state) =>
        new(state, new Dictionary<SeatId, PlayerId> { [new SeatId(1)] = PlayerId.A, [new SeatId(2)] = PlayerId.B });

    public IReadOnlyList<Command> LegalCommands(SeatId seat) => RulesEngine.LegalCommands(_state, PlayerOf(seat));

    public HostResult Submit(SeatId seat, Command command)
    {
        var player = PlayerOf(seat);
        if (command.Actor != player)
            return new HostResult(false, "Command's Actor does not match this seat.", CurrentView(seat));
        var result = RulesEngine.Apply(_state, command);
        return new HostResult(result.Accepted, result.Error, CurrentView(seat));
    }

    public View CurrentView(SeatId seat) => ViewProjector.Project(_state, PlayerOf(seat));

    private PlayerId PlayerOf(SeatId seat) =>
        _seats.TryGetValue(seat, out var player) ? player : throw new ArgumentException($"Unknown seat {seat}.");
}
