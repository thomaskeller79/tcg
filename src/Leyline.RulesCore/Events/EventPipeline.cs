using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Events;

/// <summary>Intent → pipeline → event (pillar 5's core discipline). The engine never mutates
/// TrueState directly; everything flows through here.</summary>
public sealed class EventPipeline
{
    private readonly List<IReplacementEffect> _replacementEffects = [];
    private readonly List<IEventSubscriber> _subscribers = [];

    /// <summary>Empty by default — MatchFactory registers checks in the order that matters
    /// (e.g. ChampionDeathCheck must observe a dying Champion before ZeroLifeDestructionCheck
    /// removes it from the board within the same fixed-point pass).</summary>
    private readonly List<IStateBasedCheck> _stateBasedChecks = [];

    public void RegisterStateBasedCheck(IStateBasedCheck check) => _stateBasedChecks.Add(check);
    public void RegisterSubscriber(IEventSubscriber subscriber) => _subscribers.Add(subscriber);
    public void RegisterReplacementEffect(IReplacementEffect effect) => _replacementEffects.Add(effect);

    public IReadOnlyList<IEvent> Process(EventIntent intent, TrueState state) =>
        ProcessBatch([intent], state);

    public IReadOnlyList<IEvent> ProcessBatch(IEnumerable<EventIntent> intents, TrueState state)
    {
        var applied = new List<IEvent>();
        foreach (var intent in intents)
            ProcessOne(intent, state, applied);
        RunStateBasedChecks(state, applied);
        return applied;
    }

    private void ProcessOne(EventIntent intent, TrueState state, List<IEvent> applied)
    {
        var resolved = FoldReplacements(intent, state);
        if (resolved is NoOpIntent)
            return;

        var evt = Materialize(resolved, state);
        evt.Apply(state);
        applied.Add(evt);

        foreach (var subscriber in _subscribers)
        {
            if (subscriber.ListensFor(evt, state))
                state.Stack.Push(subscriber.CreateResponse(evt, state));
        }
    }

    private EventIntent FoldReplacements(EventIntent intent, TrueState state)
    {
        var current = intent;
        foreach (var effect in _replacementEffects
                     .Where(e => e.AppliesTo(current, state))
                     .OrderBy(e => e.Priority))
        {
            current = effect.Replace(current, state);
        }
        return current;
    }

    private void RunStateBasedChecks(TrueState state, List<IEvent> applied)
    {
        bool any;
        do
        {
            any = false;
            foreach (var check in _stateBasedChecks)
            {
                foreach (var evt in check.Evaluate(state).ToList())
                {
                    evt.Apply(state);
                    applied.Add(evt);
                    any = true;
                }
            }
        } while (any);
    }

    private static IEvent Materialize(EventIntent intent, TrueState state) => intent switch
    {
        DamageIntent d => new DamageEvent(d.Source, d.Target, d.Amount),
        MoveIntent m => new ActorMovedEvent(m.Actor, state.GetActor(m.Actor).Position, m.Destination),
        ApChangeIntent a => new ActorApChangedEvent(a.Actor, a.NewAp),
        DestroyIntent d => new ActorDestroyedEvent(d.Actor),
        CombatDeclaredIntent c => new CombatDeclaredEvent(c.Combat, c.Attacker, c.TargetHex),
        DefendersDeclaredIntent d => new DefendersDeclaredEvent(d.Combat, d.Defenders),
        DamageAssignedIntent d => new DamageAssignedEvent(d.Combat, d.Assignment),
        UndefendedTargetChosenIntent u => new UndefendedTargetChosenEvent(u.Combat, u.Target),
        CombatResolvedIntent c => new CombatResolvedEvent(c.Combat),
        PhaseChangedIntent p => new PhaseChangedEvent(p.NewPhaseIndex),
        TurnAdvancedIntent t => new TurnAdvancedEvent(t.NewTurnNumber, t.NewActivePlayer),
        BondTerrainIntent b => new TerrainBondedEvent(b.Player, b.Target),
        OncePerTurnActionUsedIntent o => new OncePerTurnActionUsedEvent(o.Actor, o.ActionId),
        OncePerTurnActionsResetIntent o => new OncePerTurnActionsResetEvent(o.Actor),
        ManaChangeIntent m => new ManaChangedEvent(m.Player, m.NewMana),
        ActorRevealedIntent r => new ActorRevealedEvent(r.Actor),
        ActorConcealedIntent c => new ActorConcealedEvent(c.Actor),
        AddModifierIntent a => new AddModifierEvent(a.Modifier),
        RemoveModifierIntent r => new RemoveModifierEvent(r.Modifier),
        _ => throw new NotSupportedException($"No event mapping for intent {intent.GetType().Name}."),
    };
}
