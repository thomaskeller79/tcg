using Leyline.RulesCore.State;
using Leyline.SimHarness;

var content = TestMatches.DefaultContent();

if (args is ["interactive"])
{
    InteractiveRepl.Run(content);
    return;
}

Console.WriteLine("Leyline M1 sandbox — D15 defend-rule comparison (2 grunts vs 2 grunts, turn cap 40)");
Console.WriteLine("(run with 'interactive' for a manual Host-mediated REPL)");
Console.WriteLine();

foreach (var variant in Enum.GetValues<DefendRuleVariant>())
{
    var report = BatchSim.Run(variant, content, matchCount: 500, turnCap: 40, seed: 12345);
    Console.WriteLine(
        $"{variant,-16} winRateP1={report.WinRateP1:P1}  winRateP2={report.WinRateP2:P1}  " +
        $"draws={report.DrawRate:P1}  avgTurns={report.AverageTurns:F1}");
}
