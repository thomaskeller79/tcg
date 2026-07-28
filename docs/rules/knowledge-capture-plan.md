# Knowledge Capture Plan

*How we get the game out of your head and onto disk — and how we keep the "cards can change the rules" property intact from day one.*

**Status:** Active · **Date:** 2026-07-23

---

## Part A — The "cards can change the rules" property (architectural north star)

This is a **planning-time constraint**, not just an implementation detail. Every rule we write down in Part B gets stress-tested against it. The goal: **a card that changes a rule must use the same mechanism the base game uses — never a special case bolted on later.**

### The core discipline
> **The engine never reads a raw value or hardcodes a rule. It always *asks*.**

Instead of `if unit.movement >= distance`, the engine asks a resolver: *"What is this unit's movement right now, given every active effect on the board?"* The base rule ("movement = the unit's printed stat") is just the lowest-priority answer. A card that says "your units move +1" is another voice in the same query. Neither is privileged.

### Four mechanisms (borrowed from mature card-game engines like MTG's rules)

1. **Everything is an event.** No state mutates directly. An action becomes an *intent* → passes through a pipeline → is applied as an *event*. The pipeline is where cards get to interfere. No pipeline = no place to hook = hardcoded rule.

2. **Queries fold in modifiers (continuous effects).** Any value the engine needs — cost, movement, range, hand size, whether a phase even happens, what counts as a legal target — is computed through a query layer that gathers all active modifiers. **Never read `card.cost` directly; always ask `resolveCost(card, state)`.**

3. **Replacement effects ("instead of").** Before applying any event, run it past active replacement effects: *"If a unit would die, instead exile it."* This is how cards rewrite outcomes, not just numbers.

4. **The base rules are themselves effects.** "Draw 1 in the draw step" is a built-in effect sitting at the bottom of the stack. A card that says "draw 2 instead" is the exact same mechanism. If base rules and card effects share one representation, **"cards changing rules" becomes the only case, not a special case.**

### The rule-mutation taxonomy (our requirements checklist)
For **every rule** we document, we tag *how a card could alter it*. This is what surfaces the engine's required hook points before we write code:

| # | Mutation kind | Example | Engine hook it demands |
|---|---|---|---|
| 1 | **Modify a value** | "+1 movement to your units" | Modifier in the query layer |
| 2 | **Add/remove a step or phase** | "Skip your draw step" / "extra combat phase" | Phase list is data, walkable & mutable |
| 3 | **Change legality** | "Units may move through enemies" | Legal-move generator consults modifiers |
| 4 | **Replace an event** | "If it would die, banish instead" | Replacement-effect pipeline |
| 5 | **Trigger on an event** | "When a unit dies, draw a card" | Event subscription / triggered abilities |
| 6 | **Grant / redefine keywords** | "All your units gain Flying" | Keywords are queried, not baked into stats |
| 7 | **Alter win/loss conditions** | "You win if you control 5 hexes" | Win check is an evaluated effect, not `if` |

> **Rule of thumb during capture:** if a rule *cannot* be expressed as one of these seven, either it's a true invariant (rare — flag it explicitly) or we haven't modeled it flexibly enough yet.

### What stays hardcoded (invariants)
Not everything should be mutable — total flexibility is its own trap. A small set of **invariants** (e.g., "the game state is deterministic," "turns alternate unless an effect says otherwise," "an event pipeline exists") stays fixed and gives cards a stable substrate to modify. We'll maintain an explicit **invariants list** so the boundary is deliberate, not accidental.

---

## Part B — The ingestion workflow

You have lots of ideas (rules, card types, resource system, card properties, prototype cards) that aren't yet digital. We capture them in a **dependency order**: rules that other things reference come first, but we pull **2–3 prototype cards in early** as stress tests, because concrete cards reveal which rules must flex.

### Capture order

| Step | Domain | Why here | Output doc |
|---|---|---|---|
| 1 | **Glossary / vocabulary** | Consistent terms prevent rework. Nail names first. | `docs/rules/glossary.md` |
| 2 | **Match structure** | Board, zones, turn/phase order, win conditions — the skeleton everything hangs on. | `docs/rules/rules-structure.md` |
| 3 | **Resource system** | Economy shapes every card's cost and the whole feel. | `docs/rules/rules-resources.md` |
| 4 | **Card taxonomy** | The card *types* and what each type does. | `docs/rules/cards-taxonomy.md` |
| 5 | **Card anatomy (schema)** | The properties *every* card has → becomes the data schema. | `docs/rules/card-schema.md` |
| 6 | **Keyword / ability library** | Reusable effects cards compose from (the vocabulary of §Part A mechanisms). | `docs/rules/keywords.md` |
| 7 | **Prototype cards** | Fill the schema; stress-test the whole model against reality. | `docs/cards/*.md` |
| 8 | **Rule-mutation catalog** | Built *continuously* across all steps: the tagged list from Part A. | `docs/rules/rule-mutations.md` |

*(Steps 2–6 are the "rules skeleton." Step 8 is cross-cutting — we add to it every time we capture a rule.)*

### The loop for each domain
1. **You brain-dump** — messy is fine; talk/paste freely, one domain at a time.
2. **I structure it** into the domain doc, normalized and consistent, and **ask targeted clarifying questions** where there are gaps or contradictions.
3. **You review & correct** the doc.
4. **We tag mutability** — for each rule captured, add its row(s) to the rule-mutation catalog (Part A taxonomy). This is where we catch "oops, the engine can't express that yet."
5. **Move to the next domain.** Docs are living — we revisit freely.

### Card / effect representation (decided early, refined at step 5–6)
- **Data-driven.** Cards live in data files, not code.
- **Effects are structured data, not prose.** A card's effect references the **keyword/ability library** (step 6) — a small composable DSL — rather than free text. This is what lets a new card reuse existing mechanisms instead of needing new engine code. When a card genuinely needs a *new* primitive, that's a signal to extend the library deliberately.
- **Prototype cards (step 7) are the acid test:** if your existing prototype cards can't be expressed in the schema + keyword library, the model is wrong and we fix it *now*, on paper, for free.

---

## How to start

Pick either entry point — both work:
- **Top-down:** start at step 1–2 (glossary + structure) and build the skeleton.
- **Bottom-up:** dump your **prototype cards first**; I'll reverse-engineer the schema, keywords, and rules they imply, and we backfill the skeleton. *(Often more fun and reveals hidden requirements fast — good given you already have concrete cards.)*

Just start dumping whichever domain is clearest in your head. I'll organize it into the docs above and keep the rule-mutation catalog honest as we go.
