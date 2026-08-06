using System.Text.Json;
using Leyline.RulesCore.State;

namespace Leyline.Content.Json;

/// <summary>
/// Loads card definitions from plain JSON files (A5: card data must be engine-neutral —
/// never Godot .tres or other engine-specific resource formats). This schema is a
/// provisional placeholder for M1: the real schema depends on unfinished Track A design
/// work (docs/architecture/card-data-and-editor.md steps 5-6) and should be expected to
/// change once that lands.
/// </summary>
public sealed class JsonCardDefinitionRepository : ICardDefinitionRepository
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    private readonly Dictionary<CardDefinitionId, CardDefinition> _definitions;

    private JsonCardDefinitionRepository(Dictionary<CardDefinitionId, CardDefinition> definitions)
    {
        _definitions = definitions;
    }

    public static JsonCardDefinitionRepository LoadFromDirectory(string path)
    {
        var definitions = new Dictionary<CardDefinitionId, CardDefinition>();
        foreach (var file in Directory.EnumerateFiles(path, "*.json").OrderBy(f => f, StringComparer.Ordinal))
        {
            var dto = JsonSerializer.Deserialize<CardDefinitionDto>(File.ReadAllText(file), Options)
                ?? throw new InvalidDataException($"Could not parse card definition: {file}");

            var id = new CardDefinitionId(dto.Id);
            definitions[id] = new CardDefinition(id, dto.Name, dto.Stats.Attack, dto.Stats.Life, dto.Stats.Ap, dto.Abilities);
        }
        return new JsonCardDefinitionRepository(definitions);
    }

    public static JsonCardDefinitionRepository FromDefinitions(IEnumerable<CardDefinition> definitions) =>
        new(definitions.ToDictionary(d => d.Id));

    public CardDefinition Get(CardDefinitionId id) =>
        _definitions.TryGetValue(id, out var def) ? def : throw new KeyNotFoundException($"No card definition '{id}'.");
}
