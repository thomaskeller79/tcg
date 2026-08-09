using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Spells;

public class CastRiteTests
{
    [Fact]
    public void Damage_rite_hits_the_target_spends_mana_and_leaves_no_permanent()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 5, hand: [SpellFixtures.Firebolt]);
        var enemyChampion = match.State.ActorsOwnedBy(Fixtures.P2).Single();
        var actorCountBefore = match.State.AllActors.Count;

        var result = RulesEngine.Apply(match, new CastRiteCommand(Fixtures.P1, SpellFixtures.Firebolt, enemyChampion.Id));
        Assert.True(result.Accepted);

        Assert.Equal(12, enemyChampion.Life); // 15 - EffectAmount:3
        Assert.Empty(match.State.Players.Single(p => p.Id == Fixtures.P1).Hand);
        Assert.Equal(4, match.State.Players.Single(p => p.Id == Fixtures.P1).Mana); // 5 - ManaCost:1
        Assert.Equal(actorCountBefore, match.State.AllActors.Count); // no permanent left behind
    }

    [Fact]
    public void Heal_rite_restores_life()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 5, hand: [SpellFixtures.Mend]);
        var champion = match.State.ActorsOwnedBy(Fixtures.P1).Single();
        champion.Life = 10;

        var result = RulesEngine.Apply(match, new CastRiteCommand(Fixtures.P1, SpellFixtures.Mend, champion.Id));

        Assert.True(result.Accepted);
        Assert.Equal(14, champion.Life); // 10 + EffectAmount:4
    }

    [Fact]
    public void A_lethal_damage_rite_wins_the_match_via_the_existing_win_check_no_special_casing()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 5, hand: [SpellFixtures.Firebolt]);
        var enemyChampion = match.State.ActorsOwnedBy(Fixtures.P2).Single();
        enemyChampion.Life = 2; // Firebolt deals 3

        RulesEngine.Apply(match, new CastRiteCommand(Fixtures.P1, SpellFixtures.Firebolt, enemyChampion.Id));

        Assert.Equal(Fixtures.P1, match.State.Winner);
    }

    [Fact]
    public void Cannot_target_a_concealed_enemy_you_cannot_see()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 5, hand: [SpellFixtures.Firebolt]);
        var hiddenEnemy = new ActorId(match.State.AllActors.Max(a => a.Id.Value) + 1);
        match.State.AddActor(new CreatureState
        {
            Id = hiddenEnemy,
            Owner = Fixtures.P2,
            Definition = SpellFixtures.Grunt,
            Position = new HexCoord(3, 3),
            Layer = Layer.Below,
            Located = false, // D19: hidden by default
            Life = 5,
            CurrentAp = 3,
        });

        var result = RulesEngine.Apply(match, new CastRiteCommand(Fixtures.P1, SpellFixtures.Firebolt, hiddenEnemy));
        Assert.False(result.Accepted);
    }

    [Fact]
    public void Cannot_cast_without_enough_mana()
    {
        var match = SpellFixtures.ChampionWithMana(mana: 0, hand: [SpellFixtures.Firebolt]);
        var enemyChampion = match.State.ActorsOwnedBy(Fixtures.P2).Single();
        var result = RulesEngine.Apply(match, new CastRiteCommand(Fixtures.P1, SpellFixtures.Firebolt, enemyChampion.Id));
        Assert.False(result.Accepted);
    }
}
