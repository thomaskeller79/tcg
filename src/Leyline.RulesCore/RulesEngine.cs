using Leyline.RulesCore.Combat;
using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Queries;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Terrain;
using Leyline.RulesCore.Turns;

namespace Leyline.RulesCore;

/// <summary>The two required first-class queries (PLAN.md M1 scope): legal-command
/// enumeration and command application. Both LocalHost and the SimHarness's batch-sim
/// bypass call through here.</summary>
public static class RulesEngine
{
    public static IReadOnlyList<Command> LegalCommands(Match match, PlayerId actor)
    {
        var state = match.State;
        var commands = new List<Command>();

        if (state.Winner is not null)
            return commands; // match is over — D9

        if (state.ActiveWindow is { } window)
        {
            if (window.CurrentPriority == actor)
                commands.Add(new PassPriorityCommand(actor));
            return commands;
        }

        foreach (var combat in state.ActiveCombats)
        {
            var attackerOwner = state.GetActor(combat.Attacker).Owner;
            switch (combat.Phase)
            {
                case CombatPhase.AwaitingDefenders when actor != attackerOwner:
                    return CombatPipeline.LegalDefenderDeclarations(state, combat, actor).Cast<Command>().ToList();
                case CombatPhase.AwaitingAssignment when actor == attackerOwner:
                    return CombatPipeline.LegalDamageAssignments(state, combat, actor).Cast<Command>().ToList();
                case CombatPhase.AwaitingUndefendedChoice when actor == attackerOwner:
                    return CombatPipeline.LegalUndefendedChoices(state, combat, actor).Cast<Command>().ToList();
                case CombatPhase.AwaitingDefenders:
                case CombatPhase.AwaitingAssignment:
                case CombatPhase.AwaitingUndefendedChoice:
                    return commands; // a decision is pending, but not this player's
            }
        }

        if (actor != state.ActivePlayer || !state.CurrentPhase.OffersPriority)
            return commands;

        foreach (var actorState in state.ActorsOwnedBy(actor))
        {
            foreach (var dest in Query.ResolveLegalMoveTargets(actorState.Id, state))
                commands.Add(new MoveCommand(actor, actorState.Id, dest));
            foreach (var hex in Query.ResolveLegalAttackTargets(actorState.Id, state))
                commands.Add(new DeclareCombatCommand(actor, actorState.Id, hex));
        }

        commands.AddRange(TerrainPipeline.LegalBonds(state, actor));

        commands.Add(new EndPhaseCommand(actor));
        return commands;
    }

    public static CommandResult Apply(Match match, Command command)
    {
        var state = match.State;
        var pipeline = match.Pipeline;

        if (state.Winner is not null)
            return CommandResult.Reject("The match is already over.");

        return command switch
        {
            MoveCommand m => ApplyMove(state, pipeline, m),
            DeclareCombatCommand d => CombatPipeline.DeclareCombat(state, pipeline, d),
            DeclareDefendersCommand d => CombatPipeline.DeclareDefenders(state, pipeline, d),
            AssignDamageCommand a => CombatPipeline.AssignDamage(state, pipeline, a),
            ChooseUndefendedTargetCommand c => CombatPipeline.ChooseUndefendedTarget(state, pipeline, c),
            PassPriorityCommand p => CombatPipeline.Pass(state, pipeline, p),
            RespondCommand => CommandResult.Reject("No respondable content exists in M1."),
            EndPhaseCommand e => TurnEngine.EndPhase(state, pipeline, e),
            BondTerrainCommand b => TerrainPipeline.Bond(state, pipeline, b),
            _ => CommandResult.Reject($"Unhandled command type {command.GetType().Name}."),
        };
    }

    private static CommandResult ApplyMove(TrueState state, Events.EventPipeline pipeline, MoveCommand cmd)
    {
        var mover = state.GetActor(cmd.Mover);
        if (mover.Owner != cmd.Actor)
            return CommandResult.Reject("Not your creature.");
        if (!Query.ResolveLegalMoveTargets(cmd.Mover, state).Contains(cmd.Destination))
            return CommandResult.Reject("Illegal move destination.");

        var cost = Query.ResolveMoveCost(cmd.Mover, cmd.Destination, state);
        var events = new List<Events.IEvent>();
        events.AddRange(pipeline.Process(new Events.MoveIntent(cmd.Mover, cmd.Destination), state));
        events.AddRange(pipeline.Process(new Events.ApChangeIntent(cmd.Mover, cost.Apply(mover.CurrentAp)), state));

        // D19 (provisional): a submerged actor re-conceals the moment it leaves the hex it
        // was revealed on. A no-op if it was never revealed in the first place.
        if (mover.Layer == Layer.Below)
            events.AddRange(pipeline.Process(new Events.ActorConcealedIntent(cmd.Mover), state));

        return CommandResult.Accept(events);
    }
}
