using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Commands;

public abstract record Command(PlayerId Actor);

public sealed record MoveCommand(PlayerId Actor, ActorId Mover, HexCoord Destination) : Command(Actor);
public sealed record DeclareCombatCommand(PlayerId Actor, ActorId Attacker, HexCoord TargetHex) : Command(Actor);
public sealed record DeclareDefendersCommand(PlayerId Actor, CombatId Combat, IReadOnlyList<ActorId> Defenders) : Command(Actor);
public sealed record AssignDamageCommand(PlayerId Actor, CombatId Combat, IReadOnlyDictionary<ActorId, int> Assignment) : Command(Actor);
public sealed record ChooseUndefendedTargetCommand(PlayerId Actor, CombatId Combat, ActorId Target) : Command(Actor);
public sealed record RespondCommand(PlayerId Actor, StackItemId Response) : Command(Actor);
public sealed record PassPriorityCommand(PlayerId Actor) : Command(Actor);
public sealed record EndPhaseCommand(PlayerId Actor) : Command(Actor);
public sealed record BondTerrainCommand(PlayerId Actor, HexCoord Target) : Command(Actor);
