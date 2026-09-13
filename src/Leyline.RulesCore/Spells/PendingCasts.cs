using Leyline.RulesCore.Events;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Spells;

/// <summary>A queued Creature cast's deferred resolution (D45/D46). The summoning target is
/// re-checked here, not assumed from cast time (D33: an illegal target fizzles just this
/// instruction — mana/card/Discard already spent at cast time are never refunded). The new
/// ActorId is allocated only on an actual summon, not at cast time, so a fizzled cast never
/// burns an id for a creature that never existed.</summary>
public sealed record PendingCreatureCast(PlayerId Owner, CardDefinitionId Card, HexCoord Target) : IPendingResolution
{
    public IEnumerable<EventIntent> Resolve(TrueState state)
    {
        if (!SpellPipeline.CanSummonTo(Owner, Target, state))
            yield break; // D33 fizzle.

        yield return new CreatureSummonedIntent(state.AllocateActorId(), Owner, Card, Target);
    }

    public string Describe(TrueState state) => $"{Owner} cast {Card} -> summon at {Target}";
}

/// <summary>A queued Spell's deferred resolution — target legality (and the caster's Champion
/// still existing to be the damage source) is re-checked at resolution, not cast time (D33).</summary>
public sealed record PendingSpellCast(PlayerId Caster, CardDefinitionId Card, ActorId Target) : IPendingResolution
{
    public IEnumerable<EventIntent> Resolve(TrueState state)
    {
        if (state.FindActor(Target) is null || !Query.IsVisibleTo(Target, Caster, state))
            yield break; // D33 fizzle.

        var casterChampion = state.AllActors.OfType<ChampionState>().FirstOrDefault(c => c.Owner == Caster);
        if (casterChampion is null)
            yield break; // The caster's Champion is gone by the time this resolves.

        var def = state.Content.Get(Card);
        EventIntent? intent = def.EffectId switch
        {
            SpellEffectIds.Damage => new DamageIntent(casterChampion.Id, Target, def.EffectAmount),
            SpellEffectIds.Heal => new HealIntent(Target, def.EffectAmount),
            _ => null,
        };
        if (intent is not null)
            yield return intent;
    }

    public string Describe(TrueState state) => $"{Caster} cast {Card} -> target {Target}";
}
