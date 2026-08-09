using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.State;
using LeylineHost = Leyline.Host;

namespace Leyline.DebugUi;

/// <summary>Single mutable in-memory session — this is a local single-developer debug tool,
/// not a multi-user server, so one current Match/Host is all it needs. Aliased "LeylineHost"
/// because Leyline.Host.IHost collides with ASP.NET Core's own Microsoft.Extensions.Hosting.IHost.</summary>
public sealed class GameSession
{
    public static readonly LeylineHost.SeatId P1Seat = new(1);
    public static readonly LeylineHost.SeatId P2Seat = new(2);

    private static readonly IReadOnlyDictionary<LeylineHost.SeatId, PlayerId> Seats = new Dictionary<LeylineHost.SeatId, PlayerId>
    {
        [P1Seat] = new PlayerId(1),
        [P2Seat] = new PlayerId(2),
    };

    public Match? Match { get; private set; }
    public LeylineHost.IHost? Host { get; private set; }

    public void Load(Match match)
    {
        Match = match;
        Host = new LeylineHost.LocalHost(match, Seats);
    }

    /// <summary>
    /// M1 ships zero instant-speed abilities (no stack content), so the only ever-legal
    /// response inside a priority window is Pass — RulesEngine.LegalCommands enforces this
    /// itself (only PassPriorityCommand is offered while a window is open). "Sorcery speed
    /// only for now" therefore means the window is real plumbing that always resolves itself;
    /// auto-passing both sides removes pure clicking friction without changing engine rules.
    /// The moment real instant-speed content exists, the single-legal-command check below
    /// stops matching and this loop correctly falls through to a real decision.
    /// </summary>
    public void AutoResolvePriorityWindows()
    {
        if (Match is null || Host is null)
            return;

        while (Match.State.ActiveWindow is { } window)
        {
            var seat = window.CurrentPriority.Value == 1 ? P1Seat : P2Seat;
            var legal = Host.LegalCommands(seat);
            if (legal is not [PassPriorityCommand pass])
                break;

            Host.Submit(seat, pass);
        }
    }
}
