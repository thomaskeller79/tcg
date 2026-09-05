# The Economy (two resources, one pattern)

*The full resource model in one place. There are **two** resources — mana and Action Points — and the same shape — "mana access + a private action budget" — repeats for every actor, **including the Champion and the Companion**. Learn it once, it applies everywhere (pillar 3).*

**Decisions:** D8 (terrain/mana), D9 (Champion economy), D10 (Action Points), D15 (Defend cost), D22 (Companion), D45 (Speed, supersedes D23's instant-speed marker), D47 (Activation Capacity), D48 (first-player AP), D49 (Champion/Companion Attack) — `history/decisions.md`

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

### The `*` cost notation (once-per-turn)
A second cost flavor, distinct from `!`: **`x*AP`** — spend exactly `x`, and this specific action may be used **at most once per turn**, regardless of leftover AP or AP gained later that turn.

This is **not** the same thing as `!`: `!` drains the *whole remaining pool* but doesn't itself prevent reuse if AP is later refilled (the deliberate "surprise blocker" combo above); `*` doesn't touch the rest of the pool at all, it just locks out repeats of that one action for the turn.

Used by the Champion's `5*AP: Draw` and `2*AP: Bond` actions (`champions.md`) — `Draw`/`Bond` need to stay usable *together* in one turn while each staying capped to once/turn, which `!` cannot express without also zeroing the pool. Also used by the Companion's own Bond ability (`companions.md`) — same flavor, priced higher relative to the Companion's smaller AP total so it crowds out the rest of the turn.

### Speed (Slow/Quick/Reactive/Instant, D45 — retires the `^` marker and D23)
Timing is no longer a per-ability marker layered on top of a card-level instant/sorcery split (the old D5/D23 model). Every card/ability now has one of four **Speeds**, gated by what's currently sitting in the Aether's **Pending** region (D38) rather than a fixed per-phase window list:
- **Slow** — controller's main phase only, Pending empty (≈ the old sorcery-speed default).
- **Quick** — whenever only **Physical Traces** (the trace left by Move/Attack/Ascend/Descend) are in Pending.
- **Reactive** — whenever no **Instant Trace** is in Pending.
- **Instant** — any time; resolves **fully atomically**, zero response window for anyone, not even another Instant — a deliberate simplification that keeps the top speed tier free of unbounded stack/priority-passing complexity.

Speed governs *when* a card/ability may be played; `!`/`*` still govern *how its AP is consumed* — independent axes, same as before. Spending AP reactively still draws from the actor's normal AP budget, never a separate reactive pool — so answering on the opponent's turn means that AP had to be held in reserve since the actor's own last turn. Applies generally, for any actor. Full detail: `interaction-stack.md`.

The `x!!AP` ("double-exhaust") idea remains a proposed-not-adopted alternative — see `history/playtest-variants.md`.

### Defending (D15)
Defending costs **`0*AP`** — free, but at most once per turn per actor, via the same `*` once-per-turn flavor as Bond/Draw. Completely decoupled from remaining AP in both directions. `cannot defend` remains an occasional negative keyword. A card can still deliberately spend a creature's Defend for the turn as part of a strong ability's own effect (the MTG "tap cost" flavor) — explicit card-level data, not a base-rule exception. Rejected alternatives and the bug that ruled them out: `history/decisions.md` D15, `history/playtest-variants.md`.

## Champion AP — the same model as a creature (D9)
The Champion **has an AP value and spends it directly**, exactly like a creature — one movement/combat/action query for every entity, no gating resource and no second combat model. The Champion's specific action costs (draw/bond/move/abilities) differ from a plain creature's defaults and live in `champions.md`; its **Attack cost is now unified with the generic Actor default** (`3!AP` doubled to `6!AP` while network-bonded, D49) rather than a bespoke figure — same for Companion. **First-player asymmetry (D48):** the Champion of the player going first enters with reduced starting AP (placeholder 4) instead of full Activation Capacity; the second player's Champion enters at full Activation Capacity.

## Non-Actor permanent funding (D24–D26); Activation Capacity for Objects (D47)
For the three **Actors** (D44) — Creature, Champion, Companion — "whose AP, whose mana" is always unambiguous: an Actor spends only its own AP, and either the shared pool (Creature, Champion) or its own private pool (Companion). That symmetry **breaks** for **Objects** that have no *funding* AP of their own: Terrain, Item, Structure. The full resolution — an ownership **tree** where every permanent has exactly one parent, and paying a cost means climbing to the nearest node that actually holds that resource — lives in `ownership.md`; Terrain/Structure/Item specifics are in `resources-terrain.md` and `structures-items.md`.

**Separately (D47), Terrain, Structure, Ruins, and Graves each also carry their own small Activation Capacity** (default 1) — purely to self-gate "has this printed ability already fired this round," reusing the standard AP-refresh mechanism rather than a bespoke once-per-turn clause. This is **additive** to the funding model above, not a replacement: Activation Capacity never pays anything, it only tracks reuse. These Objects get full Activation Capacity immediately on entering play, unlike an Actor (which still enters with 0 AP, D10/D20). Item is the one Object type that does **not** get this — it stays entirely without AP of any kind.

**Life is not a third resource alongside mana/AP.** A creature already tracks its own Life (D10) the same way it tracks its own AP, so a creature's own "pay N life" ability spends its own Life directly — self-paid, same as AP, no climbing — unless a card explicitly names its controller as the payer instead (`ownership.md`).

## Open sub-levers (tuning, not structure)
1. **AP refresh** — rec refill to max each turn, no carryover.
2. **Champion base AP** — the baseline total (proposed: 7, tuning) that funds all of draw/bond/move/attack/ability each turn. See `champions.md`.
3. **First-player Champion starting AP** (D48) — placeholder 4, real number deferred.
4. **Trace Duration** (D50) — placeholder 5 rounds, real number deferred.

## Complexity discipline
Total systems: **mana + AP + terrain network + Pending/Speed + perception**. Mitigation: every AP cost is a **small integer**, resist per-action special cases, let depth come from combinations (pillar 3, tension #6 in PLAN).

## Invariant vs. mutable
- **Invariant:** mana is a single shared pool per player; each creature has a private AP budget; the Champion has a private AP budget too (D9), spent the same way. Every actor reaches mana through the same query.
- **Mutable (card-driven):** AP totals and per-action AP costs, what refills/carries over, whether defending costs AP, extra AP, mana/AP ability costs — all effects/queries (pillar 5).
