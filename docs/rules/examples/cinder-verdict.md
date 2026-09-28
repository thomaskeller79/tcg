# Worked example: Cinder Verdict

*A deliberately overloaded test card, walked through casting and resolution step by step. Not a real card (not part of `docs/cards/`). Use it to check any change to the casting or resolution procedure (`interaction-stack.md`, D91–D93): re-run the walkthrough and see what changes.*

**Tests:** cast condition, early binding, CNF cost with `X`, an ε branch and a mixed disjunct, settled-condition collapse, `choose X` with repeats and per-pick target instances, an up-to-zero target, a card-scope target shared across modal effects, a relation between two targets (D83), target-relative binding (chooser, Behavior), a response in Pending, just-in-time checks, immediate death, a resolution-time choice, a Neutral creation, IDs in text order, trigger collection and ordering.

**Depends on open items:** the effect notation below is provisional (`PLAN.md` Track A item 4); the relation H1–T1 exists only under current D83 (item 32 would remove it); a relation and verb read against a Grave (item 27); a chooser that could be Neutral (item 31).

---

## The card

> **Cinder Verdict** · Spell · Fire · Speed: Quick
> *Cast only if you control a Fire creature.*
> **Cost:** 🔥 + (🔥🔥 **or** sacrifice a creature) + `X` + optional kicker {2}
> **E1.** Deal `X`+1 damage to target creature **T1**. If it dies this way, create a Fire Warrior on target hex **H1** within distance 1 of T1.
> **E2.** Choose `X`, repeats allowed:
>  • **a.** Deal 1 damage to up to one target creature **Ta**.
>  • **b.** T1 doesn't refresh AP at its controller's next Beginning.
> **E3.** If a creature was sacrificed to pay for this, T1's controller chooses one: they discard a card, **or** you draw a card.
> **E4.** If kicked, create two Fire Warriors on target hex **H2** adjacent to a creature you control, then create an Ember Imp there, Neutral, *Aggressive toward T1's controller*, acting on Neutral A.

Normal form (provisional notation):

```
castCondition: you control a Fire creature
cost:  c1 = 🔥 ∧ c2 = (b1: 🔥🔥 ∨ b2: sacrifice creature) ∧ c3 = X ∧ c4 = (b1: 2 ∨ b2: ε)
card-scope targets: T1 {permanent, creature}, H1 {location, hex}, H2 {location, hex, own: adjacent to a creature you control}
relations: H1 within 1 of T1
E1: choose 1 { out1 := damage(T1, X+1);  [if out1.died] create(Fire Warrior, H1) }
E2: choose X, repeatable {
      a: Ta {permanent, creature, count 0..1};  damage(Ta, 1)
      b: skipRefresh(T1) }
E3: choose 1 { [if c2 = b2] choose 1 at resolution by T1.controller { discard(T1.controller, 1), draw(you, 1) } }
E4: choose 1 { [if c4 = b1] create(Fire Warrior, H2); create(Fire Warrior, H2);
               create(Ember Imp, H2, triple: Neutral, Aggressive toward T1.controller, Neutral A) }
```

E4 is three separate create instructions. Written as one instruction ("create two Fire Warriors"), the two Warcaller triggers below would come from the same instruction, source and ability, and be ordered only by event order inside the instruction (D92).

## Board

Champion A is active.

| ID | Permanent | Controller | Life | Notes |
|---|---|---|---|---|
| 10 | Pyro Adept | A | 2 | A's only Fire creature |
| 14 | Warcaller | A | 3 | *Whenever a Fire Warrior enters under your control, it gets +1 Attack.* |
| 20 | Stone Brute | B | 3 | Adjacent to Watcher. |
| 22 | Watcher | B | 2 | *Whenever a creature within 1 of this is dealt damage, you gain 1 life.* Adjacent to Brute and Boar. |
| 30 | Wild Boar | Neutral (seat Neutral A) | 1 | *When this dies, deal 1 damage to each adjacent creature.* Adjacent to Watcher. |

## Casting

Text column: only the lines that changed in that step. ~~Struck~~ is removed, **bold** is new.

| Step | What happens | Tests | Text after this step (changes only) |
|---|---|---|---|
| **0. Legality** | Quick fits Pending (empty). Cast condition: Pyro Adept #10 ✓. Source-relative binding: "you" = Champion A. | cast condition, early binding | ~~*Cast only if you control a Fire creature.*~~<br>E3… or ~~you draw~~ **Champion A draws** a card<br>E4… adjacent to a creature ~~you control~~ **Champion A controls** |
| **1. Pay** | The card goes to Discard. 🔥; c2 = **b2**: A sacrifices **Pyro Adept #10**; X = 3; c4 = b1, pays {2}. | cast condition not rechecked (legal); ε branch; mixed disjunct needs explicit confirmation | **Cost:** 🔥 + sacrifice a creature + 3 + 2<br>E1: Deal ~~X+1~~ **4** damage…<br>E2: Choose ~~X~~ **3**…<br>E3: ~~If a creature was sacrificed to pay for this,~~ T1's controller chooses one…<br>E4: ~~If kicked,~~ create two Fire Warriors… |
| **2. Choose** | E2: 3 picks → **a, a, b**. Each **a** gets its own Ta instance. | count = variable, per-pick instances | E2 becomes:<br>**a¹.** Deal 1 damage to up to one target creature **Ta¹**.<br>**a².** Deal 1 damage to up to one target creature **Ta²**.<br>**b.** T1 doesn't refresh AP at its controller's next Beginning. |
| **3. Target** | T1 = Stone Brute #20. H1 = h₁ (within 1 of Brute). H2 = h₂ (next to Warcaller). Ta¹ = Wild Boar #30. **Ta² = none.** | up-to-zero; own qualifier consumed, relation kept | E1: Deal 4 damage to **Stone Brute #20**. If it dies this way, create a Fire Warrior on **hex h₁** within distance 1 of **Stone Brute #20**.<br>a¹: Deal 1 damage to **Wild Boar #30**.<br>a²: ~~Deal 1 damage to up to one target creature.~~ **(no target: does nothing)**<br>b: **Stone Brute #20** doesn't refresh AP…<br>E4: …on **hex h₂** ~~adjacent to a creature Champion A controls~~ |
| *binding* | Target-relative: T1.controller = **Champion B**. | target-relative binding | E2 b: …at ~~its controller's~~ **Champion B's** next Beginning.<br>E3: ~~T1's controller~~ **Champion B** chooses one: ~~they discard~~ **Champion B discards** a card, or Champion A draws a card.<br>E4: …Aggressive toward ~~T1's controller~~ **Champion B**… |
| **4. Pending** | The trace enters Pending. | | *(full trace text below)* |

**The trace as it enters Pending:**

> **Cinder Verdict** (trace) · Quick · paid: 🔥 + sacrifice a creature + 3 + 2
> **E1.** Deal 4 damage to Stone Brute #20. If it dies this way, create a Fire Warrior on hex h₁ within distance 1 of Stone Brute #20.
> **E2.** a¹. Deal 1 damage to Wild Boar #30. · a². *(nothing)* · b. Stone Brute #20 doesn't refresh AP at Champion B's next Beginning.
> **E3.** Champion B chooses one: Champion B discards a card, or Champion A draws a card.
> **E4.** Create two Fire Warriors on hex h₂, then create an Ember Imp there, Neutral, Aggressive toward Champion B, acting on Neutral A.

**B responds** with a Quick card: "Move target creature you control to an adjacent hex." B moves Brute #20 to a hex at distance 2 from h₁ that is still adjacent to Watcher. Nothing else responds; that trace resolves, then `Now` reaches Cinder Verdict.

## Resolution, instruction by instruction

| Instr. | Check | Result | Triggers collected | Trace line after this instruction |
|---|---|---|---|---|
| E1 `out1 := damage(Brute, 4)` | Brute: identity ✓ visible ✓. No location condition, so the move doesn't matter. | Brute takes 4 → **dies immediately** → becomes a Grave. | **t1** Watcher (B) | ✓ **done: Stone Brute #20 died** |
| E1 `[if died] create(Warrior, h₁)` | Condition ✓. **Relation "h₁ within 1 of Stone Brute #20":** now a Grave at distance 2 → **false** → fizzles. | No Warrior. | none | ✗ **fizzled: relation false** |
| E2 a¹ `damage(Boar, 1)` | ✓ | Boar dies immediately. | **t2** Watcher (B) · **t3** Boar's death trigger (Neutral A) | ✓ **done: Wild Boar #30 died** |
| E2 a² | No target. | Does nothing. | none | – *(nothing)* |
| E2 b `skipRefresh(T1)` | Stone Brute #20 is a Grave → **verb applicability** fails. | Fizzles. | none | ✗ **fizzled: verb not applicable** |
| E3 resolution choice | Chooser = Champion B, bound at declaration, even though Brute is dead. | B chooses: B discards a card. | none | ✓ **Champion B discarded a card** |
| E4 create Warrior | ✓ | Fire Warrior #41, 0 AP, parent = payer (A). | **t4** Warcaller for #41 (A) | ✓ **created Fire Warrior #41** |
| E4 create Warrior | ✓ | Fire Warrior #42. | **t5** Warcaller for #42 (A) | ✓ **created Fire Warrior #42** |
| E4 create Imp | ✓ | Ember Imp #43, Neutral, parentless root, 0 AP. | none | ✓ **created Ember Imp #43** |
| end | | Trace → Past (Duration 5), then collected triggers enter Pending. | | |

## Trigger order

The earliest instruction's triggers end up on top. Within one instruction: APNAP over the seats (A, Neutral A, B, Neutral B — the active seat enters first, resolves last), then source ID, printed ability order, event order.

Enter Pending in this order: t5 (E4, 2nd create) → t4 (E4, 1st create) → t3 (Neutral A), t2 (B) from a¹ → t1 (E1).

**Resolves: t1 → t2 → t3 → t4 → t5.**

## Findings when this was first run (item 4 discussion)

1. A relation read against a Grave: evaluates only because a Grave is the same object (D88) → `PLAN.md` item 27.
2. Triggers from one instruction, same source and ability → last tiebreak = event order inside the instruction (D92).
3. A chooser that could be Neutral (if T1 were Neutral) → `PLAN.md` item 31.
4. Sacrificing the cast-condition creature as a cost is legal (D91).
5. An instruction left with no bound target does nothing (D91).
