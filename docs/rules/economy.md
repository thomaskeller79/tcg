# The Economy (two resources, one pattern)

*The full resource model in one place. There are **two** resources — mana and Activation Points — and the same shape — "mana access + a private action budget" — repeats for every actor, **including the Champion and the Companion**. Learn it once, it applies everywhere (pillar 3).*

**Decisions:** D8 (terrain/mana), D9 (Champion economy), D10 (Action Points), D15 (Defend cost), D22 (Companion), D45 (Speed), D47 (Activation Capacity), D48, D49 (Champion/Companion Attack), D58 (Activation Points), D65 (Defend once per round), D76 (first-turn AP asymmetry), D77 (mana accounting), D87 (Terrain holds no AP), D88, D110 (Remnant holds no AP), D89 (summoning sickness), D92 (Instant blocks active play), D114 (Setup is entering; Haste X), D116 (`~` cost; Attack `3~AP`, Defend `1~AP`) — `history/decisions.md`

---

## The two resources

Still **exactly two** — a Companion does **not** introduce a third. It adds a second **pool instance** of the same resource (mana), not a new resource.

| Resource | Topology | Held by | Refills | Spent on |
|---|---|---|---|---|
| **Mana** | **one pool per Champion**, plus **one pool per Companion on the Island** (D22) | the Champion (its own pool, D8/D9); each Companion (its own separate pool, drawn by its own bonding) | live, the instant each bonded producer is reachable (D8/D77) | Champion's pool: casting spells, summoning units, **any** actor's mana-abilities. A Companion's pool: everything in that Companion's own subtree (`ancestry.md`) — never a different payer's abilities or spells. |
| **Activation Points (AP)** | **private, per permanent** (D58) | each **creature** (D10), the **Champion** (D9), each **Companion** (D22), and each **Structure** (D58, D87) | to max each eligible refresh (no carryover, rec) | move · fight · activate abilities · bond a terrain (Champion, Companion) · (Champion only) draw a card |

## The repeating pattern
Every board actor = **mana access** (the Champion's pool, or its own pool if it's a Companion) + **its own private per-turn AP budget**.
- **Champion:** mana + its own AP pool, priced like a creature's but weighted toward draw/bond/abilities over combat — attacking is deliberately the costliest line, so *channelers channel, they don't fight* (D9) by cost design, not by a separate scarce resource.
- **Companion:** its **own** mana pool (separate from the Champion's) + its own AP pool, with Bond priced more restrictively than the Champion's relative to its smaller AP total — draws mana like a channeler but can't share it (D22).
- **Creature:** shared mana + AP. This is where most board action lives.

Abilities **bridge the two economies**: a creature ability can cost `X mana + Y AP`; a Champion ability costs the same shape — `mana + AP`. Same grammar at both scales, no special case for the Champion.

## Mana is global; AP is local
- **Mana = one number per Champion.** The Champion's spells, its abilities, and *every* creature's mana-abilities all draw from that same pool (unless a Companion intercepts them into its own, `ancestry.md`). Channeling (D8) is what fills it. This is why the Champion is "a channeler": it's the tap the whole army drinks from.
- **AP = private.** Each creature spends only its own AP; the Champion spends only its own AP. These never pool.

## Creatures are three numbers: Attack / Life / Activation Points (D10)
AP **subsumes** the old separate stats rather than adding to them:
- **Movement stat is gone** → moving costs AP; a creature's "speed" *is* its AP.
- **Range demotes to a keyword** (`Ranged N`); most creatures are melee. The three *defining* numbers stay Attack / Life / AP.

## Move, attack and defend are default *abilities*, not rules (D10, D116)
There is no hardcoded move/attack logic. Every creature carries three **default abilities**, each **replaceable** by a creature-specific version:
- **`1AP: Move`** — one hex per AP.
- **`3~AP: Attack`** — see the `~` cost below.
- **`1~AP: Defend`** — see Defending below.

This is pillar 5 at its purest: the base rules *are* abilities, so the engine needs only an ability/cost system — no special move/attack code.

### AP cost flavors: `xAP`, `x!AP`, `x~AP`, `x*AP`
- **`xAP`** — spend exactly `x`. Leftover AP remains usable (enables multi-action, e.g. a custom `1AP` attack that permits hit-and-run or multi-attack).
- **`x!AP`** ("exhaust") — **require `x`, then consume *all* remaining AP.** The permanent can't act again until its next refresh, including on other players' turns: no defending, no reactive abilities.
- **`x~AP`** ("done for this turn", D116) — **spend exactly `x`; the permanent can't activate abilities until the end of this turn.** Leftover AP stays and is usable again from the next turn on — in particular on the opponents' turns, for defending.
- **`x*AP`** ("once per cycle") — **spend exactly `x`; this specific action may be used at most once per own-turn cycle** (from the controller's turn start to its next one), regardless of leftover AP or AP gained later.

**No-multi-attack and no hit-and-run are emergent, not rules:** default attack is `3~AP`, so after attacking the creature can do nothing else this turn; a creature printed with `3AP: Attack` *could* attack twice, or attack and move away.

**`!` is for powerful abilities:** a strong ability priced with `!` costs the permanent its defence until its next refresh — power is balanced against the ability to defend. Read `!` as **"exhaust."** A creature at 0 AP *looks* spent — which is why a card that grants **+1 AP** is deceptively strong (an apparently-exhausted creature can suddenly act or defend; feeds pillar 6).

**`*`** is used by the Champion's `5*AP: Draw` and `2*AP: Bond` actions (`champions.md`) — `Draw`/`Bond` need to stay usable *together* in one turn while each staying capped, which neither `!` nor `~` can express. Also used by the Companion's own Bond ability (`companions.md`) — same flavor, priced higher relative to the Companion's smaller AP total so it crowds out the rest of the turn.

### Speed (Slow/Quick/Reactive/Instant, D45)
Every card/ability has one of four **Speeds**, gated by what's currently sitting in **Pending** (the Aether's next-to-resolve zone, D38):
- **Slow** — controller's main phase only, Pending empty.
- **Quick** — whenever only **Physical Traces** (the trace left by Move/Attack/Ascend/Descend) are in Pending.
- **Reactive** — whenever no **Instant Trace** is in Pending.
- **Instant** — whenever no Instant is in Pending; while it sits in Pending, nothing can be actively put into the Aether and `Now` advances automatically until it has resolved — zero response window for anyone, not even another Instant. Triggers are still added and resolve normally. A deliberate simplification that keeps the top speed tier free of unbounded stack/priority-passing complexity.

Speed governs *when* a card/ability may be played; `!`/`~`/`*` still govern *how its AP is consumed* — independent axes, same as before. Spending AP reactively still draws from the actor's normal AP budget, never a separate reactive pool — so answering on the opponent's turn means that AP had to be held in reserve since the actor's own last turn. Applies generally, for any actor. Full detail: `interaction-stack.md`.

The `x!!AP` ("double-exhaust") idea remains a proposed-not-adopted alternative — see `history/playtest-variants.md`.

### Defending (D116)
Defending costs **`1~AP`**: 1 AP, and the defender can't activate abilities for the rest of that turn. Since every opponent turn is a separate turn (Neutral A, the other Champion, Neutral B), a creature can defend **once per opponent turn**, as long as it has AP for it — every AP held back from its own turn buys one more defence in a later opponent turn. A creature that went all in on its own turn (moved and attacked with everything) can't defend until its next refresh; one that used a `!` ability can't either. A card granting AP makes a surprise defender. The cost to defending is also Life: each defence takes the attacker's damage. **Defender** (positive keyword): this creature has `1AP: Defend` instead — it can defend any number of attacks per turn while its AP lasts. `cannot defend` remains an occasional negative keyword. Rejected alternatives and the scenarios that decided it: `history/decisions.md` D15, D116, `history/playtest-variants.md`.

## Champion AP — the same model as a creature (D9)
The Champion **has an AP value and spends it directly**, exactly like a creature — one movement/combat/action query for every entity, no gating resource and no second combat model. The Champion's specific action costs (draw/bond/move/abilities) differ from a plain creature's defaults and live in `champions.md`; its **Attack cost matches the generic Actor default** (`3~AP` doubled to `6~AP` while network-bonded, D49, D116) — same for Companion. **First-turn asymmetry (D114):** Setup raises both Champions to at least the same starting AP (placeholder 4); the first-move-advantage lever is that Setup is Champion A's first Beginning phase, so Champion A gets no AP-refresh on turn 1 (`setup.md`).

## Activation Points: one resource for Champion, Companion, Creature and Structure (D58, D87, D88)
**Activation Points** is the one resource held by Champion, Companion, Creature and Structure alike — Item, Terrain and Remnant hold none (a Terrain's AP cost climbs to its bonder, D87; a Remnant is what a fallen creature leaves, D110) — each with its own value **explicitly printed on its card data**, refreshing to that max every eligible refresh (D89, below), no carryover by default. There is no default value — a card's printed number has to be large enough to actually afford its own priciest self-funded ability, since holding Activation Points and funding an ability from them are the same thing. Values are kept comparable across types on purpose, so a generic effect ("target permanent gains 2 Activation Points") means the same thing regardless of what it targets.

**Payment is one rule, for every resource, not two separate rules by type:** to pay a cost, climb from the entity itself and stop at the **first** node — inclusive of the entity itself — that holds that resource (full detail: `ancestry.md`). Activation Points and Life never climb simply because the paying entity is the very first node checked and already holds them; mana climbs because only a Champion or Companion ever hold it, so the walk keeps going until it reaches one. A card may still explicitly name a different payer than this default walk would reach (e.g. "this creature's controller pays 2 life") — a deliberate override, not something the general rule does on its own.

**Consequence:** a Structure's own Activation-Points-costed ability is funded by the Structure itself, not by whoever bonds/controls it — decoupled from the controlling Champion's own action economy. It's also what lets a Neutral Structure act on its own at all: an Activation-Points-only ability is always self-payable once Neutral, with no bonder required; a mana-inclusive ability still needs a live root supplying the mana. See `neutral-permanents.md`.

**Life is not a third resource alongside mana/Activation Points.** Whichever permanents track their own Life (Champion, Companion, Creature, Structure, D10/D24) do so the same way they track Activation Points, so a "pay N life" ability on one of them spends its own Life directly — self-paid, no climbing — unless a card explicitly names the controller as payer instead (`ancestry.md`).

## Summoning sickness (and Haste): 0 Activation Points on entering (D89, D114)
**Every permanent entering the Island enters with 0 Activation Points** — cast, created by an effect, flickered, however it got there — and refreshes at its controller's next Beginning phase. That is all summoning sickness is (D14's pessimistic default): a permanent that wasn't there at the beginning of its controller's turn can't act yet. This includes permanents placed at Setup (D114, `setup.md`). **Haste X** (D114): the permanent enters with X Activation Points, capped at its max; plain **Haste** means it enters with its max. Haste is an entry-time keyword — granting it to a permanent already on the Island does nothing (a card that wants that says "gains X Activation Points" instead); Flicker on a Haste permanent sets its Activation Points to X again. **A control change leaves Activation Points untouched** — a stolen creature can act against its previous controller right away.

## Open sub-levers (tuning, not structure)
1. **AP refresh** — rec refill to max each turn, no carryover.
2. **Champion base AP** — the baseline total (proposed: 7, tuning) that funds all of draw/bond/move/attack/ability each turn. See `champions.md`.
3. **Champion starting AP at Setup** (D114) — placeholder 4 for both Champions, real number deferred.
4. **Trace Duration** (D50) — placeholder 5 rounds, real number deferred.

## Complexity discipline
Total systems: **mana + AP + terrain network + Pending/Speed + perception**. Mitigation: every AP cost is a **small integer**, resist per-action special cases, let depth come from combinations (pillar 3, tension #6 in PLAN).

## Invariant vs. mutable
- **Invariant:** mana is a single pool per Champion, plus one pool per Companion on the Island; Champion, Companion, Creature and Structure each have their own private Activation Points budget, and every AP cost is reached through the same payment rule (D58, D87); every actor reaches mana through the same query (climb to the first Champion or Companion pool, D74); every permanent entering the Island enters with 0 Activation Points (D89).
- **Mutable (card-driven):** Activation Points totals and per-action costs, what refills/carries over, whether defending costs AP, extra Activation Points, mana/AP ability costs — all effects/queries (pillar 5).
