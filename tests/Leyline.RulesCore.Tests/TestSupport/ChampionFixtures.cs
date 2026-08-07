using Leyline.Content.Json;
using Leyline.RulesCore;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Tests.TestSupport;

public static class ChampionFixtures
{
    public static readonly CardDefinitionId Champion = new("test.champion");

    public static ICardDefinitionRepository Content(int championLife = 15, int championMaxAp = 2) =>
        JsonCardDefinitionRepository.FromDefinitions(
        [
            new CardDefinition(Fixtures.Grunt, "Grunt", Attack: 3, Life: 5, MaxAp: 3, AbilityIds: ["core.move", "core.attack"]),
            new CardDefinition(Champion, "Champion (test)", Attack: 2, Life: championLife, MaxAp: championMaxAp, AbilityIds: ["core.move", "core.attack", "champion.bond"]),
        ]);

    /// <summary>One attacking grunt for P1 next to P2's (otherwise undefended) Champion.</summary>
    public static Match AttackerVsChampion(int championLife = 15) =>
        MatchFactory.CreateMatch(
            Fixtures.SmallBoard(),
            [Fixtures.P1, Fixtures.P2],
            [new CreaturePlacement(Fixtures.P1, Fixtures.Grunt, new HexCoord(0, 0))],
            new MatchConfig(DefendRuleVariant.Exhaust),
            Content(championLife),
            seed: 3,
            champions: [new ChampionPlacement(Fixtures.P2, Champion, new HexCoord(1, 0))]);

    /// <summary>P1 has only a Champion (no creatures) — for exercising its AP/Move/Attack directly.</summary>
    public static Match ChampionOnlyMatch(int maxAp = 2, int championLife = 15) =>
        MatchFactory.CreateMatch(
            Fixtures.SmallBoard(),
            [Fixtures.P1, Fixtures.P2],
            creatures: [],
            new MatchConfig(DefendRuleVariant.Exhaust),
            Content(championLife, maxAp),
            seed: 5,
            champions: [new ChampionPlacement(Fixtures.P1, Champion, new HexCoord(0, 0))]);

    /// <summary>Adjacent Champions for both players — for exercising Champion-vs-Champion combat.</summary>
    public static Match ChampionVsChampion(int maxAp = 2, int championLife = 15) =>
        MatchFactory.CreateMatch(
            Fixtures.SmallBoard(),
            [Fixtures.P1, Fixtures.P2],
            creatures: [],
            new MatchConfig(DefendRuleVariant.Exhaust),
            Content(championLife, maxAp),
            seed: 6,
            champions:
            [
                new ChampionPlacement(Fixtures.P1, Champion, new HexCoord(0, 0)),
                new ChampionPlacement(Fixtures.P2, Champion, new HexCoord(1, 0)),
            ]);
}
