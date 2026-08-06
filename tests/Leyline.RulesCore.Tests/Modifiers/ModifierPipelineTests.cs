using Leyline.RulesCore;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Modifiers;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.TestSupport;

namespace Leyline.RulesCore.Tests.Modifiers;

/// <summary>
/// Covers the add/remove modifier mechanism designed in design-continuous-effects.md: adding
/// an IQueryModifier via ModifierPipeline, querying through the fold, end-of-turn expiry via
/// ExpireModifiersEffect, and precise removal by ModifierId.
/// </summary>
public class ModifierPipelineTests
{
    [Fact]
    public void Adding_a_delta_modifier_changes_the_query_result()
    {
        var match = Fixtures.Adjacent1v1(attack: 2);
        var actor = match.State.ActorsOwnedBy(Fixtures.P1).Single().Id;

        ModifierPipeline.AddModifier(match.State, match.Pipeline,
            id => new IntDeltaModifier(id, "Attack", actor, Delta: 1, ModifierDuration.UntilEndOfTurn));

        Assert.Equal(3, Query.ResolveAttack(actor, match.State));
    }

    [Fact]
    public void Adding_a_set_modifier_overrides_the_baseline_regardless_of_its_value()
    {
        var match = Fixtures.Adjacent1v1(attack: 2);
        var actor = match.State.ActorsOwnedBy(Fixtures.P1).Single().Id;

        ModifierPipeline.AddModifier(match.State, match.Pipeline,
            id => new IntSetModifier(id, "Attack", actor, Value: 0, ModifierDuration.UntilEndOfTurn));

        Assert.Equal(0, Query.ResolveAttack(actor, match.State));
    }

    [Fact]
    public void An_UntilEndOfTurn_modifier_is_gone_after_the_End_phase_and_the_query_reverts()
    {
        var match = Fixtures.Adjacent1v1(attack: 2);
        var actor = match.State.ActorsOwnedBy(Fixtures.P1).Single().Id;

        ModifierPipeline.AddModifier(match.State, match.Pipeline,
            id => new IntDeltaModifier(id, "Attack", actor, Delta: 1, ModifierDuration.UntilEndOfTurn));
        Assert.Equal(3, Query.ResolveAttack(actor, match.State));

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1));

        Assert.Empty(match.State.ActiveModifiers);
        Assert.Equal(2, Query.ResolveAttack(actor, match.State));
    }

    [Fact]
    public void A_Permanent_modifier_survives_the_End_phase()
    {
        var match = Fixtures.Adjacent1v1(attack: 2);
        var actor = match.State.ActorsOwnedBy(Fixtures.P1).Single().Id;

        ModifierPipeline.AddModifier(match.State, match.Pipeline,
            id => new IntDeltaModifier(id, "Attack", actor, Delta: 1, ModifierDuration.Permanent));

        RulesEngine.Apply(match, new EndPhaseCommand(Fixtures.P1));

        Assert.Single(match.State.ActiveModifiers);
        Assert.Equal(3, Query.ResolveAttack(actor, match.State));
    }

    [Fact]
    public void Modifiers_apply_in_append_order_so_outcome_depends_on_cast_order()
    {
        // Worked example from design-continuous-effects.md: "+1 Attack" and "Attack becomes 0"
        // resolve differently depending on which was added first, since there's no layer
        // system — just append order.
        var deltaFirst = Fixtures.Adjacent1v1(attack: 2);
        var deltaFirstActor = deltaFirst.State.ActorsOwnedBy(Fixtures.P1).Single().Id;
        ModifierPipeline.AddModifier(deltaFirst.State, deltaFirst.Pipeline,
            id => new IntDeltaModifier(id, "Attack", deltaFirstActor, 1, ModifierDuration.UntilEndOfTurn));
        ModifierPipeline.AddModifier(deltaFirst.State, deltaFirst.Pipeline,
            id => new IntSetModifier(id, "Attack", deltaFirstActor, 0, ModifierDuration.UntilEndOfTurn));
        Assert.Equal(0, Query.ResolveAttack(deltaFirstActor, deltaFirst.State));

        var setFirst = Fixtures.Adjacent1v1(attack: 2);
        var setFirstActor = setFirst.State.ActorsOwnedBy(Fixtures.P1).Single().Id;
        ModifierPipeline.AddModifier(setFirst.State, setFirst.Pipeline,
            id => new IntSetModifier(id, "Attack", setFirstActor, 0, ModifierDuration.UntilEndOfTurn));
        ModifierPipeline.AddModifier(setFirst.State, setFirst.Pipeline,
            id => new IntDeltaModifier(id, "Attack", setFirstActor, 1, ModifierDuration.UntilEndOfTurn));
        Assert.Equal(1, Query.ResolveAttack(setFirstActor, setFirst.State));
    }

    [Fact]
    public void Removing_a_modifier_by_id_leaves_other_modifiers_intact()
    {
        var match = Fixtures.Adjacent1v1(attack: 2);
        var actor = match.State.ActorsOwnedBy(Fixtures.P1).Single().Id;
        ModifierId? firstId = null;

        ModifierPipeline.AddModifier(match.State, match.Pipeline, id =>
        {
            firstId = id;
            return new IntDeltaModifier(id, "Attack", actor, Delta: 1, ModifierDuration.Permanent);
        });
        ModifierPipeline.AddModifier(match.State, match.Pipeline,
            id => new IntDeltaModifier(id, "Attack", actor, Delta: 10, ModifierDuration.Permanent));

        Assert.Equal(13, Query.ResolveAttack(actor, match.State));

        ModifierPipeline.RemoveModifier(match.State, match.Pipeline, firstId!.Value);

        Assert.Single(match.State.ActiveModifiers);
        Assert.Equal(12, Query.ResolveAttack(actor, match.State));
    }
}
