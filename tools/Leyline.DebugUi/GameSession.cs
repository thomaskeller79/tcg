using Leyline.Content.Json;
using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;
using Leyline.Scenarios;
using LeylineHost = Leyline.Host;

namespace Leyline.DebugUi;

/// <summary>
/// The one live match of this local, single-developer tool. Aliased "LeylineHost" because
/// Leyline.Host.IHost collides with ASP.NET Core's own IHost.
/// </summary>
public sealed class GameSession
{
    public static readonly LeylineHost.SeatId SeatA = new(1);
    public static readonly LeylineHost.SeatId SeatB = new(2);

    private readonly object _lock = new();

    public GameSession(string contentDir)
    {
        ContentDir = contentDir;
        Content = CardJson.LoadDirectory(contentDir);
    }

    public string ContentDir { get; }
    public ICardDefinitionRepository Content { get; private set; }
    public TrueState? State { get; private set; }
    public LeylineHost.IHost? Host { get; private set; }
    public string? ScenarioName { get; private set; }

    /// <summary>Auto-pass per seat: when the only legal command is Pass, pass automatically —
    /// except to end your own Action phase. Leaks that you hold nothing playable (PLAN.md
    /// Track B item 6); acceptable for a debug tool, and switchable.</summary>
    public bool[] AutoPass { get; } = [true, true];

    public object Lock => _lock;

    public void Load(string name, string path)
    {
        Content = CardJson.LoadDirectory(ContentDir); // pick up card edits without a restart
        State = ScenarioLoader.LoadFromFile(path, Content);
        Host = LeylineHost.LocalHost.TwoSeats(State);
        ScenarioName = name;
        RunAutoPass();
    }

    public static PlayerId PlayerOf(int seat) => seat == 1 ? PlayerId.A : PlayerId.B;
    public static LeylineHost.SeatId SeatOf(PlayerId p) => p == PlayerId.A ? SeatA : SeatB;

    public CommandResult Submit(int seat, int index)
    {
        if (State is null || Host is null)
            return CommandResult.Reject("No scenario loaded.");
        var legal = Host.LegalCommands(new LeylineHost.SeatId(seat));
        if (index < 0 || index >= legal.Count)
            return CommandResult.Reject("Stale or out-of-range action — the view will refresh.");
        var result = Host.Submit(new LeylineHost.SeatId(seat), legal[index]);
        RunAutoPass();
        return new CommandResult(result.Accepted, result.Error);
    }

    public void RunAutoPass()
    {
        if (State is null || Host is null)
            return;
        for (var guard = 0; guard < 500 && !State.IsOver && State.Decision is null; guard++)
        {
            if (State.PriorityHolder is not { } holder || !AutoPass[holder.Value - 1])
                return;
            var ownMainPhaseEnd = State.ActiveSeat.Champion() == holder && State.Pending.Count == 0;
            if (ownMainPhaseEnd)
                return;
            var legal = Host.LegalCommands(SeatOf(holder));
            if (legal is not [PassCommand pass])
                return;
            Host.Submit(SeatOf(holder), pass);
        }
    }
}
