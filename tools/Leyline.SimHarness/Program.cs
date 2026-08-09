using Leyline.SimHarness;

var content = TestMatches.DefaultContent();

if (args is ["interactive"])
{
    InteractiveRepl.Run(content);
    return;
}

Console.WriteLine("Leyline M1 sandbox — batch simulation (2 grunts vs 2 grunts, turn cap 40)");
Console.WriteLine("(run with 'interactive' for a manual Host-mediated REPL)");
Console.WriteLine();

var report = BatchSim.Run(content, matchCount: 500, turnCap: 40, seed: 12345);
Console.WriteLine(
    $"winRateP1={report.WinRateP1:P1}  winRateP2={report.WinRateP2:P1}  " +
    $"draws={report.DrawRate:P1}  avgTurns={report.AverageTurns:F1}");
