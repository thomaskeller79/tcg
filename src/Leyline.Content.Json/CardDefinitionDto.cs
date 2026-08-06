namespace Leyline.Content.Json;

internal sealed class CardDefinitionDto
{
    public string Id { get; set; } = "";
    public string Type { get; set; } = "";
    public string Name { get; set; } = "";
    public StatsDto Stats { get; set; } = new();
    public List<string> Abilities { get; set; } = [];
    public List<string> Keywords { get; set; } = [];
}

internal sealed class StatsDto
{
    public int Attack { get; set; }
    public int Life { get; set; }
    public int Ap { get; set; }
}
