using Leyline.RulesCore.Commands;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests;

/// <summary>Multi-system integration: Bond, Combat, and the win-check all interoperating in
/// one match, not just individually unit-tested.</summary>
public class GoldenPathTests
{
    [Fact]
    public void Bond_then_win_via_combat_all_interoperate_in_one_match()
    {
        var match = TerrainFixtures.ChampionWithTerrainChain(championMaxAp: 3); // P1 champion at (0,0), terrain at (1,0)/(2,0)

        // Turn 1 (P1): bond terrain, costing 2 of the Champion's 3 starting AP.
        var bonded = RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(1, 0)));
        Assert.True(bonded.Accepted);

        // Bonding again this turn is illegal — the `*` once-per-turn gate, independent of AP remaining.
        var doubleSpend = RulesEngine.Apply(match, new BondTerrainCommand(Fixtures.P1, new HexCoord(2, 0)));
        Assert.False(doubleSpend.Accepted);

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1)); // -> P2's turn
        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P2)); // -> back to P1: mana + AP refresh, once-per-turn gate resets

        var p1 = match.State.Players.Single(p => p.Id == Fixtures.P1);
        Assert.Equal(1, p1.Mana); // the bonded node is now connected and producing

        // Turn 2 (P1): the Champion already has its refreshed AP — no command needed to act —
        // walk it next to P2's champion and kill it. Combat needed zero changes to allow this (Slice 2).
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        Assert.True(champion.CurrentAp > 0);

        // Place a weak enemy Champion directly adjacent so one undefended hit is lethal.
        var enemyChampionId = match.State.AllocateActorId();
        var enemyPosition = new HexCoord(0, 1);
        match.State.AddActor(new ChampionState
        {
            Id = enemyChampionId,
            Owner = Fixtures.P2,
            Definition = ChampionFixtures.Champion,
            Position = enemyPosition,
            Life = 1,
            CurrentAp = 0,
        });

        RulesEngine.Apply(match, new DeclareCombatCommand(Fixtures.P1, champion.Id, enemyPosition));
        var combatId = match.State.ActiveCombats.Single().Id;
        RulesEngine.Apply(match, new DeclareDefendersCommand(Fixtures.P2, combatId, []));
        RulesEngine.Apply(match, new ChooseUndefendedTargetCommand(Fixtures.P1, combatId, enemyChampionId));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P2));
        RulesEngine.Apply(match, new PassPriorityCommand(Fixtures.P1));

        Assert.Equal(Fixtures.P1, match.State.Winner);
    }
}
