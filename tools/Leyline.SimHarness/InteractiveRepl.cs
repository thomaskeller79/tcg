using Leyline.Host;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Perception;
using Leyline.RulesCore.State;

namespace Leyline.SimHarness;

/// <summary>
/// Manual smoke-test through the real Host/Perception path (Slice 6) — a REPL for one or two
/// human-driven seats. Prints that seat's View + LegalCommands each step; never touches
/// TrueState directly, exactly as a human-driven seat-controller would be limited to.
/// </summary>
public static class InteractiveRepl
{
    public static void Run(ICardDefinitionRepository content)
    {
        var match = TestMatches.TwoVsTwoGruntsWithChampions(DefendRuleVariant.Exhaust, content);
        var p1Seat = new SeatId(1);
        var p2Seat = new SeatId(2);
        var seats = new Dictionary<SeatId, PlayerId> { [p1Seat] = new(1), [p2Seat] = new(2) };
        var host = new LocalHost(match, seats);

        Console.WriteLine("Interactive M1 sandbox — type a command number, or 'q' to quit.");

        while (true)
        {
            if (match.State.Winner is { } winner)
            {
                Console.WriteLine($"\n=== {winner} wins! ===");
                return;
            }

            var (seat, commands) = FindSeatWithCommands(host, p1Seat, p2Seat);
            if (commands.Count == 0)
            {
                Console.WriteLine("No seat has any legal commands — stopping.");
                return;
            }

            PrintView(seat, host.CurrentView(seat));
            PrintCommands(commands);

            Console.Write("> ");
            var line = Console.ReadLine();
            if (line is null || line.Equals("q", StringComparison.OrdinalIgnoreCase))
                return;

            if (!int.TryParse(line, out var index) || index < 0 || index >= commands.Count)
            {
                Console.WriteLine("Not a valid choice.");
                continue;
            }

            var result = host.Submit(seat, commands[index]);
            if (!result.Accepted)
                Console.WriteLine($"Rejected: {result.RejectionReason}");
        }
    }

    private static (SeatId Seat, IReadOnlyList<Command> Commands) FindSeatWithCommands(IHost host, SeatId a, SeatId b)
    {
        var commandsA = host.LegalCommands(a);
        return commandsA.Count > 0 ? (a, commandsA) : (b, host.LegalCommands(b));
    }

    private static void PrintView(SeatId seat, View view)
    {
        Console.WriteLine();
        Console.WriteLine($"--- Seat {seat.Value} | Turn {view.TurnNumber} | Active {view.ActivePlayer} | Phase {view.CurrentPhase} ---");
        foreach (var actor in view.Actors.OrderBy(a => a.Id.Value))
            Console.WriteLine($"  {actor.Id} owner={actor.Owner} pos={actor.Position} life={actor.Life} ap={actor.CurrentAp} layer={actor.Layer}");
    }

    private static void PrintCommands(IReadOnlyList<Command> commands)
    {
        for (var i = 0; i < commands.Count; i++)
            Console.WriteLine($"  [{i}] {Describe(commands[i])}");
    }

    private static string Describe(Command command) => command switch
    {
        MoveCommand m => $"Move {m.Mover} -> {m.Destination}",
        DeclareCombatCommand d => $"Attack with {d.Attacker} into {d.TargetHex}",
        DeclareDefendersCommand d => $"Declare defenders {{{string.Join(",", d.Defenders)}}}",
        AssignDamageCommand a => $"Assign damage {{{string.Join(",", a.Assignment.Select(kv => $"{kv.Key}={kv.Value}"))}}}",
        ChooseUndefendedTargetCommand c => $"Undefended target -> {c.Target}",
        PassPriorityCommand => "Pass",
        EndPhaseCommand => "End phase",
        BondTerrainCommand b => $"Bond terrain at {b.Target}",
        ChannelActCommand => "Channel: act as creature",
        _ => command.GetType().Name,
    };
}
