# Worked example: Cinder Verdict

*A deliberately overloaded test card, walked through casting and resolution step by step. Not a real card (not part of `docs/cards/`). Use it to check any change to the casting or resolution procedure (`interaction-stack.md`, D91–D93, D130, D135): re-run the walkthrough and see what changes.*

**Tests:** cast condition, choose → bind → pay in every step, the card and fixed costs paid first, `X` as an amount, an alternative cost as options (one doing nothing), a mixed mana/fell option needing confirmation, kicker as an optional option, a paid object leaving the decision state (the felled creature can't be targeted), everything paid arriving at C5, `choose X` with repeats and per-pick target instances, an up-to-zero target, a card-scope target shared across options, a relation between two targets (D83), target-relative binding (chooser, Behavior), a known target cost with its own choice, a hidden target cost and a declined top-up, a collapse stated by the card (D131), a response in Pending, just-in-time checks, immediate falling, a resolution-time choice nested in a cast-time option, a Neutral creation, IDs in text order, trigger collection and ordering.

**Depends on open items:** the relation H1–T1 exists only under current D83 (item 32 would remove it); a chooser that could be Neutral (item 31).

---

## The card

> **Cinder Verdict** · Spell · Fire · Speed: Quick
> *Cast only if you control a Fire creature.*
> **Cost:** 🔥 + `X`
> **E1.** Deal `X`+1 damage to target creature **T1**. If it falls this way, create a Fire Warrior on target terrain **H1** within distance 1 of T1.
> **E2.** Choose `X`, repeats allowed:
>  • **a.** Deal 1 damage to up to one target creature **Ta**.
>  • **b.** T1 doesn't refresh AP at its controller's next Beginning.
> **E3.** As an additional cost, pay 🔥🔥 or fell a creature you control. If you fell one, T1's controller chooses one: they discard a card, or you draw a card.
> **E4.** Kicker {2}: create two Fire Warriors on target terrain **H2** adjacent to a creature you control, then create an Ember Imp there, Neutral, *Aggressive toward T1's controller*, acting on Neutral A.

Normal form (`../effect-form.md`):

```
castCondition: you control a Fire creature
cost:  🔥 (fixed, C1) + X (amount, C2)

T1 := you choose 1..1 creature
H1 := you choose 1..1 terrain, within 1 of T1                  -- names T1: a relation under D83 (item 32)
H2 := you choose 1..1 terrain, adjacent to a creature you control

E1: you choose 1..1 { felled := damage(T1, X+1);  [if felled ≠ ∅] create(Fire Warrior, H1) }
E2: you choose X..X repeatable
      a: { Ta := you choose 0..1 creature;  damage(Ta, 1) }
      b: { skipRefresh(T1) }
E3: you choose 1..1
      { (🔥🔥) }
      { (fell a creature you control)
        T1.controller choose 1..1 at resolution { discard(T1.controller, 1) } { draw(you, 1) } }
E4: you choose 0..1
      { ({2}) create(Fire Warrior, H2);
              create(Fire Warrior, H2);
              create(Ember Imp, H2, triple: Neutral, Aggressive toward T1.controller, Neutral A) }
```

E3's fell option holds a choice made at resolution by another chooser — the one level of nesting the form allows (D135). The cost of each option is visible only inside it: nothing outside E3 can ask whether a creature was felled.

E4 is three separate create instructions. Written as one instruction ("create two Fire Warriors"), the two Warcaller triggers below would come from the same instruction, source and ability, and be ordered only by event order inside the instruction (D92).

## Board

Champion A is active. A's Hand: Cinder Verdict and **Spark**.

| ID | Permanent | Controller | Life | Notes |
|---|---|---|---|---|
| 10 | Pyro Adept | A | 2 | A's only Fire creature |
| 14 | Warcaller | A | 3 | *Whenever a Fire Warrior enters under your control, it gets +1 Attack.* |
| 20 | Stone Brute | B | 3 | *Targeting this costs an additional discard.* (known) Adjacent to Watcher. |
| 22 | Watcher | B | 2 | *Whenever a creature within 1 of this is dealt damage, you gain 1 life.* Adjacent to Brute and Boar. |
| 30 | Wild Boar | Neutral (seat Neutral A) | 1 | *When this falls, deal 1 damage to each adjacent creature.* Adjacent to Watcher. |
| h₁ | Cinder Bog (terrain) | B | – | *Spells that target this cost {1} more.* *Mimic: appears to the opponent as a plain terrain; when a top-up is demanded for it, it collapses and the demanding Champion sees its true card.* A sees a plain terrain. |

## Casting

Each step is choose → bind → pay (`interaction-stack.md` §Casting). Text column: only the lines that changed in that step. ~~Struck~~ is removed, **bold** is new.

| Step | Choose | Pay | Tests | Text after this step (bind; changes only) |
|---|---|---|---|---|
| **C0 Legality** | — (a check) Quick fits Pending (empty). Cast condition: Pyro Adept #10 ✓. A legal combination exists. | — | cast condition | ~~*Cast only if you control a Fire creature.*~~ |
| **C1 Commit** | A plays Cinder Verdict. | The card leaves the Hand (in transit until C5); 🔥. | card and fixed cost paid first | "you" = Champion A:<br>E3 …or ~~you draw~~ **Champion A draws** a card<br>E4 …adjacent to a creature ~~you control~~ **Champion A controls** |
| **C2 Amounts** | X = 3. | 3. | `X` as an amount | E1: Deal ~~X+1~~ **4** damage…<br>E2: Choose ~~X~~ **3**… |
| **C3 Choose** | E1: its only option (no prompt). E2: 3 picks → **a, a, b**; each **a** gets its own Ta instance. E3: the **fell** option (mixed with a mana option, so explicitly confirmed); A fells **Pyro Adept #10**. E4: kicked. | E3: Adept #10 leaves the decision state (it falls; its Remnant arrives at C5). E4: {2}. | options as costs; a do-nothing option; confirmation; cast condition not rechecked (legal); per-pick instances | E2 becomes:<br>**a¹.** Deal 1 damage to up to one target creature **Ta¹**.<br>**a².** Deal 1 damage to up to one target creature **Ta²**.<br>**b.** T1 doesn't refresh AP at its controller's next Beginning.<br>E3: ~~As an additional cost, pay 🔥🔥 or fell a creature you control. If you fell one,~~ T1's controller chooses one…<br>E4: ~~Kicker {2}:~~ create two Fire Warriors… |
| **C4 Target** | Adept #10 is gone from the decision state, so it is no candidate. T1 = Stone Brute #20. H1 = h₁ (within 1 of Brute; plain in A's view). H2 = h₂ (next to Warcaller). Ta¹ = Wild Boar #30. **Ta² = none.** | Brute: *discard a card* — A chooses **Spark** now (the surcharge's own choice). h₁: +0 in A's view. | the felled creature can't be targeted; up-to-zero; known target cost with a choice; cost against the caster's view | E1: Deal 4 damage to **Stone Brute #20**. If it falls this way, create a Fire Warrior on **terrain h₁** within distance 1 of **Stone Brute #20**.<br>a¹: Deal 1 damage to **Wild Boar #30**.<br>a²: ~~Deal 1 damage to up to one target creature.~~ **(no target: does nothing)**<br>b: **Stone Brute #20** doesn't refresh AP…<br>E4: …on **terrain h₂** ~~adjacent to a creature Champion A controls~~<br>Target-relative: T1.controller = **Champion B**:<br>b: …at ~~its controller's~~ **Champion B's** next Beginning.<br>E3: ~~T1's controller~~ **Champion B** chooses one: ~~they discard~~ **Champion B discards** a card, or Champion A draws a card.<br>E4: …Aggressive toward ~~T1's controller~~ **Champion B**… |
| **C5 Enter Pending** | — | Everything paid arrives: Cinder Verdict and Spark in Discard, Adept's Remnant on its terrain. No trigger fired. The trace enters Pending. | atomic casting; arrivals at C5 | *(full trace text below)* |

**The trace as it enters Pending:**

> **Cinder Verdict** (trace) · Quick · paid: 🔥 + 3 + fell a creature you control + 2 + discard a card
> **E1.** Deal 4 damage to Stone Brute #20. If it falls this way, create a Fire Warrior on terrain h₁ within distance 1 of Stone Brute #20.
> **E2.** a¹. Deal 1 damage to Wild Boar #30. · a². *(nothing)* · b. Stone Brute #20 doesn't refresh AP at Champion B's next Beginning.
> **E3.** Champion B chooses one: Champion B discards a card, or Champion A draws a card.
> **E4.** Create two Fire Warriors on terrain h₂, then create an Ember Imp there, Neutral, Aggressive toward Champion B, acting on Neutral A.

**B responds** with a Quick card: "Move target creature you control to an adjacent terrain." B moves Brute #20 to a hex at distance 2 from h₁ that is still adjacent to Watcher. Nothing else responds; that trace resolves, then `Now` reaches Cinder Verdict.

## Resolution, instruction by instruction

| Step | Check | Result | Triggers collected | Trace line after this step |
|---|---|---|---|---|
| **R0 Top-up** | Brute: true cost = paid. **h₁: true cost +{1}** → a top-up is demanded; Cinder Bog collapses as its text states — A sees its true card. | A can see Brute now stands 2 from h₁, so E1's create would fizzle anyway: **A declines.** H1 becomes illegal for this trace. | none | ✗ **H1: not topped up** |
| **R1** E1 `felled := damage(Brute, 4)` | Brute: identity ✓ visible ✓. No location condition, so the move doesn't matter. | Brute takes 4 → **falls immediately** → ceases to exist; a new Remnant is created (D110). | **t1** Watcher (B) | ✓ **done: Stone Brute #20 fell** |
| **R1** E1 `[if felled] create(Warrior, h₁)` | Condition ✓. H1 is illegal (not topped up) → fizzles. (The relation "h₁ within 1 of Stone Brute #20" would fail too: Brute no longer exists.) | No Warrior. | none | ✗ **fizzled: H1 not topped up** |
| **R1** E2 a¹ `damage(Boar, 1)` | ✓ | Boar falls immediately. | **t2** Watcher (B) · **t3** Boar's fall trigger (Neutral A) | ✓ **done: Wild Boar #30 fell** |
| **R1** E2 a² | No target. | Does nothing. | none | – *(nothing)* |
| **R1** E2 b `skipRefresh(T1)` | Stone Brute #20 no longer exists → **identity check** fails. | Fizzles. | none | ✗ **fizzled: target gone** |
| **R1** E3 nested choice | Chooser = Champion B, bound at C4, even though Brute has fallen. | B chooses: B discards a card. | none | ✓ **Champion B discarded a card** |
| **R1** E4 create Warrior | ✓ | Fire Warrior #41, 0 AP, parent = payer (A). | **t4** Warcaller for #41 (A) | ✓ **created Fire Warrior #41** |
| **R1** E4 create Warrior | ✓ | Fire Warrior #42. | **t5** Warcaller for #42 (A) | ✓ **created Fire Warrior #42** |
| **R1** E4 create Imp | ✓ | Ember Imp #43, Neutral, parentless root, 0 AP. | none | ✓ **created Ember Imp #43** |
| **R2 Into Past** | | Trace → Past (Duration 5). | | |
| **R3 Triggers** | | The collected triggers enter Pending. | | |

## Trigger order

The earliest instruction's triggers end up on top. Within one instruction: APNAP over the seats (A, Neutral A, B, Neutral B — the active seat enters first, resolves last), then source ID, printed ability order, event order.

Enter Pending in this order: t5 (E4, 2nd create) → t4 (E4, 1st create) → t3 (Neutral A), t2 (B) from a¹ → t1 (E1).

**Resolves: t1 → t2 → t3 → t4 → t5.**

## Findings

1. A relation read against a fallen target fizzles on identity (D85, D110).
2. Triggers from one instruction, same source and ability → last tiebreak = event order inside the instruction (D92).
3. A chooser that could be Neutral (if T1 were Neutral) → `PLAN.md` item 31.
4. Felling the cast-condition creature as a cost is legal (D91).
5. An instruction left with no bound target does nothing (D91).
6. A payment choice must use up its object at once: otherwise the creature chosen to be felled could still be targeted, two costs could name the same object, and the card being cast could target itself in the Hand (D135).
7. A surcharge with its own choice can still pick an object that is a target of the same trace — accepted, `PLAN.md` S3 (D135).
8. "If you fell one, T1's controller chooses…" needs a resolution-time choice inside a cast-time option — the one level of nesting (D135).
9. A top-up is optional: the caster declines when the instruction will fizzle anyway, and the target becomes illegal for the trace (D130).
