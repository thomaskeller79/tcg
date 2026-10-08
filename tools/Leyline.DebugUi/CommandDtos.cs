using Leyline.RulesCore.Commands;
using Leyline.RulesCore.Model;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;

namespace Leyline.DebugUi;

public sealed record TargetDto(int? Object, HexCoord? Hex, string? Slice, string? Entity, string Label);

/// <summary>One legal command, labelled for the UI. The client walks an activation step by
/// step — source, ability, each target selection in order, then the payment when it needs a
/// choice (D125) — by filtering this list (interaction-stack.md: pay → choose → target; not
/// submitting is the abort). <see cref="Payment"/> is null when the payment is automatic.</summary>
public sealed record LegalCommandDto(
    int Index,
    string Kind,
    int? Source,
    string? Ability,
    string Label,
    IReadOnlyList<IReadOnlyList<TargetDto>> Targets,
    string? Cost,
    string? Speed,
    IReadOnlyDictionary<int, int>? Split,
    string? Payment = null);

public sealed record SubmitRequest(int Seat, int Index);
public sealed record AutoPassRequest(int Seat, bool Enabled);
public sealed record SubmitResultDto(bool Accepted, string? Error);

public static class CommandDtos
{
    public static LegalCommandDto ToDto(TrueState state, int index, Command command)
    {
        switch (command)
        {
            case PassCommand:
                return new(index, "Pass", null, null, PassLabel(state, command.Actor), [], null, null, null);
            case SplitDamageCommand s:
                return new(index, "SplitDamage", null, null,
                    "Split: " + string.Join(", ", s.Split.Where(kv => kv.Value > 0).Select(kv => $"{kv.Value} → {state.NameOf(kv.Key)}")),
                    [], null, null, s.Split.ToDictionary(kv => kv.Key.Value, kv => kv.Value));
            case RedirectCommand r:
                return new(index, "Redirect", null, null, $"Go to {r.Choice} instead", [[Target(state, r.Choice)]], null, null, null);
            case CancelRedirectCommand:
                return new(index, "CancelRedirect", null, null, "Cancel (the cost stays paid)", [], null, null, null);
            case ActivateCommand a:
            {
                var source = state.Find<GameObject>(a.Source);
                string abilityName, cost, speed;
                if (source is CardObject card)
                {
                    var def = state.Def(card);
                    abilityName = $"Cast {def.Name}";
                    cost = def.Cost.ToString();
                    speed = def.Speed.ToString();
                }
                else
                {
                    var p = (Permanent)source!;
                    var ability = state.Abilities(p).First(x => x.Id == a.Ability);
                    abilityName = ability.Name;
                    cost = Costs.Effective(state, p, ability, a.Targets).ToString();
                    speed = ability.Speed.ToString();
                }
                var targets = a.Targets.Select(list => (IReadOnlyList<TargetDto>)list.Select(t => Target(state, t)).ToList()).ToList();
                var label = targets.Count == 0
                    ? abilityName
                    : $"{abilityName} → {string.Join("; ", targets.Select(l => l.Count == 0 ? "nothing" : string.Join(" + ", l.Select(t => t.Label))))}";
                var payment = a.Mana is { } mana ? string.Join(" + ", mana) : null;
                if (payment is not null)
                    label += $" · pay {payment}";
                return new(index, "Activate", a.Source.Value, a.Ability, label, targets, cost, speed, null, payment);
            }
            default:
                return new(index, command.GetType().Name, null, null, command.GetType().Name, [], null, null, null);
        }
    }

    private static string PassLabel(TrueState state, PlayerId actor)
    {
        if (state.Pending.Count > 0)
            return "Pass (let the top of Pending resolve)";
        return state.ActiveSeat.Champion() == actor ? "End your Action phase" : "Pass";
    }

    private static TargetDto Target(TrueState state, TargetChoice t)
    {
        if (t.Object is { } id)
            return new(id.Value, null, null, null, state.NameOf(id));
        var entity = t.HasEntity ? (t.Entity is { } e ? e.ToString() : "Neutral") : null;
        var label = $"{t.Hex}{(t.Slice is { } s ? " " + s : "")}{(entity is null ? "" : $" vs {(entity == "Neutral" ? "Neutral" : "Champion " + entity)}")}";
        return new(null, t.Hex, t.Slice?.ToString(), entity, label);
    }
}
