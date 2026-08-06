using Leyline.RulesCore.Commands;

namespace Leyline.Host.Tests;

/// <summary>
/// Deliberately never references Leyline.RulesCore (no project reference — see the .csproj)
/// so a bug that "works because we're in-process" shows up here as a compile error, not a
/// runtime leak. Every assertion talks only in Command/View/ObservedEvent, exactly what a
/// future RemoteHost's wire protocol would also be limited to (A1).
/// </summary>
public class LocalHostTests
{
    [Fact]
    public void Submitting_a_command_under_the_wrong_seat_is_rejected()
    {
        var (host, p1Seat, p2Seat) = LocalHostFactory.CreateTwoGruntSkirmish();
        var moveCommand = host.LegalCommands(p1Seat).OfType<MoveCommand>().First();

        var result = host.Submit(p2Seat, moveCommand); // P1's command submitted under P2's seat

        Assert.False(result.Accepted);
    }

    [Fact]
    public void Opponents_view_never_contains_a_below_layer_occupant_before_it_is_revealed()
    {
        var (host, p1Seat, _) = LocalHostFactory.CreateTwoGruntSkirmish();

        var view = host.CurrentView(p1Seat);

        Assert.Single(view.Actors); // only P1's own Ground grunt — P2's submerged grunt is hidden
        Assert.All(view.Cells, c => Assert.Empty(c.Below));
    }

    [Fact]
    public void The_owners_view_does_contain_their_own_submerged_creature()
    {
        var (host, _, p2Seat) = LocalHostFactory.CreateTwoGruntSkirmish();

        var view = host.CurrentView(p2Seat);

        Assert.Equal(2, view.Actors.Count); // both grunts — P2 can see their own submerged one
    }

    [Fact]
    public void Submit_returns_a_View_and_ObservedEvents_for_an_accepted_command()
    {
        var (host, p1Seat, _) = LocalHostFactory.CreateTwoGruntSkirmish();
        var commands = host.LegalCommands(p1Seat);
        Assert.NotEmpty(commands);

        var result = host.Submit(p1Seat, commands.OfType<MoveCommand>().First());

        Assert.True(result.Accepted);
        Assert.NotNull(result.View);
        Assert.NotEmpty(result.Events);
    }

    [Fact]
    public void An_unknown_seat_is_rejected_rather_than_throwing()
    {
        var (host, p1Seat, _) = LocalHostFactory.CreateTwoGruntSkirmish();
        var unknownSeat = new SeatId(99);

        Assert.Throws<ArgumentException>(() => host.CurrentView(unknownSeat));
    }
}
