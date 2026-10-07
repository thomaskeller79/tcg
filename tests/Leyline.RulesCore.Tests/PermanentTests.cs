using Leyline.RulesCore.Model;
using Leyline.RulesCore.Perception;
using Leyline.RulesCore.Rules;
using Leyline.RulesCore.State;
using Leyline.RulesCore.Tests.Support;
using static Leyline.RulesCore.Tests.Support.Game;

namespace Leyline.RulesCore.Tests;

/// <summary>object-properties.md §5: casting each permanent type, Remnants (D110), Structures
/// (D111), raise, Bounce/Flicker (D69), Haste (D114), Items (D26, D30, D105), Companion loss (D82).</summary>
public class PermanentTests
{
    [Fact]
    public void A_cast_creature_enters_on_terrain_you_control_with_0_AP_and_the_Champion_as_parent()
    {
        var g = Load("handcard A creature.fire-warrior\nmana A Fire 1");
        Assert.False(g.Cast(A, "creature.fire-warrior", At(1, 1)).Accepted); // not controlled
        g.Ok(g.Cast(A, "creature.fire-warrior", At(0, 2)));
        g.ResolveAll();
        var warrior = g.P("creature.fire-warrior");
        Assert.Equal(0, warrior.CurrentAp);
        Assert.Equal(g.Champion(A).Id, warrior.Parent);
        Assert.Equal(A, g.State.Controller(warrior));
    }

    [Fact]
    public void Haste_enters_with_its_Activation_Points_and_Flying_enters_in_the_Sky()
    {
        var g = Load("handcard A creature.ember-imp creature.sky-hawk\nmana A Fire 1\nmana A Air 1");
        g.Ok(g.Cast(A, "creature.ember-imp", At(0, 2)));
        g.ResolveAll();
        Assert.Equal(3, g.P("creature.ember-imp").CurrentAp);
        g.Ok(g.Cast(A, "creature.sky-hawk", At(0, 2)));
        g.ResolveAll();
        Assert.Equal(Slice.Sky, g.P("creature.sky-hawk").Slice);
    }

    [Fact]
    public void A_fallen_creature_leaves_a_Remnant_on_its_terrain_a_fallen_Structure_leaves_nothing()
    {
        var g = Load("""
            place B creature.sky-hawk 0,0
            bond B 0,-1
            place B structure.watchtower 0,-1
            handcard A spell.firebolt*2
            mana A Fire 4
            """);
        g.Ok(g.Cast(A, "spell.firebolt", Obj(g.P("creature.sky-hawk"))));
        g.ResolveAll();
        var remnant = Assert.Single(g.Remnants);
        Assert.Equal(H(0, 0), remnant.Hex);
        Assert.Equal(Slice.Ground, remnant.Slice); // Sky → Ground (D105)
        Assert.Null(g.State.Controller(remnant)); // (0,0) isn't bonded

        g.State.Get<Permanent>(g.P("structure.watchtower").Id).CurrentLife = 3;
        g.Ok(g.Cast(A, "spell.firebolt", Obj(g.P("structure.watchtower"))));
        g.ResolveAll();
        Assert.Null(g.Find("structure.watchtower"));
        Assert.Single(g.Remnants);
    }

    [Fact]
    public void A_Remnant_belongs_to_whoever_bonded_its_terrain_and_can_be_raised()
    {
        var g = Load("""
            bond A 0,1
            place B test.grunt 0,1
            handcard A spell.smite spell.raise
            mana A Light 3
            mana A Darkness 3
            """);
        g.Ok(g.Cast(A, "spell.smite", Obj(g.P("test.grunt"))));
        g.ResolveAll();
        var remnant = Assert.Single(g.Remnants);
        Assert.Equal(A, g.State.Controller(remnant)); // your terrain, your Remnant
        g.Ok(g.Cast(A, "spell.raise", Obj(remnant)));
        g.ResolveAll();
        Assert.Empty(g.Remnants);
        var raised = g.P("test.grunt");
        Assert.Equal(A, g.State.Controller(raised));
        Assert.Equal(0, raised.CurrentAp);
    }

    [Fact]
    public void Bounce_creates_a_new_card_that_keeps_permanent_max_tier_changes()
    {
        var g = Load("""
            place A test.grunt 0,1
            handcard A spell.recall
            mana A Water 2
            """);
        var grunt = g.P("test.grunt");
        grunt.MaxLifeDelta = 2;
        grunt.CurrentLife = 1;
        g.Ok(g.Cast(A, "spell.recall", Obj(grunt)));
        g.ResolveAll();
        Assert.Null(g.Find("test.grunt"));
        var card = g.HandCard(A, "test.grunt");
        Assert.Equal(2, card.MaxLifeDelta);
        Assert.Empty(g.Remnants); // ceasing to exist without falling
    }

    [Fact]
    public void Flicker_returns_a_new_permanent_on_the_same_terrain_with_0_AP()
    {
        var g = Load("""
            bond A 0,1
            place A test.grunt 0,1
            handcard A spell.blink
            mana A Light 2
            """);
        var old = g.P("test.grunt");
        g.Ok(g.Cast(A, "spell.blink", Obj(old)));
        g.ResolveAll();
        var fresh = g.P("test.grunt");
        Assert.NotEqual(old.Id, fresh.Id);
        Assert.Equal(H(0, 1), fresh.Hex);
        Assert.Equal(0, fresh.CurrentAp);
        Assert.Equal(A, g.State.Controller(fresh));
    }

    [Fact]
    public void Items_are_equipped_from_the_same_location_give_their_bonus_and_drop_when_the_carrier_falls()
    {
        var g = Load("""
            place A test.grunt 0,1
            place A item.flame-blade 0,1
            handcard B spell.firebolt*2
            """);
        var grunt = g.P("test.grunt");
        var blade = g.P("item.flame-blade");
        g.Ok(g.Act(A, grunt, DefaultAbilities.Equip, Obj(blade)));
        g.ResolveAll();
        Assert.Equal(grunt.Id, blade.Carrier);
        Assert.Equal(5, g.State.Attack(grunt));
        Assert.Equal(H(0, 1), g.State.PositionOf(blade));

        g.Ok(g.Act(A, grunt, DefaultAbilities.Move, At(1, 0)));
        g.ResolveAll();
        Assert.Equal(H(1, 0), g.State.PositionOf(blade));

        g.AdvanceTo(Seat.ChampionB);
        g.Champion(B).Pool!.Add(Element.Fire, 4);
        g.Ok(g.Cast(B, "spell.firebolt", Obj(grunt)));
        g.ResolveAll();
        g.Ok(g.Cast(B, "spell.firebolt", Obj(grunt)));
        g.ResolveAll();
        Assert.Null(g.Find("test.grunt"));
        Assert.Null(blade.Carrier);
        Assert.Equal(H(1, 0), blade.Hex);
        Assert.Null(g.State.Controller(blade)); // a loose Item has no parent (D113)
    }

    [Fact]
    public void An_equipped_Items_ability_is_paid_from_its_carriers_AP()
    {
        var g = Load("""
            place A test.grunt 0,1
            place A item.spark-amulet 0,1
            place B test.brute 0,0
            """);
        var grunt = g.P("test.grunt");
        var amulet = g.P("item.spark-amulet");
        Assert.False(g.Act(A, amulet, "amulet-jolt", Obj(g.P("test.brute"))).Accepted); // loose: no ability
        g.Ok(g.Act(A, grunt, DefaultAbilities.Equip, Obj(amulet)));
        g.ResolveAll();
        g.Ok(g.Act(A, amulet, "amulet-jolt", Obj(g.P("test.brute"))));
        g.ResolveAll();
        Assert.Equal(6, g.P("test.brute").CurrentLife);
        Assert.Equal(0, grunt.CurrentAp); // 4 - 2 (equip) - 2 (jolt)
    }

    [Fact]
    public void Un_equip_drops_the_Item_on_the_carriers_terrain()
    {
        var g = Load("""
            place A creature.sky-hawk 0,1
            place A item.iron-shield 0,1
            """);
        var hawk = g.P("creature.sky-hawk");
        var shield = g.P("item.iron-shield");
        g.Ok(g.Act(A, hawk, DefaultAbilities.Equip, Obj(shield))); // Ground and Sky count as one Slice
        g.ResolveAll();
        Assert.Equal(5, g.State.MaxLife(hawk));
        g.Ok(g.Act(A, hawk, DefaultAbilities.Unequip, Obj(shield)));
        g.ResolveAll();
        Assert.Null(shield.Carrier);
        Assert.Equal(Slice.Ground, shield.Slice); // never in Sky (D105)
    }

    [Fact]
    public void A_Companion_ceasing_to_exist_unbonds_its_terrain_and_its_creatures_turn_Neutral()
    {
        var g = Load("""
            place A companion.ash 0,1
            handcard B spell.smite
            """);
        var ash = g.P("companion.ash");
        g.Ok(g.Act(A, ash, DefaultAbilities.Bond, At(0, 1)));
        g.ResolveAll();
        ash.CurrentAp = 5;
        ash.Pool!.Add(Element.Fire, 1);
        g.Ok(g.Act(A, ash, "ash-kindle"));
        g.ResolveAll();
        var warrior = g.P("creature.fire-warrior");
        Assert.Equal(ash.Id, warrior.Parent);
        Assert.Equal(A, g.State.Controller(warrior));

        g.AdvanceTo(Seat.ChampionB);
        g.Champion(B).Pool!.Add(Element.Light, 3);
        g.Ok(g.Cast(B, "spell.smite", Obj(ash)));
        g.ResolveAll();
        Assert.Null(g.Terrain(0, 1).Parent);
        Assert.Null(g.State.Controller(warrior));
        Assert.Equal(new BehaviorAssignment(BehaviorAssignment.Aggressive, B, Seat.NeutralA), warrior.Behavior);
    }

    [Fact]
    public void A_Structure_is_cast_into_its_slot_and_acts_with_its_own_AP()
    {
        var g = Load("""
            handcard A structure.watchtower
            mana A Metal 2
            place B test.grunt 0,0
            """);
        g.Ok(g.Cast(A, "structure.watchtower", At(0, 2)));
        g.ResolveAll();
        var tower = g.P("structure.watchtower");
        Assert.Equal(0, tower.CurrentAp);
        Assert.False(g.Act(A, tower, "tower-volley", Obj(g.P("test.grunt"))).Accepted);
        g.NextTurnOf(Seat.ChampionA);
        Assert.Equal(2, tower.CurrentAp);
        g.Ok(g.Act(A, tower, "tower-volley", Obj(g.P("test.grunt"))));
        g.ResolveAll();
        Assert.Equal(4, g.P("test.grunt").CurrentLife);
        Assert.Empty(Combat.AttackCandidates(g.State, tower, A)); // a Structure never fights (D107)
    }

    [Fact]
    public void A_Root_Structure_is_hidden_from_the_opponent()
    {
        var g = Load("handcard A structure.root-cellar\nmana A Darkness 1");
        g.Ok(g.Cast(A, "structure.root-cellar", At(0, 2)));
        g.ResolveAll();
        var cellar = g.P("structure.root-cellar");
        Assert.Equal(Slice.Root, cellar.Slice);
        Assert.DoesNotContain(ViewProjector.Project(g.State, B).Permanents, p => p.Id == cellar.Id.Value);
        Assert.Contains(ViewProjector.Project(g.State, A).Permanents, p => p.Id == cellar.Id.Value);
    }
}
