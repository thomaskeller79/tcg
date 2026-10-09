using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Commands;

/// <summary>Everything a Champion can submit (architecture.md §2.3: commands in, views out).</summary>
public abstract record Command(PlayerId Actor);

/// <summary>Cast a card from Hand (<c>Ability</c> = "cast") or activate a permanent's ability,
/// with one choice list per target selection, in order, and the mana it spends (D125) — null
/// when the payment is fully automatic.</summary>
public sealed record ActivateCommand(PlayerId Actor, ObjectId Source, string Ability, IReadOnlyList<IReadOnlyList<TargetChoice>> Targets, IReadOnlyList<ManaUnit>? Mana = null) : Command(Actor);

/// <summary>Pass priority (also how an Action phase ends, G4).</summary>
public sealed record PassCommand(PlayerId Actor) : Command(Actor);

/// <summary>G16: split an attacker's damage among the candidates.</summary>
public sealed record SplitDamageCommand(PlayerId Actor, IReadOnlyDictionary<ObjectId, int> Split) : Command(Actor);

public sealed record CommandResult(bool Accepted, string? Error)
{
    public static readonly CommandResult Ok = new(true, null);
    public static CommandResult Reject(string error) => new(false, error);
}
