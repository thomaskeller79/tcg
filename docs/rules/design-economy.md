# Design — The Economy (two resources, one pattern)

*The full resource model in one place. There are **two** resources — mana and Action Points — and the same shape — "mana access + a private action budget" — repeats for every actor, **including the Champion and, as of D22, the Companion**. Learn it once, it applies everywhere (pillar 3).*

**Status:** Design note · **Date:** 2026-07-27 · revised 2026-08-06, 2026-08-07 · Decisions: D8 (terrain/mana), D9 (Champion economy), D10 (Action Points), D22 (Companion)

---

## The two resources

Still **exactly two** — a Companion does **not** introduce a third. It adds a second **pool instance** of the same resource (mana), not a new resource.

| Resource | Topology | Held by | Refills | Spent on |
|---|---|---|---|---|
| **Mana** | **one shared pool per player**, plus **one private pool per Companion in play** (D22) | the player (shared pool, channeled by the Champion, D8/D9); each Companion (its own private pool, drawn by its own bonding) | as terrain is bonded/connected (D8), per the bonding root that owns each node | shared pool: casting spells, summoning units, **any** actor's mana-abilities. Private pool: **only that Companion's own printed abilities.** |
| **Action Points (AP)** | **private, per actor** | each **creature** (D10), the **Champion** (D9), and each **Companion** (D22) | to max each turn (no carryover, rec) | move · fight · activate abilities · bond a terrain (Champion, Companion) · (Champion only) draw a card |

## The repeating pattern
Every board actor = **mana access** (shared pool, or a private pool if it's a Companion) + **its own private per-turn AP budget**.
- **Champion:** mana + its own AP pool, priced like a creature's but weighted toward draw/bond/abilities over combat — attacking is deliberately the costliest line, so *channelers channel, they don't fight* (D9) by cost design, not by a separate scarce resource.
- **Companion:** a **private** mana pool (not the shared one) + its own AP pool, with Bond priced more restrictively than the Champion's relative to its smaller AP total — draws mana like a channeler but can't share it (D22).
- **Creature:** shared mana + AP. This is where most board action lives.

Abilities **bridge the two economies**: a creature ability can cost `X mana + Y AP`; a Champion ability costs the same shape — `mana + AP`. Same grammar at both scales, no special case for the Champion.

## Mana is global; AP is local
- **Mana = one number per player.** The Champion's spells, its abilities, and *every* creature's mana-abilities all draw from the same pool. Channeling (D8) is what fills it. This is why the Champion is "a channeler": it's the shared tap the whole army drinks from.
- **AP = private.** Each creature spends only its own AP; the Champion spends only its own AP. These never pool.

## Creatures are three numbers: Attack / Life / Action Points (D10)
AP **subsumes** the old separate stats rather than adding to them:
- **Movement stat is gone** → moving costs AP; a creature's "speed" *is* its AP.
- **Range demotes to a keyword** (`Ranged N`); most creatures are melee. The three *defining* numbers stay Attack / Life / AP.

## Move and attack are default *abilities*, not rules (D10)
There is no hardcoded move/attack logic. Every creature carries two **default abilities**, each **replaceable** by a creature-specific version:
- **`1AP: Move`** — one hex per AP.
- **`3!AP: Attack`** — see the `!` cost below.

This is pillar 5 at its purest: the base rules *are* abilities, so the engine needs only an ability/cost system — no special move/attack code.

### The `!` cost notation
An AP cost is written `xAP` or `x!AP`:
- **`xAP`** — spend exactly `x`. Leftover AP remains usable (enables multi-action, e.g. a custom `1AP` attack that permits hit-and-run or multi-attack).
- **`x!AP`** — **require `x`, then consume *all* remaining AP.** A single such action ends the creature's turn-actions.

**No-multi-attack is emergent, not a rule:** default attack is `3!AP`, so attacking drains the creature; a creature printed with `3AP: Attack` (no bang) *could* attack twice. The engine's cost system must support "require-x-consume-all" as a cost flavor; the specific values are tuning/content.

Read `!` as **"exhaust."** A creature at 0 AP *looks* spent — which is why a card that grants **+1 AP** is deceptively strong (an apparently-tapped creature can suddenly act/block; feeds pillar 6).

### The `*` cost notation (once-per-turn) — adopted 2026-08-06 for the Champion
A second cost flavor, distinct from `!`: **`x*AP`** — spend exactly `x`, and this specific action may be used **at most once per turn**, regardless of leftover AP or AP gained later that turn.

This is **not** the same thing as `!`: `!` drains the *whole remaining pool* but doesn't itself prevent reuse if AP is later refilled (that's the deliberate "surprise blocker" combo above); `*` doesn't touch the rest of the pool at all, it just locks out repeats of that one action for the turn. Conflating the two would break either behavior, so they're kept as separate flavors.

Introduced for the Champion's `5*AP: Draw` and `2*AP: Bond` actions (see `design-champions.md`) — `Draw`/`Bond` need to stay usable *together* in one turn (their costs sum to a typical Champion's full AP budget) while each staying capped to once/turn, which `!` cannot express without also zeroing the pool. **Now also used for the Companion's own Bond ability (D22, `design-companions.md`)** — same flavor, priced higher relative to the Companion's smaller AP total so it crowds out the rest of the turn. Whether `*` generalizes further, to plain creatures, and the exact numbers, are still open/tuning.

### The `^` marker (instant-speed) — adopted 2026-08-07 (D23)
A third axis, independent of `!`/`*`: **`^`** prefixed on any AP cost marks that specific *ability* as playable **instant-speed** — usable in any priority window (D5), on either player's turn. **Default (no `^`): sorcery-speed** — usable only during the actor's controller's own Action phase, and only while the stack (the Aether's pending region, D16) is empty. Mirrors MtG's "sorcery speed" exactly, but generalized to the **ability** level rather than the card level: D5's existing instant/sorcery split already covers whole cards; `^` lets individual abilities *on the same card* differ (a Champion might have one sorcery-speed ability and one reactive one).

Composes freely with `!`/`*`: `^2*AP`, `^3!AP`, `^1AP` are all valid — `^` says *when*, `!`/`*` say *how the AP is consumed*. Spending AP reactively still draws from the actor's normal AP budget, never a separate reactive pool — so using an instant-speed ability on the opponent's turn means that AP had to be held in reserve since the actor's own last turn. Introduced to resolve `design-champions.md`'s former "can Champion abilities be reactive?" open question **generally**, for any actor (creature, Champion, Companion), instead of as a Champion-specific rule.

### Defending — two variants under playtest (D15)
The defend-cost rule is being resolved **empirically**, not on paper (it collapses the redundant `defended`-flag into AP). Two candidates:
- **V1 — `1!AP` (exhaust):** defending costs `1!AP` → at most once/turn, and only if AP remains (acting can spend it). Keeps AP in ℕ. Gains the +1-AP ambush-blocker combo above.
- **V2 — delete it:** defending free + unlimited; **persistent damage (D14)** is the limiter (Life = defensive stamina). No per-turn defend state.
`cannot defend` remains an occasional negative keyword under either. See D15.

## Champion AP — the same model as a creature (D9, revised 2026-08-06)
The Champion **has an AP value and spends it directly**, exactly like a creature — one movement/combat/action query for every entity, no gating resource and no second combat model. *(The earlier design gated the Champion's AP behind a single per-turn "Channel" action; that's superseded — see `design-champions.md`. Alternative considered and still rejected: Champion has no AP and "act" is a fixed mini-behavior — that would be a second combat model.)* The Champion's specific action costs (draw/bond/move/attack/abilities) differ from a plain creature's defaults and live in `design-champions.md`.

## Open: who funds a non-actor permanent's ability? (surfaced 2026-08-07, unresolved)
For the three **actors** — Creature, Champion, Companion — "whose AP, whose mana" is always unambiguous: an actor spends only its own AP, and either the shared pool (Creature, Champion) or its own private pool (Companion, D22). That symmetry **breaks** for permanents that have **no AP of their own**: Terrain, Item, Structure. Each needs its own answer for who pays when *its* printed ability activates:

- **Terrain — RESOLVED** (`design-resources-terrain.md`): activated by whoever **controls** it (the root — Champion or Companion — that bonded it); costs **mana only, no AP**; funded from that controller's own pool (shared if Champion-bonded, that Companion's private pool if Companion-bonded). Uncontrolled (unbonded) terrain's ability can't be activated by anyone.
- **Item — OPEN.** Working hypothesis (unconfirmed): an Item is carried/equipped by a creature, so its ability spends **the carrying creature's own AP** + mana from the **shared pool**. Unaddressed: what happens if a Champion or a Companion is the one carrying it instead of a plain creature — does the cost then shift to *that* actor's AP and mana source (private, if a Companion)?
- **Structure — OPEN.** Working hypothesis (unconfirmed): a Structure is Champion-controlled (stationary, never carried), so its ability spends **the Champion's own AP** + **the Champion's/shared mana**. Unaddressed: can a Companion ever control a Structure instead, and if so does the cost mirror the Companion's own AP + private mana the way Terrain already does?

Not resolved here — flagged to pick up alongside the Structure/Item design pass (PLAN §8 Track A step 0).

## Open sub-levers (tuning, not structure)
1. ~~Move cost~~ → default `1AP: Move` (D10); Champion's own move costs are bespoke, see `design-champions.md`.
2. ~~Attack cost / multi-attack~~ → default `3!AP: Attack`; multi-attack via a custom non-`!` cost (D10).
3. ~~Defending cost AP~~ → **free**; `cannot defend` is a keyword (D10). *(Champion-specific candidates under playtest — see D15, `design-champions.md` Open question 2.)*
4. **AP refresh** — rec refill to max each turn, no carryover.
5. **Champion base AP** — the baseline total (proposed: 7, tuning) that funds all of draw/bond/move/attack/ability each turn. See `design-champions.md`.

## Complexity discipline
Total systems now: **mana + AP + terrain network + stack + perception** — one fewer than before, since the Champion's Channel folded into AP (2026-08-06). Mitigation: every AP cost is a **small integer**, resist per-action special cases, let depth come from combinations (pillar 3, tension #6 in PLAN).

## Invariant vs. mutable
- **Invariant:** mana is a single shared pool per player; each creature has a private AP budget; the Champion has a private AP budget too (D9), spent the same way. Every actor reaches mana through the same query.
- **Mutable (card-driven):** AP totals and per-action AP costs, what refills/carries over, whether defending costs AP, extra AP, mana/AP ability costs — all effects/queries (pillar 5).
