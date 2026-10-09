namespace Leyline.RulesCore.Model;

/// <summary>
/// The default abilities written on every card of a type (D10, D106, glossary "Default ability").
/// Costs here are the printed baseline; context-dependent prices — a bonded Champion or Companion
/// doubling Move/Attack/Defend (D9, D49, D117), the Defender keyword (D116), static abilities on
/// subtypes (D129) — are applied by the cost query (Rules.Costs), not baked in here.
/// </summary>
public static class DefaultAbilities
{
    public const string Move = "move";
    public const string Attack = "attack";
    public const string Defend = "defend";
    public const string Equip = "equip";
    public const string Unequip = "unequip";
    public const string Ascend = "ascend";
    public const string Descend = "descend";
    public const string Bond = "bond";
    public const string Draw = "draw";
    public const string Collapse = "collapse";

    /// <summary>The pseudo-ability id a card in Hand is cast with.</summary>
    public const string Cast = "cast";

    public static readonly AbilityDefinition MoveAbility = new()
    {
        Id = Move, Subtypes = ["Move"], Name = "Move", Builtin = BuiltinAbility.Move,
        Cost = Cost.ApOnly(1), Speed = Speed.Slow, Physical = true, Duration = 0,
        Text = "Move to an adjacent terrain.",
    };

    public static readonly AbilityDefinition AttackAbility = new()
    {
        Id = Attack, Subtypes = ["Attack"], Name = "Attack", Builtin = BuiltinAbility.Attack,
        Cost = Cost.ApOnly(3, ApFlavor.Done), Speed = Speed.Slow, Physical = true, Duration = 0,
        Text = "Attack a terrain, a Slice and an entity.",
    };

    public static readonly AbilityDefinition DefendAbility = new()
    {
        Id = Defend, Subtypes = ["Defend"], Name = "Defend", Builtin = BuiltinAbility.Defend,
        Cost = Cost.ApOnly(1, ApFlavor.Done), Speed = Speed.Quick, Physical = true, Duration = 0,
        Text = "Defend target attack.",
    };

    public static readonly AbilityDefinition EquipAbility = new()
    {
        Id = Equip, Subtypes = ["Equip"], Name = "Equip", Builtin = BuiltinAbility.Equip,
        Cost = Cost.ApOnly(2), Speed = Speed.Slow,
        Text = "Equip target Item sharing this location.",
    };

    public static readonly AbilityDefinition UnequipAbility = new()
    {
        Id = Unequip, Subtypes = ["Unequip"], Name = "Un-equip", Builtin = BuiltinAbility.Unequip,
        Cost = Cost.ApOnly(0), Speed = Speed.Slow,
        Text = "Drop a carried Item.",
    };

    public static readonly AbilityDefinition AscendAbility = new()
    {
        Id = Ascend, Subtypes = ["Ascend"], Name = "Ascend", Builtin = BuiltinAbility.Ascend,
        Cost = Cost.ApOnly(3), Speed = Speed.Slow, Physical = true, Duration = 0,
        Text = "Root → Ground.",
    };

    public static readonly AbilityDefinition DescendAbility = new()
    {
        Id = Descend, Subtypes = ["Descend"], Name = "Descend", Builtin = BuiltinAbility.Descend,
        Cost = Cost.ApOnly(3), Speed = Speed.Slow, Physical = true, Duration = 0,
        Text = "Ground → Root.",
    };

    public static readonly AbilityDefinition DrawAbility = new()
    {
        Id = Draw, Subtypes = ["Draw"], Name = "Draw", Builtin = BuiltinAbility.Draw,
        Cost = Cost.ApOnly(5, ApFlavor.OncePerCycle), Speed = Speed.Slow,
        Text = "Draw a card.",
    };

    public static readonly AbilityDefinition CollapseAbility = new()
    {
        Id = Collapse, Subtypes = ["Collapse"], Name = "Collapse Network", Builtin = BuiltinAbility.Collapse,
        Cost = Cost.ApOnly(0), Speed = Speed.Slow,
        Text = "Drop every bond of this root.",
    };

    public static AbilityDefinition BondAbility(int ap) => new()
    {
        Id = Bond, Subtypes = ["Bond"], Name = "Bond", Builtin = BuiltinAbility.Bond,
        Cost = Cost.ApOnly(ap, ApFlavor.OncePerCycle), Speed = Speed.Slow,
        Text = "Bond a reachable terrain.",
    };

    /// <summary>Every activated and triggered ability a permanent of this card has: the type's
    /// defaults (minus RemovedDefaults) followed by the printed ones, in printed order.</summary>
    public static IReadOnlyList<AbilityDefinition> For(CardDefinition def)
    {
        var defaults = new List<AbilityDefinition>();
        switch (def.Type)
        {
            case CardType.Champion:
                defaults.AddRange([DrawAbility, BondAbility(2), MoveAbility, CollapseAbility, AttackAbility, DefendAbility, EquipAbility, UnequipAbility]);
                break;
            case CardType.Companion:
                defaults.AddRange([BondAbility(def.BondAp), MoveAbility, CollapseAbility, AttackAbility, DefendAbility, EquipAbility, UnequipAbility]);
                break;
            case CardType.Creature:
                defaults.AddRange([MoveAbility, AttackAbility, DefendAbility, EquipAbility, UnequipAbility]);
                break;
        }

        if (def.Type.IsCreatureType() && def.HasKeyword(Keyword.Subterranean))
            defaults.AddRange([AscendAbility, DescendAbility]);

        return defaults
            .Where(a => !def.RemovedDefaults.Contains(a.Id))
            .Concat(def.Abilities)
            .ToList();
    }
}
