using Leyline.Content.Json;
using Leyline.Host;
using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rng;
using Leyline.RulesCore.State;
using Leyline.Scenarios;

// Leyline simulation harness (architecture.md §2.9 — dev only, reads true state).
//   dotnet run --project tools/Leyline.SimHarness [scenario] [matches]        batch random play
//   dotnet run --project tools/Leyline.SimHarness interactive [scenario]      a REPL through LocalHost
var root = FindRepoRoot();
var content = CardJson.LoadDirectory(Path.Combine(root, "content", "cards"));
string ScenarioPath(string name) => Path.Combine(root, "tools", "Leyline.DebugUi", "scenarios", name + ".scenario");

if (args is ["interactive", ..])
{
    Interactive(ScenarioLoader.LoadFromFile(ScenarioPath(args.Length > 1 ? args[1] : "demo-match"), content));
    return;
}

var scenario = args.Length > 0 ? args[0] : "demo-match";
var matches = args.Length > 1 ? int.Parse(args[1]) : 50;
var text = File.ReadAllText(ScenarioPath(scenario));
int winsA = 0, winsB = 0, draws = 0, unfinished = 0;
long rounds = 0;
for (var i = 0; i < matches; i++)
{
    var state = ScenarioLoader.Load($"seed {1000 + i}\n" + text, content);
    var rng = RngState.FromSeed((ulong)(i + 1));
    for (var step = 0; step < 20000 && !state.IsOver && state.Round <= 40; step++)
    {
        var actor = state.Decision?.Decider ?? state.PriorityHolder!.Value;
        var legal = RulesEngine.LegalCommands(state, actor);
        // Random policy that acts more often than it passes, so matches progress.
        var (roll, n1) = rng.NextInt(3);
        rng = n1;
        Command pick;
        if (legal.Count > 1 && legal[0] is PassCommand && roll > 0)
        {
            var (j, n2) = rng.NextInt(legal.Count - 1);
            rng = n2;
            pick = legal[j + 1];
        }
        else
        {
            var (j, n2) = rng.NextInt(legal.Count);
            rng = n2;
            pick = legal[j];
        }
        var result = RulesEngine.Apply(state, pick);
        if (!result.Accepted)
            throw new InvalidOperationException($"Legal command rejected: {pick} — {result.Error}");
    }
    rounds += state.Round;
    if (state.Winner == PlayerId.A) winsA++;
    else if (state.Winner == PlayerId.B) winsB++;
    else if (state.IsDraw) draws++;
    else unfinished++;
}
Console.WriteLine($"{scenario}, {matches} random matches: A {winsA}, B {winsB}, draws {draws}, unfinished after 40 rounds {unfinished}, avg rounds {(double)rounds / matches:F1}");

static void Interactive(TrueState state)
{
    var host = LocalHost.TwoSeats(state);
    Console.WriteLine("Interactive Leyline — type a command number, or q to quit.");
    while (!state.IsOver)
    {
        var player = state.Decision?.Decider ?? state.PriorityHolder!.Value;
        var seat = new SeatId(player.Value);
        var view = host.CurrentView(seat);
        Console.WriteLine($"\n--- Champion {player} · round {view.Round} · {view.ActiveSeat} · {view.Phase} · Pending {view.Pending.Count}");
        foreach (var p in view.Permanents.Where(p => p.Kind != "Item" || p.Carrier is null))
            Console.WriteLine($"  #{p.Id} {p.Name} [{p.Controller}] ({p.Hex.Q},{p.Hex.R}) {p.Slice} A{p.Attack} L{p.Life} AP{p.Ap}");
        var commands = host.LegalCommands(seat);
        for (var i = 0; i < commands.Count; i++)
            Console.WriteLine($"  [{i}] {commands[i]}");
        Console.Write("> ");
        var line = Console.ReadLine();
        if (line is null || line == "q")
            return;
        if (int.TryParse(line, out var index) && index >= 0 && index < commands.Count)
        {
            var result = host.Submit(seat, commands[index]);
            if (!result.Accepted)
                Console.WriteLine($"Rejected: {result.Error}");
        }
    }
    Console.WriteLine(state.IsDraw ? "Draw." : $"Champion {state.Winner} wins.");
}

static string FindRepoRoot()
{
    var dir = new DirectoryInfo(AppContext.BaseDirectory);
    while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "content", "cards")))
        dir = dir.Parent;
    return dir?.FullName ?? throw new DirectoryNotFoundException("content/cards not found.");
}
