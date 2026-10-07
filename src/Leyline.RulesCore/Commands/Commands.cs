using Leyline.RulesCore.Model;
using Leyline.RulesCore.State;

namespace Leyline.RulesCore.Commands;

/// <summary>Everything a Champion can submit (architecture.md §2.3: commands in, views out).</summary>
public abstract record Command(PlayerId Actor);

/// <summary>Cast a card from Hand (<c>Ability</c> = "cast") or activate a permanent's ability,
/// with one choice list per target selection, in order.</summary>
public sealed record ActivateCommand(PlayerId Actor, ObjectId Source, string Ability, IReadOnlyList<IReadOnlyList<TargetChoice>> Targets) : Command(Actor);

/// <summary>Pass priority (also how an Action phase ends, G4).</summary>
public sealed record PassCommand(PlayerId Actor) : Command(Actor);

/// <summary>G16: split an attacker's damage among the candidates.</summary>
public sealed record SplitDamageCommand(PlayerId Actor, IReadOnlyDictionary<ObjectId, int> Split) : Command(Actor);

/// <summary>D68: pick another location after one proved illegal against true state.</summary>
public sealed record RedirectCommand(PlayerId Actor, TargetChoice Choice) : Command(Actor);

/// <summary>D68: give up after a failed location; the cost stays paid.</summary>
public sealed record CancelRedirectCommand(PlayerId Actor) : Command(Actor);

public sealed record CommandResult(bool Accepted, string? Error)
{
    public static readonly CommandResult Ok = new(true, null);
    public static CommandResult Reject(string error) => new(false, error);
}
