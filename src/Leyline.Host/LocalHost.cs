using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Perception;
using Leyline.RulesCore.State;

namespace Leyline.Host;

/// <summary>
/// In-process Host for hotseat/solo play. Every call routes through ViewProjector and never
/// hands out a raw TrueState reference — the explicit A1 anti-pattern ("a 'works locally'
/// mistake that only surfaces once RemoteHost is built") this type exists to avoid.
/// </summary>
public sealed class LocalHost : IHost
{
    private readonly Match _match;
    private readonly IReadOnlyDictionary<SeatId, PlayerId> _seats;

    public LocalHost(Match match, IReadOnlyDictionary<SeatId, PlayerId> seats)
    {
        _match = match;
        _seats = seats;
    }

    public IReadOnlyList<Command> LegalCommands(SeatId seat) =>
        RulesEngine.LegalCommands(_match, PlayerOf(seat));

    public HostResult Submit(SeatId seat, Command command)
    {
        var player = PlayerOf(seat);
        if (command.Actor != player)
            return new HostResult(false, "Command's Actor does not match this seat.", CurrentView(seat), []);

        var result = RulesEngine.Apply(_match, command);
        var events = result.Accepted
            ? ViewProjector.ProjectEvents(result.Events, player, _match.State)
            : [];
        return new HostResult(result.Accepted, result.RejectionReason, CurrentView(seat), events);
    }

    public View CurrentView(SeatId seat) => ViewProjector.Project(_match.State, PlayerOf(seat));

    private PlayerId PlayerOf(SeatId seat) =>
        _seats.TryGetValue(seat, out var player) ? player : throw new ArgumentException($"Unknown seat {seat}.");
}
