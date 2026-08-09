using Leyline.RulesCore.Commands;

namespace Leyline.DebugUi;

/// <summary>Ports InteractiveRepl.Describe() (tools/Leyline.SimHarness) for the web UI — same
/// index-and-label pattern, different presentation surface. Small enough that duplicating it
/// across two standalone tools beats inventing a shared project just for a formatter.</summary>
public static class CommandLabeler
{
    public static string Describe(Command command) => command switch
    {
        MoveCommand m => $"Move {m.Mover} -> {m.Destination}",
        DeclareCombatCommand d => $"Attack with {d.Attacker} into {d.TargetHex}",
        DeclareDefendersCommand d => $"Declare defenders {{{string.Join(",", d.Defenders)}}}",
        AssignDamageCommand a => $"Assign damage {{{string.Join(",", a.Assignment.Select(kv => $"{kv.Key}={kv.Value}"))}}}",
        ChooseUndefendedTargetCommand c => $"Undefended target -> {c.Target}",
        PassPriorityCommand => "Pass",
        EndPhaseCommand => "End phase",
        BondTerrainCommand b => $"Bond terrain at {b.Target}",
        DrawCardCommand => "Draw a card",
        CollapseNetworkCommand => "Collapse the leyline network",
        CastCreatureCommand c => $"Cast {c.Card} -> {c.Target}",
        CastRiteCommand c => $"Cast {c.Card} -> target {c.Target}",
        _ => command.GetType().Name,
    };

    /// <summary>Structured classification alongside the label. Move/Attack carry the mover's
    /// ActorId — the web UI's unit-selection filter matches on that, and drag-and-drop hit-
    /// tests TargetHex directly. Bond/Draw get short Kind tags too (no ActorId — they're
    /// PlayerId-scoped commands, but the UI attributes them to "the selected Champion" since
    /// there's exactly one per player) so selecting a Champion surfaces them as its abilities,
    /// same as Move/Attack for a selected Creature. CastCreature/CastRite carry Card, so
    /// selecting a card in hand works the same way; CastRite also carries TargetActorId so the
    /// client can label each option with the actor it would hit. Everything else (EndPhase,
    /// Pass, combat decisions) stays a global, always-shown action — it isn't tied to one
    /// selected thing.</summary>
    public static LegalCommandDto ToDto(int index, Command command) => command switch
    {
        MoveCommand m => new LegalCommandDto(index, Describe(m), "Move", m.Mover.Value, m.Destination, null),
        DeclareCombatCommand d => new LegalCommandDto(index, Describe(d), "Attack", d.Attacker.Value, d.TargetHex, null),
        BondTerrainCommand b => new LegalCommandDto(index, Describe(b), "Bond", null, b.Target, null),
        DrawCardCommand => new LegalCommandDto(index, Describe(command), "Draw", null, null, null),
        CollapseNetworkCommand => new LegalCommandDto(index, Describe(command), "Collapse", null, null, null),
        CastCreatureCommand c => new LegalCommandDto(index, Describe(c), "CastCreature", null, c.Target, c.Card.Value),
        CastRiteCommand c => new LegalCommandDto(index, Describe(c), "CastRite", null, null, c.Card.Value, c.Target.Value),
        _ => new LegalCommandDto(index, Describe(command), command.GetType().Name, null, null, null),
    };
}
