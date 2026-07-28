# Design — The Economy (three resources, one pattern)

*The full resource model in one place. There are **three** resources in **two** topologies, and the same shape — "shared mana + a private action budget" — repeats for every actor. Learn it once, it applies everywhere (pillar 3).*

**Status:** Design note · **Date:** 2026-07-27 · Decisions: D8 (terrain/mana), D9 (Channel), D10 (Action Points)

---

## The three resources

| Resource | Topology | Held by | Refills | Spent on |
|---|---|---|---|---|
| **Mana** | **One shared pool per player** | the player (channeled by the Champion, D8/D9) | as terrain is bonded/connected (D8) | casting spells, summoning units, **any** actor's mana-abilities |
| **Channel** | private, **1 per turn** | the **Champion** (D9) | each turn | bond a terrain · act as a creature · activate a Champion ability |
| **Action Points (AP)** | **private, per creature** | each **creature** (D10) | to max each turn (no carryover, rec) | move · fight · activate creature abilities |

## The repeating pattern
Every board actor = **access to the one shared mana pool** + **its own private per-turn action budget**.
- **Champion:** mana + a tiny private budget (**1 Channel**). Deliberately action-starved — *channelers channel, they don't fight* (D9).
- **Creature:** mana + a rich private budget (**AP**). This is where board action lives.

Abilities **bridge the two economies**: a creature ability can cost `X mana + Y AP`; a Champion ability can cost `mana + the Channel`. Same grammar at both scales.

## Mana is global; action budgets are local
- **Mana = one number per player.** The Champion's spells, its abilities, and *every* creature's mana-abilities all draw from the same pool. Channeling (D8) is what fills it. This is why the Champion is "a channeler": it's the shared tap the whole army drinks from.
- **AP / Channel = private.** Each creature spends only its own AP; the Champion spends only its own Channel. These never pool.

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

### Defending — two variants under playtest (D15)
The defend-cost rule is being resolved **empirically**, not on paper (it collapses the redundant `defended`-flag into AP). Two candidates:
- **V1 — `1!AP` (exhaust):** defending costs `1!AP` → at most once/turn, and only if AP remains (acting can spend it). Keeps AP in ℕ. Gains the +1-AP ambush-blocker combo above.
- **V2 — delete it:** defending free + unlimited; **persistent damage (D14)** is the limiter (Life = defensive stamina). No per-turn defend state.
`cannot defend` remains an occasional negative keyword under either. See D15.

## Champion ↔ AP reconciliation (rec: uniform)
The Champion's "act as a creature" Channel option (D9) spends **AP**, like any creature — the Champion **has an AP value** but may spend it **only** on a turn it devotes its Channel to *act*. One movement/combat model for every entity (one engine query), with the Champion's action simply gated behind a scarce Channel. *(Alternative: Champion has no AP and "act" is a fixed mini-behavior — rejected as a second combat model.)*

## Open sub-levers (tuning, not structure)
1. ~~Move cost~~ → default `1AP: Move` (D10).
2. ~~Attack cost / multi-attack~~ → default `3!AP: Attack`; multi-attack via a custom non-`!` cost (D10).
3. ~~Defending cost AP~~ → **free**; `cannot defend` is a keyword (D10).
4. **AP refresh** — rec refill to max each turn, no carryover.
5. **Champion base AP** — how much can a Champion do on an "act" turn? (Modest — it's not a warrior.)

## Complexity discipline
Total systems now: **mana + Channel + AP + terrain network + stack + perception**. AP is *net-neutral* (it replaces Movement + once-per-turn rules), but the whole is rich. Mitigation: every AP cost is a **small integer**, resist per-action special cases, let depth come from combinations (pillar 3, tension #6 in PLAN).

## Invariant vs. mutable
- **Invariant:** mana is a single shared pool per player; each creature has a private AP budget; each Champion has one Channel/turn. Every actor reaches mana through the same query.
- **Mutable (card-driven):** AP totals and per-action AP costs, what refills/carries over, whether defending costs AP, extra Channels, mana/AP ability costs — all effects/queries (pillar 5).
