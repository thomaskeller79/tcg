using Leyline.RulesCore;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Tests.TestSupport;

public static class HiddenLayerFixtures
{
    /// <summary>P1 Ground grunt at (0,0); P2 Below (submerged, hidden) grunt at (1,0), adjacent.</summary>
    public static Match GroundVsBelow() =>
        MatchFactory.CreateMatch(
            Fixtures.SmallBoard(),
            [Fixtures.P1, Fixtures.P2],
            [
                new CreaturePlacement(Fixtures.P1, Fixtures.Grunt, new HexCoord(0, 0)),
                new CreaturePlacement(Fixtures.P2, Fixtures.Grunt, new HexCoord(1, 0), Layer.Below),
            ],
            new MatchConfig(DefendRuleVariant.Exhaust),
            Fixtures.Content(),
            seed: 7);
}
