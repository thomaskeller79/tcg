# Effect Normal Form

*The internal structure every card's and ability's rules text is written in. Card text shown to players may use shorter wording ("you may," "kicker," "choose one or more"); every such wording must translate into this form. Kept deliberately compact, so that a card validator or an AI can read and check it.*

**Decisions:** D83, D91, D92, D94, D95, D96, D104, D108, D130, D135 (`history/decisions.md`)

---

## Grammar

```
effects     ::= selection* effect+                                        -- card-level selections: one shared instance
effect      ::= chooser choose count [repeatable] [at resolution] option+
option      ::= [(cost)] selection* (instruction | nested)*               -- option-level names: fresh instance per pick
nested      ::= [condition] chooser choose count [repeatable] at resolution option+   -- only inside a cast-time option; its options hold instructions only
selection   ::= name := chooser choose count [at resolution] constraint* [(cost)]
count       ::= min..max                                                  -- integers or expressions over variables
instruction ::= [output :=] [condition] verb arg*
condition   ::= predicate over variables, selections, outputs, game state
arg         ::= name | reference | card name | creation triple
```

An effect is a list of choices, each a list of options, each a list of instructions. Card text may be worded differently and compiles into this form, possibly with a blow-up (an effect shared by several options is restated in each).

**One level of nesting, bounded by time (D135).** An option of a choice made at casting may contain a choice made at resolution where an instruction would stand, with its own chooser and an optional condition: "(fell a creature you control) T1's controller chooses: they discard, or you draw." An option of a choice made at resolution holds only instructions. Choices at casting never nest — they are all yours and made at once, so they flatten into one menu.

**Costs sit where they are decided (D135)**, one per casting step (`interaction-stack.md` §Casting): the card's **fixed cost** (C1) and its **amounts** (`X`, C2) are card properties outside this form, as is the cast condition; a cost on an **option** is paid when the option is chosen (C3, or at resolution for a choice made then); a cost on a **selection** is a cost that depends on that target (C4). A cost on an option is visible only inside that option. An option's chooser pays its cost; a choice made at casting is always the caster's. An option may have no instructions — an alternative cost whose option does nothing else.

## Prototype scope

Until the game runs end to end, only cards of this restricted shape are legal (D104). Every such card is a valid card in the full form, so extending the scope only adds.

- **Rules text** is targets (objects selected at cast, any number) plus a **sequence of instructions** (`verb arg*`), run in card order — one effect with one option, chosen by you, at cast.
- **Not in scope:** modes, conditions, outputs, repeatable picks, choices at resolution, choosers other than you.
- **A permanent card** has no target but its implicit location.
- **Abilities** of a permanent (static, triggered, activated) follow the same shape, and function only on the Island (an activated one through its Trace). Abilities that function from Hand, Library or Discard are not in scope, with one exception: **card statics restricting the card's own location target** (D110) — the Slice filter (D32, e.g. Flying: enters in Sky) and restrictions such as "can only target a terrain holding a Remnant you control."
- **Cost** is a fixed list: no `X`, no costs on options, no cast condition. Costs that depend on a target (D130, e.g. a terrain's "Move abilities that target this cost 1 more AP") are in scope.

The procedure stays the full one (C0–C5, D135, `interaction-stack.md`), with C2 Amounts and C3 Choose empty: C1 pays the fixed cost, C4 the costs that depend on targets. The rest of this doc describes the full form; parts outside the scope stay decided, and their open questions are `PLAN.md` Track D.

## One primitive: choose

A **chooser** chooses **count** items, either among **options** (an effect) or among **objects** (a selection), either **at cast** or **at resolution**:

| | At cast | At resolution |
|---|---|---|
| among options | a mode (C3 Choose) | "you may," "choose one" made at resolution |
| among objects | a **target** (C4 Target) | "a creature of your choice," "an opponent chooses a creature they control" |

A target is exactly an object selection made at cast; "can't be targeted" therefore only matters at cast (D85). The **chooser** is mandatory and defaults to "you"; a choice at cast is always yours, another chooser only chooses at resolution (D135); it binds like any reference (`interaction-stack.md` §Binding). **Kind** ("creature," "hex") is an ordinary constraint. **Count** is a range: "up to one target creature" is `0..1`, "you may X" is `you choose 0..1 at resolution { X }`, a fixed clause is `you choose 1..1 { … }` with a single option.

## Names and scope

Every name is one of three kinds, differing only in when it is bound:

| Kind | Bound at | Example |
|---|---|---|
| variable | C2 Amounts | `X` |
| selection | C4 Target (at cast) or its instruction (at resolution) | `T := you choose 1..1 creature` |
| output | Resolution | `felled := damage(T, 3)` |

Picks of a repeatable effect run in **menu order**, repeats back to back.

**A name is visible in the block where it is declared — the card, or an option — and only to what follows its declaration.** References flow inward, never outward. Variables and card-level selections are card-wide, with one instance each. An option's cost, its selections and all outputs live in their option; a repeatable option picked twice has two independent instances of each, neither visible to the other or outside the option. An effect that needs the same fact in several options repeats it in each.

## Outputs

An instruction can bind an output for later instructions in the same option. **The output is the instruction's state diff** — everything it changed, including the permanents it felled (they fall immediately after it, `interaction-stack.md` §Resolution) — and conditions and arguments query it ("how many creatures fell"). An output exists only during its trace's resolution; it never reaches the trace's stored state, so Remand never touches it. It is in one of three states:

- **unbound** — its instruction didn't run (its condition was false, or it read an unbound output);
- **empty** — its instruction ran or fizzled and affected nothing ("up to 2 target creatures gain trample" with 0 targets);
- **a value.**

**An instruction that reads an unbound output doesn't run, and its own output is unbound too.** So "didn't happen" propagates and is never mistaken for "happened, with no effect": after `felled := [if X ≥ 2] damage(T, 3)`, "if it did not fall this way" (`[if felled = ∅]`) is true only when the damage actually happened (or fizzled) without felling anything.

## Conditions

A condition sits on the instruction only. A condition on a fact already settled at cast (a variable) collapses into the trace text when that step completes; any other condition is evaluated just before its instruction runs (`interaction-stack.md` §Resolution).

## Card text to normal form

| Card text | Normal form |
|---|---|
| "Kicker {2}. … If kicked, also X." | `you choose 0..1 { ({2}) X }` |
| "+{1}: A. +{2}: B. Choose one or more." | `you choose 1..2 { ({1}) A } { ({2}) B }` |
| "As an additional cost, pay 🔥🔥 or fell a creature you control. If a creature was felled this way, Y." | `you choose 1..1 { (🔥🔥) } { (fell a creature you control) Y }` |
| "Escalate {1}. Choose one or more." | amount `X`, cost `X·1`; `you choose X+1..X+1 { … }` |
| "Deal 2 damage to target creature. If the creature is a Fire creature, discard a card as an additional cost." | `T := you choose 1..1 creature ([if T is Fire] discard a card)`; `damage(T, 2)` |
| "You may draw a card." | `you choose 0..1 at resolution { draw(you, 1) }` |
| "Target opponent chooses a creature they control. Destroy it." | `P := you choose 1..1 opponent`; `C := P choose 1..1 at resolution creature, controlled by P`; `destroy(C)` |
| "Deal 3 damage to target creature. If it falls this way, its controller chooses one: they discard a card, or you draw a card." | `T := you choose 1..1 creature`; `you choose 1..1 { felled := damage(T, 3); [if felled ≠ ∅] T.controller choose 1..1 at resolution { discard(T.controller, 1) } { draw(you, 1) } }` |
| "Deal 3 damage to target creature. If it falls this way, draw a card." | `T := you choose 1..1 creature`; `felled := damage(T, 3)`; `[if felled ≠ ∅] draw(you, 1)` |

Worked example across the whole casting and resolution procedure: `examples/cinder-verdict.md`.

## Open questions

1. **Relations between two selections** (D83: "target terrain within distance 1 of T1" stays in the trace and is rechecked). `PLAN.md` Track D item 32 would drop them in favour of instruction conditions; until then a selection's constraint naming another selection is a relation.
2. **Thresholds across repeated picks** aren't expressible ("if 3 or more creatures died this way, draw a card" after "choose three, repeats allowed"). `PLAN.md` Track D item 41.
