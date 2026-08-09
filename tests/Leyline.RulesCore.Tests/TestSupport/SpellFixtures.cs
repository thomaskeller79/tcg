using Leyline.Content.Json;
using Leyline.RulesCore;
using Leyline.RulesCore.Spells;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Tests.TestSupport;

public static class SpellFixtures
{
    public static readonly CardDefinitionId Champion = new("test.champion");
    public static readonly CardDefinitionId Grunt = new("test.grunt");
    public static readonly CardDefinitionId Firebolt = new("test.firebolt"); // Rite: 3 damage
    public static readonly CardDefinitionId Mend = new("test.mend"); // Rite: 4 heal

    public static ICardDefinitionRepository Content() =>
        JsonCardDefinitionRepository.FromDefinitions(
        [
            new CardDefinition(Champion, "Champion (test)", Attack: 2, Life: 15, MaxAp: 7,
                AbilityIds: ["core.move", "core.attack", "champion.bond", "champion.draw", "champion.collapse"], Type: CardType.Champion),
            new CardDefinition(Grunt, "Grunt", Attack: 3, Life: 5, MaxAp: 3,
                AbilityIds: ["core.move", "core.attack"], Type: CardType.Creature, ManaCost: 2),
            new CardDefinition(Firebolt, "Firebolt", Attack: 0, Life: 0, MaxAp: 0,
                AbilityIds: [], Type: CardType.Rite, ManaCost: 1, EffectId: RiteEffectIds.Damage, EffectAmount: 3),
            new CardDefinition(Mend, "Mend", Attack: 0, Life: 0, MaxAp: 0,
                AbilityIds: [], Type: CardType.Rite, ManaCost: 1, EffectId: RiteEffectIds.Heal, EffectAmount: 4),
        ]);

    private static Board BoardWithTerrainAt10()
    {
        var cells = new List<Cell>();
        for (var q = 0; q < 4; q++)
            for (var r = 0; r < 4; r++)
                cells.Add(new Cell { Coord = new HexCoord(q, r) });
        cells.First(c => c.Coord == new HexCoord(1, 0)).Terrain = "basic";
        return new Board(cells);
    }

    /// <summary>P1 Champion at (0,0), P2 Champion at (3,3), terrain at (1,0) — bond it yourself
    /// to exercise the real mana pipeline, or use ChampionWithMana below to skip straight to
    /// spell-casting with mana injected directly (test isolation from TerrainNetworkTests,
    /// which already covers bonding/connection in depth).</summary>
    public static Match TwoChampionsWithTerrain() =>
        MatchFactory.CreateMatch(
            BoardWithTerrainAt10(),
            [Fixtures.P1, Fixtures.P2],
            creatures: [],
            new MatchConfig(DefendRuleVariant.Exhaust),
            Content(),
            seed: 9,
            champions:
            [
                new ChampionPlacement(Fixtures.P1, Champion, new HexCoord(0, 0)),
                new ChampionPlacement(Fixtures.P2, Champion, new HexCoord(3, 3)),
            ]);

    public static Match ChampionWithMana(
        int mana = 10,
        IReadOnlyList<CardDefinitionId>? hand = null,
        IReadOnlyList<CardDefinitionId>? library = null)
    {
        var match = MatchFactory.CreateMatch(
            BoardWithTerrainAt10(),
            [Fixtures.P1, Fixtures.P2],
            creatures: [],
            new MatchConfig(DefendRuleVariant.Exhaust),
            Content(),
            seed: 10,
            champions:
            [
                new ChampionPlacement(Fixtures.P1, Champion, new HexCoord(0, 0)),
                new ChampionPlacement(Fixtures.P2, Champion, new HexCoord(3, 3)),
            ],
            playerSetups: [new PlayerSetup(Fixtures.P1, library ?? [], hand ?? [])]);

        match.State.Players.First(p => p.Id == Fixtures.P1).Mana = mana;

        // Bond (1,0) directly (match setup, same footing as initial placement) so there's a
        // real connected/producing summon target — mana itself is injected above rather than
        // earned through RefreshManaEffect, since the bond/connect pipeline is already covered
        // in depth by TerrainNetworkTests and isn't what these spell-casting tests exercise.
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).OfType<ChampionState>().Single();
        champion.Network.Bond(new HexCoord(1, 0));

        return match;
    }
}
