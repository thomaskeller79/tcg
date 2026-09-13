# The Economy (two resources, one pattern)

*The full resource model in one place. There are **two** resources — mana and Activation Points — and the same shape — "mana access + a private action budget" — repeats for every actor, **including the Champion and the Companion**. Learn it once, it applies everywhere (pillar 3).*

**Decisions:** D8 (terrain/mana), D9 (Champion economy), D10 (Action Points), D15 (Defend cost), D22 (Companion), D45 (Speed), D47 (Activation Capacity), D48 (first-player AP), D49 (Champion/Companion Attack), D57 (summoning sickness), D58 (Activation Points), D65 (Defend once per round) — `history/decisions.md`

---

## The two resources

Still **exactly two** — a Companion does **not** introduce a third. It adds a second **pool instance** of the same resource (mana), not a new resource.

| Resource | Topology | Held by | Refills | Spent on |
|---|---|---|---|---|
| **Mana** | **one shared pool per player**, plus **one private pool per Companion on the Island** (D22) | the player (shared pool, channeled by the Champion, D8/D9); each Companion (its own private pool, drawn by its own bonding) | as terrain is bonded/connected (D8), per the bonding root that owns each node | shared pool: casting spells, summoning units, **any** actor's mana-abilities. Private pool: **only that Companion's own printed abilities.** |
| **Activation Points (AP)** | **private, per permanent** (D58) | each **creature** (D10), the **Champion** (D9), each **Companion** (D22), and every non-Item Object (D58) | to max each eligible refresh (no carryover, rec) | move · fight · activate abilities · bond a terrain (Champion, Companion) · (Champion only) draw a card |

## The repeating pattern
Every board actor = **mana access** (shared pool, or a private pool if it's a Companion) + **its own private per-turn AP budget**.
- **Champion:** mana + its own AP pool, priced like a creature's but weighted toward draw/bond/abilities over combat — attacking is deliberately the costliest line, so *channelers channel, they don't fight* (D9) by cost design, not by a separate scarce resource.
- **Companion:** a **private** mana pool (not the shared one) + its own AP pool, with Bond priced more restrictively than the Champion's relative to its smaller AP total — draws mana like a channeler but can't share it (D22).
- **Creature:** shared mana + AP. This is where most board action lives.

Abilities **bridge the two economies**: a creature ability can cost `X mana + Y AP`; a Champion ability costs the same shape — `mana + AP`. Same grammar at both scales, no special case for the Champion.

## Mana is global; AP is local
- **Mana = one number per player.** The Champion's spells, its abilities, and *every* creature's mana-abilities all draw from the same pool. Channeling (D8) is what fills it. This is why the Champion is "a channeler": it's the shared tap the whole army drinks from.
- **AP = private.** Each creature spends only its own AP; the Champion spends only its own AP. These never pool.

## Creatures are three numbers: Attack / Life / Activation Points (D10)
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

### Speed (Slow/Quick/Reactive/Instant, D45)
Every card/ability has one of four **Speeds**, gated by what's currently sitting in **Pending** (the Aether's next-to-resolve zone, D38):
- **Slow** — controller's main phase only, Pending empty.
- **Quick** — whenever only **Physical Traces** (the trace left by Move/Attack/Ascend/Descend) are in Pending.
- **Reactive** — whenever no **Instant Trace** is in Pending.
- **Instant** — any time; resolves **fully atomically**, zero response window for anyone, not even another Instant — a deliberate simplification that keeps the top speed tier free of unbounded stack/priority-passing complexity.

Speed governs *when* a card/ability may be played; `!`/`*` still govern *how its AP is consumed* — independent axes, same as before. Spending AP reactively still draws from the actor's normal AP budget, never a separate reactive pool — so answering on the opponent's turn means that AP had to be held in reserve since the actor's own last turn. Applies generally, for any actor. Full detail: `interaction-stack.md`.

The `x!!AP` ("double-exhaust") idea remains a proposed-not-adopted alternative — see `history/playtest-variants.md`.

### Defending (D15, D65)
Defending costs **`0*AP`** — free, but at most **once per round** per actor, completely decoupled from remaining AP in both directions. This is the one exception to the plain `*` flavor's "once per turn": with Neutral turns (D60) putting up to four turns in a round, "once per turn" would let a single creature defend up to four times a round, well beyond what the cap was calibrated for — once per round keeps total defensive capacity stable regardless of how many turns fall in a round. The reset point is still the controller's own turn-start, the same cadence AP already refreshes on — for a player that's their own one turn per round as before; for a Neutral permanent, its assigned neutral turn. `cannot defend` remains an occasional negative keyword. A card can still deliberately spend a creature's Defend for the round as part of a strong ability's own effect (the MTG "tap cost" flavor) — explicit card-level data, not a base-rule exception. Rejected alternatives and the bug that ruled out the original toggle: `history/decisions.md` D15, D65, `history/playtest-variants.md`.

## Champion AP — the same model as a creature (D9)
The Champion **has an AP value and spends it directly**, exactly like a creature — one movement/combat/action query for every entity, no gating resource and no second combat model. The Champion's specific action costs (draw/bond/move/abilities) differ from a plain creature's defaults and live in `champions.md`; its **Attack cost matches the generic Actor default** (`3!AP` doubled to `6!AP` while network-bonded, D49) — same for Companion. **First-player asymmetry (D48):** the Champion of the player going first enters with reduced starting AP (placeholder 4) instead of its full Activation Points; the second player's Champion enters at full Activation Points.

## Activation Points: one resource for every permanent but Item (D58)
**Activation Points** is the one resource every permanent except Item holds — Creature, Champion, Companion, Structure, Terrain, Ruin, Grave alike — each with its own value **explicitly printed on its card data**, refreshing to that max every eligible refresh (D57, below), no carryover by default. There is no default value — a card's printed number has to be large enough to actually afford its own priciest self-funded ability, since holding Activation Points and funding an ability from them are the same thing. Values are kept comparable across types on purpose, so a generic effect ("target permanent gains 2 Activation Points") means the same thing regardless of what it targets.

**Payment is one rule, for every resource, not two separate rules by type:** to pay a cost, climb from the entity itself and stop at the **first** node — inclusive of the entity itself — that holds that resource (full detail: `ownership.md`). Activation Points and Life never climb simply because the paying entity is the very first node checked and already holds them; mana climbs because only a Champion or Companion ever hold it, so the walk keeps going until it reaches one. A card may still explicitly name a different payer than this default walk would reach (e.g. "this creature's controller pays 2 life") — a deliberate override, not something the general rule does on its own.

**Consequence:** an Object's own Activation-Points-costed ability (e.g. a Structure's printed activated ability) is funded by the Object itself, not by whoever bonds/controls it — decoupled from the controlling player's own action economy. It's also what lets a Neutral Object act on its own at all: an Activation-Points-only ability is always self-payable once Neutral, with no bonder required; a mana-inclusive ability still needs a live root supplying the mana. See `neutral-permanents.md`.

**Life is not a third resource alongside mana/Activation Points.** Whichever permanents track their own Life (Champion, Companion, Creature, Structure, D10/D24) do so the same way they track Activation Points, so a "pay N life" ability on one of them spends its own Life directly — self-paid, no climbing — unless a card explicitly names the controller as payer instead (`ownership.md`).

## Summoning sickness (and Haste) is about casting, not type (D57)
A permanent enters with 0 Activation Points — summoning-sick, D14's pessimistic default — **if and only if it entered the Island through the casting/Aether procedure** (D33/D46). **Creature, Companion, and Structure are all cast**, so all three are summoning-sick by default, refreshing at their first eligible refresh; **Haste** (the existing positive keyword) removes this for any of them, not just Creature. **Terrain (placed via a map's home ground / neutral ground) and the Champion (placed at match setup) never go through the casting procedure**, so summoning sickness was never a question for them in the first place — not a type-based exception, the rule about casting simply doesn't apply. The Champion's own separate first-player starting-AP reduction (D48) is an unrelated balancing lever, not summoning sickness. A Ruin or Grave, being a transformation of a permanent already on the Island rather than something newly cast, is unaffected either way.

## Open sub-levers (tuning, not structure)
1. **AP refresh** — rec refill to max each turn, no carryover.
2. **Champion base AP** — the baseline total (proposed: 7, tuning) that funds all of draw/bond/move/attack/ability each turn. See `champions.md`.
3. **First-player Champion starting AP** (D48) — placeholder 4, real number deferred.
4. **Trace Duration** (D50) — placeholder 5 rounds, real number deferred.

## Complexity discipline
Total systems: **mana + AP + terrain network + Pending/Speed + perception**. Mitigation: every AP cost is a **small integer**, resist per-action special cases, let depth come from combinations (pillar 3, tension #6 in PLAN).

## Invariant vs. mutable
- **Invariant:** mana is a single shared pool per player; every permanent but Item has its own private Activation Points budget, reached through the same payment rule (D58); every actor reaches mana through the same query; summoning sickness applies to whatever is cast, regardless of type (D57).
- **Mutable (card-driven):** Activation Points totals and per-action costs, what refills/carries over, whether defending costs AP, extra Activation Points, mana/AP ability costs — all effects/queries (pillar 5).
