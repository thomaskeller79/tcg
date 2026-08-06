using Leyline.Content.Json;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Tests.TestSupport;

public class JsonCardDefinitionRepositoryTests
{
    [Fact]
    public void LoadFromDirectory_reads_the_provisional_JSON_schema()
    {
        var contentDir = Path.Combine(AppContext.BaseDirectory, "TestSupport", "content");
        var repository = JsonCardDefinitionRepository.LoadFromDirectory(contentDir);

        var def = repository.Get(new CardDefinitionId("creature.test_grunt"));

        Assert.Equal("Grunt (test)", def.Name);
        Assert.Equal(3, def.Attack);
        Assert.Equal(5, def.Life);
        Assert.Equal(3, def.MaxAp);
        Assert.Equal(["core.move", "core.attack"], def.AbilityIds);
    }
}
