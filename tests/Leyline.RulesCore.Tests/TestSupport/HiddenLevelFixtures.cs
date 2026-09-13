using Leyline.RulesCore;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Tests.TestSupport;

public static class HiddenLevelFixtures
{
    /// <summary>P1 Surface grunt at (0,0); P2 Underground (submerged, hidden) grunt at (1,0), adjacent.</summary>
    public static Match SurfaceVsUnderground() =>
        MatchFactory.CreateMatch(
            Fixtures.SmallBoard(),
            [Fixtures.P1, Fixtures.P2],
            [
                new CreaturePlacement(Fixtures.P1, Fixtures.Grunt, new HexCoord(0, 0)),
                new CreaturePlacement(Fixtures.P2, Fixtures.Grunt, new HexCoord(1, 0), Level.Underground),
            ],
            Fixtures.Content(),
            seed: 7);
}
