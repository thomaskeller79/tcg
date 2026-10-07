using Leyline.Content.Json;
using Leyline.Host;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Model;
using Leyline.Scenarios;

namespace Leyline.Host.Tests;

public class LocalHostTests
{
    private static IHost NewHost()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (!Directory.Exists(Path.Combine(dir!.FullName, "content", "cards")))
            dir = dir.Parent;
        var content = CardJson.LoadDirectory(Path.Combine(dir.FullName, "content", "cards"));
        var state = ScenarioLoader.Load("""
            map hexagon 2
            start A 0,1
            start B 0,-1
            home A radius=0
            home B radius=0
            neutral fill terrain.fire
            champion A champion.pyra
            champion B champion.thorn
            terraindeck A terrain.fire
            terraindeck B terrain.earth
            deck A spell.firebolt*3
            deck B spell.mend*3
            openinghand 2
            """, content);
        return LocalHost.TwoSeats(state);
    }

    [Fact]
    public void Each_seat_sees_only_its_own_hand()
    {
        var host = NewHost();
        var a = host.CurrentView(new SeatId(1));
        Assert.NotNull(a.Players.Single(p => p.Player == "A").Hand);
        Assert.Null(a.Players.Single(p => p.Player == "B").Hand);
        Assert.Equal(2, a.Players.Single(p => p.Player == "B").HandCount);
    }

    [Fact]
    public void A_seat_cannot_submit_for_the_other_player()
    {
        var host = NewHost();
        var result = host.Submit(new SeatId(2), new PassCommand(PlayerId.A));
        Assert.False(result.Accepted);
    }

    [Fact]
    public void Only_the_priority_holder_has_legal_commands_and_passing_hands_priority_over()
    {
        var host = NewHost();
        Assert.NotEmpty(host.LegalCommands(new SeatId(1)));
        Assert.Empty(host.LegalCommands(new SeatId(2)));
        Assert.True(host.Submit(new SeatId(1), new PassCommand(PlayerId.A)).Accepted);
        Assert.True(host.CurrentView(new SeatId(2)).YourPriority);
    }
}
