# Effect Normal Form

*The internal structure every card's and ability's rules text is written in. Card text shown to players may use shorter wording ("you may," "kicker," "choose one or more"); every such wording must translate into this form. Kept deliberately compact, so that a card validator or an AI can read and check it.*

**Decisions:** D83, D91, D92, D94, D95 (`history/decisions.md`)

---

## Grammar

```
effects     ::= selection* effect+                                        -- card-level selections: one shared instance
effect      ::= chooser choose count [repeatable] [at resolution] option+
option      ::= selection* instruction+                                   -- option-level names: fresh instance per pick
selection   ::= name := chooser choose count [at resolution] constraint*
count       ::= min..max                                                  -- integers or expressions over variables
instruction ::= [output :=] [condition] verb arg*
condition   ::= predicate over variables, branch labels (ci = bk), selections, outputs, game state
arg         ::= name | reference | card name | creation triple
```

The cost (a CNF, `interaction-stack.md` §Casting) and the cast condition are card properties outside this form; the cost's variables (`X`) and branch labels are names the form can read.

## One primitive: choose

A **chooser** chooses **count** items, either among **options** (an effect) or among **objects** (a selection), either **at cast** or **at resolution**:

| | At cast | At resolution |
|---|---|---|
| among options | a mode (Choose step) | "you may," "choose one" made at resolution |
| among objects | a **target** (Target step) | "a creature of your choice," "an opponent chooses a creature they control" |

A target is exactly an object selection made at cast; "can't be targeted" therefore only matters at cast (D85). The **chooser** is mandatory and defaults to "you"; it binds like any reference (`interaction-stack.md` §Binding). **Kind** ("creature," "hex") is an ordinary constraint. **Count** is a range: "up to one target creature" is `0..1`, "you may X" is `you choose 0..1 at resolution { X }`, a fixed clause is `you choose 1..1 { … }` with a single option.

## Names and scope

Every name is one of three kinds, differing only in when it is bound:

| Kind | Bound at | Example |
|---|---|---|
| variable | Pay | `X`, `c2 = b1` |
| selection | Target (at cast) or its instruction (at resolution) | `T := you choose 1..1 creature` |
| output | Resolution | `killed := damage(T, 3)` |

**A name is visible in the block where it is declared — the card, or an option — and only to what follows its declaration.** Variables and card-level selections are card-wide, with one instance each. Option-level selections and all outputs live in their option; a repeatable option picked twice has two independent instances of each, neither visible to the other or outside the option. An effect that needs the same fact in several options repeats it in each.

## Outputs

An instruction can bind an output — a named result, such as the objects it killed — for later instructions in the same option. An output exists only during its trace's resolution; it never reaches the trace's stored state, so Remand never touches it. It is in one of three states:

- **unbound** — its instruction didn't run (its condition was false, or it read an unbound output);
- **empty** — its instruction ran or fizzled and affected nothing ("up to 2 target creatures gain trample" with 0 targets);
- **a value.**

**An instruction that reads an unbound output doesn't run, and its own output is unbound too.** So "didn't happen" propagates and is never mistaken for "happened, with no effect": after `killed := [if c4 = b1] damage(T, 3)`, "if it did not die this way" (`[if killed = ∅]`) is true only when the damage actually happened (or fizzled) without killing.

## Conditions

A condition sits on the instruction only. A condition on a fact already settled at cast (a variable, a paid branch) collapses into the trace text when that step completes; any other condition is evaluated just before its instruction runs (`interaction-stack.md` §Resolution).

## Card text to normal form

| Card text | Normal form |
|---|---|
| "Kicker {2}. … If kicked, also X." | cost conjunct `c4 = (b1: 2 ∨ b2: ε)`; `[if c4 = b1] X` |
| "+{1}: A. +{2}: B. Choose one or more." | cost `c2 = (b1: 1 ∨ b2: 2 ∨ b3: 3)`; `[if c2 = b1 ∨ c2 = b3] A`, `[if c2 = b2 ∨ c2 = b3] B` |
| "Escalate {1}. Choose one or more." | cost conjunct `X·1`; `you choose X+1..X+1 { … }` |
| "You may draw a card." | `you choose 0..1 at resolution { draw(you, 1) }` |
| "Target opponent chooses a creature they control. Destroy it." | `P := you choose 1..1 opponent`; `C := P choose 1..1 at resolution creature, controlled by P`; `destroy(C)` |
| "Deal 3 damage to target creature. If it dies this way, draw a card." | `T := you choose 1..1 creature`; `killed := damage(T, 3)`; `[if killed ≠ ∅] draw(you, 1)` |

Worked example across the whole casting and resolution procedure: `examples/cinder-verdict.md`.

## Open questions

1. **Relations between two selections** (D83: "target hex within distance 1 of T1" stays in the trace and is rechecked). `PLAN.md` Track A item 32 would drop them in favour of instruction conditions; until then a selection's constraint naming another selection is a relation.
2. **Order of repeated picks.** When a repeatable effect picks the same option several times, do the instances run in menu order (repeats back to back, as MTG does) or in the order picked? Proposed: menu order.
3. **Aggregates across picks** ("draw a card for each creature killed by this spell") aren't expressible — outputs don't leave their option instance. Accepted until a real card needs it.
4. **A condition can't gate a choice.** Conditions sit on instructions, so "if a creature was sacrificed, an opponent chooses one: …" still asks for the choice when the condition is false, with every option then doing nothing. Harmless when the condition is settled at cast (it collapses first), but a resolution-time condition would force a pointless choice.
