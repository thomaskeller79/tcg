using Leyline.Content.Json;
using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Model;
using static Leyline.RulesCore.Tests.Support.Game;

namespace Leyline.RulesCore.Tests;

/// <summary>D125: generic and disjunctive pips, colorless and disjunctive pool mana, and the
/// semi-automatic payment by domination.</summary>
public class ManaTests
{
    private static readonly ManaUnit Fire = ManaUnit.Of(Element.Fire);
    private static readonly ManaUnit Air = ManaUnit.Of(Element.Air);
    private static readonly ManaUnit Metal = ManaUnit.Of(Element.Metal);
    private static readonly ManaUnit FireMetal = ManaUnit.Of(Element.Fire, Element.Metal);

    private static IReadOnlyList<IReadOnlyList<ManaUnit>> Options(string cost, params (ManaUnit Unit, int Count)[] pool) =>
        ManaPayment.Options(pool.ToDictionary(p => p.Unit, p => p.Count), CardJson.ParseCost(cost).Mana);

    [Fact]
    public void Colored_pips_are_paid_before_generic_so_the_generic_takes_what_is_left()
    {
        var only = Assert.Single(Options("1F", (Fire, 1), (Air, 1)));
        Assert.Equal([Fire, Air], only.OrderBy(u => u.Colors));
    }

    [Fact]
    public void Colorless_mana_always_pays_a_generic_pip_and_never_a_colored_one()
    {
        Assert.Equal([ManaUnit.Colorless], Assert.Single(Options("1", (ManaUnit.Colorless, 1), (Fire, 1))));
        Assert.Empty(Options("F", (ManaUnit.Colorless, 3)));
    }

    [Fact]
    public void A_one_Element_pool_pays_everything_automatically()
    {
        Assert.Equal(3, Assert.Single(Options("2F", (Fire, 4))).Count);
    }

    [Fact]
    public void Two_incomparable_Elements_for_a_generic_pip_are_the_players_choice()
    {
        var options = Options("1", (Fire, 1), (Air, 1));
        Assert.Equal(2, options.Count);
        Assert.Contains(options, o => o.SequenceEqual([Fire]));
        Assert.Contains(options, o => o.SequenceEqual([Air]));
    }

    [Fact]
    public void Dual_mana_dominates_its_Elements_so_the_single_Element_is_spent_first()
    {
        Assert.Equal([Fire], Assert.Single(Options("F", (FireMetal, 1), (Fire, 1))));
        Assert.Equal([Fire], Assert.Single(Options("1", (FireMetal, 1), (Fire, 1))));
        Assert.Equal([FireMetal], Assert.Single(Options("M", (FireMetal, 1), (Fire, 1))));
    }

    [Fact]
    public void A_hybrid_pip_takes_either_Element()
    {
        var options = Options("F/A", (Fire, 1), (Air, 1));
        Assert.Equal(2, options.Count);
        Assert.Empty(Options("F/A", (Metal, 2)));
    }

    [Fact]
    public void A_choice_that_leaves_the_cost_unpayable_is_never_offered()
    {
        // {Fire/Air}{Fire/Metal} from Fire + Air: paying the first with Fire is a dead end, so the
        // only complete payment remains — and it is automatic.
        var only = Assert.Single(Options("F/A F/M", (Fire, 1), (Air, 1)));
        Assert.Equal([Fire, Air], only.OrderBy(u => u.Colors));
    }

    [Fact]
    public void The_cost_syntax_reads_hybrid_pips()
    {
        var cost = CardJson.ParseCost("1F/AW");
        Assert.Equal([ManaPip.Generic, ManaPip.Of(Element.Fire, Element.Air), ManaPip.Of(Element.Water)], cost.Mana);
        Assert.Equal("{1} {Fire/Air} {Water}", cost.ToString());
    }

    [Fact]
    public void A_cast_needing_a_choice_is_offered_once_per_payment_and_refused_without_one()
    {
        var g = Load("""
            handcard A spell.study
            mana A Air 2
            """); // plus the 1 Fire of A's bonded home tile
        var card = g.HandCard(A, "spell.study");
        var casts = RulesEngine.LegalCommands(g.State, A).OfType<ActivateCommand>().Where(c => c.Source == card.Id).ToList();
        Assert.Equal(2, casts.Count); // {2}: Fire+Air or Air+Air
        Assert.All(casts, c => Assert.NotNull(c.Mana));

        var refused = g.Apply(new ActivateCommand(A, card.Id, DefaultAbilities.Cast, []));
        Assert.Equal("Choose how to pay.", refused.Error);

        g.Ok(g.Apply(casts.Single(c => c.Mana!.Count(u => u == Air) == 2)));
        Assert.Equal(1, g.Champion(A).Pool!.Amounts[Fire]);
    }
}
