using Leyline.Content.Json;
using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.Scenarios;

namespace Leyline.Scenarios.Tests;

public class ScenarioLoaderTests
{
    private static readonly string RepoRoot = FindRoot();

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (!Directory.Exists(Path.Combine(dir!.FullName, "content", "cards")))
            dir = dir.Parent;
        return dir.FullName;
    }

    private static ICardDefinitionRepository Content => CardJson.LoadDirectory(Path.Combine(RepoRoot, "content", "cards"));

    [Fact]
    public void Every_shipped_scenario_loads_and_ends_setup_in_Champion_As_Action_phase()
    {
        var dir = Path.Combine(RepoRoot, "tools", "Leyline.DebugUi", "scenarios");
        var files = Directory.GetFiles(dir, "*.scenario");
        Assert.NotEmpty(files);
        foreach (var file in files)
        {
            var state = ScenarioLoader.LoadFromFile(file, Content);
            Assert.Equal(Phase.Action, state.Phase);
            Assert.NotNull(state.ChampionOf(PlayerId.A));
            Assert.NotNull(state.ChampionOf(PlayerId.B));
        }
    }

    [Fact]
    public void Every_card_references_only_known_cards()
    {
        var content = Content;
        foreach (var def in content.All)
        {
            foreach (var ability in def.Abilities)
                foreach (var ins in ability.Instructions.Where(i => i.Card is not null))
                    Assert.True(content.Contains(ins.Card!), $"{def.Id} creates unknown {ins.Card}");
        }
    }

    [Fact]
    public void Unknown_keywords_fail_with_a_line_number()
    {
        var ex = Assert.Throws<InvalidDataException>(() => ScenarioLoader.Load("map hexagon 2\nbogus 1", Content));
        Assert.StartsWith("line 2:", ex.Message);
    }

    [Fact]
    public void Cost_syntax_parses_mana_ap_flavors_and_life()
    {
        var cost = CardJson.ParseCost("2FW 3~AP 1Life");
        Assert.Equal(4, cost.Mana.Count);
        Assert.Equal(2, cost.Mana.Count(m => m.IsGeneric));
        Assert.Equal(3, cost.Ap);
        Assert.Equal(ApFlavor.Done, cost.Flavor);
        Assert.Equal(1, cost.Life);
    }
}
