# The Economy (two resources, one pattern)

*The full resource model in one place. There are **two** resources — mana and Energy — and the same shape — "mana access + a private action budget" — repeats for every actor, **including the Champion and the Companion**. Learn it once, it applies everywhere (pillar 3).*

**Decisions:** D8 (terrain/mana), D9 (Champion economy), D10 (Action Points), D15 (Defend cost), D22 (Companion), D45 (Speed), D47 (Activation Capacity), D48, D49 (Champion/Companion Attack), D58 (Energy), D65 (Defend once per round), D76 (first-turn Energy asymmetry), D77 (mana accounting), D87 (Terrain holds no Energy), D88, D110 (Remnant holds no Energy), D89 (summoning sickness), D92 (Instant blocks active play), D114 (Setup is entering; Haste X), D116 (`~` cost; Attack `3~En`, Defend `1~En`), D134 (connected Champion/Companion costs), D143 (stat changes), D144 (Duration), D147 (Health as a cost), D168 (Power, Health, damage), D158 (`!` drains at C5), D160 (entry Energy) — `history/decisions.md`

---

## The two resources

Still **exactly two** — a Companion does **not** introduce a third. It adds a second **pool instance** of the same resource (mana), not a new resource.

| Resource | Topology | Held by | Refills | Spent on |
|---|---|---|---|---|
| **Mana** | **one pool per Champion**, plus **one pool per Companion on the Island** (D22) | the Champion (its own pool, D8/D9); each Companion (its own separate pool, drawn by its own bonding) | live, the instant each bonded producer is reachable (D8/D77) | Champion's pool: casting spells, summoning units, **any** actor's mana-abilities. A Companion's pool: everything in that Companion's own subtree (`ancestry.md`) — never a different payer's abilities or spells. |
| **Energy** | **private, per permanent** (D58) | each **creature** (D10), the **Champion** (D9), each **Companion** (D22), and each **Structure** (D58, D87) | fully recharged each eligible turn (no carryover, rec) | move · fight · activate abilities · bond a terrain (Champion, Companion) · (Champion only) draw a card |

## The repeating pattern
Every board actor = **mana access** (the Champion's pool, or its own pool if it's a Companion) + **its own private per-turn Energy budget**.
- **Champion:** mana + its own Energy pool, priced like a creature's but weighted toward draw/bond/abilities over combat — attacking is deliberately the costliest line, so *channelers channel, they don't fight* (D9) by cost design, not by a separate scarce resource.
- **Companion:** its **own** mana pool (separate from the Champion's) + its own Energy pool, with Bond priced more restrictively than the Champion's relative to its smaller Energy total — draws mana like a channeler but can't share it (D22).
- **Creature:** shared mana + Energy. This is where most board action lives.

Abilities **bridge the two economies**: a creature ability can cost `X mana + Y Energy`; a Champion ability costs the same shape — `mana + Energy`. Same grammar at both scales, no special case for the Champion.

## Mana is global; Energy is local
- **Mana = one number per Champion.** The Champion's spells, its abilities, and *every* creature's mana-abilities all draw from that same pool (unless a Companion intercepts them into its own, `ancestry.md`). Channeling (D8) is what fills it. This is why the Champion is "a channeler": it's the tap the whole army drinks from.
- **Energy = private.** Each creature spends only its own Energy; the Champion spends only its own Energy. These never pool.

## Creatures are three numbers: Power / Health / Energy (D10, D168)
Energy **subsumes** the old separate stats rather than adding to them:
- **Movement stat is gone** → moving costs Energy; a creature's "speed" *is* its Energy.
- **Range demotes to a keyword** (`Ranged a-b`, D127); most creatures are melee (reach 0–1). The three *defining* numbers stay Power / Health / Energy.

## Move, attack and defend are default *abilities*, not rules (D10, D116)
There is no hardcoded move/attack logic. Every creature carries three **default abilities**, each **replaceable** by a creature-specific version:
- **`1En: Move`** — one hex per Energy.
- **`3~En: Attack`** — see the `~` cost below.
- **`1~En: Defend`** — see Defending below.

This is pillar 5 at its purest: the base rules *are* abilities, so the engine needs only an ability/cost system — no special move/attack code.

### Energy cost flavors: `xEn`, `x!En`, `x~En`, `x*En`
- **`xEn`** — spend exactly `x`. Leftover Energy remains usable (enables multi-action, e.g. a custom `1En` attack that permits hit-and-run or multi-attack).
- **`x!En`** ("exhaust") — **require `x`, then consume *all* remaining Energy.** The permanent can't act again until its next recharge, including on other players' turns: no defending, no reactive abilities. `x` is paid at C1 with the fixed costs; the rest is consumed at C5, after every surcharge (`interaction-stack.md` §Casting, D158) — so an exhausting Move onto a Sucking Mire needs `x`+1 Energy.
- **`x~En`** ("done for this turn", D116) — **spend exactly `x`; the permanent can't activate abilities until the end of this turn.** Leftover Energy stays and is usable again from the next turn on — in particular on the opponents' turns, for defending.
- **`x*En`** ("once per cycle") — **spend exactly `x`; this specific action may be used at most once per own-turn cycle** (from the controller's turn start to its next one), regardless of leftover Energy or Energy gained later.

**No-multi-attack and no hit-and-run are emergent, not rules:** default attack is `3~En`, so after attacking the creature can do nothing else this turn; a creature printed with `3En: Attack` *could* attack twice, or attack and move away.

**`!` is for powerful abilities:** a strong ability priced with `!` costs the permanent its defence until its next recharge — power is balanced against the ability to defend. Read `!` as **"exhaust."** A creature with no Energy left *looks* spent — which is why a card that **recharges 1** is deceptively strong (an apparently-exhausted creature can suddenly act or defend; feeds pillar 6).

**`*`** is used by the Champion's `5*En: Draw` and `2*En: Bond` actions (`champions.md`) — `Draw`/`Bond` need to stay usable *together* in one turn while each staying capped, which neither `!` nor `~` can express. Also used by the Companion's own Bond ability (`companions.md`) — same flavor, priced higher relative to the Companion's smaller Energy total so it crowds out the rest of the turn.

### Speed (Slow/Quick/Reactive/Instant, D45)
Every card/ability has one of four **Speeds**, gated by what's currently sitting in **Pending** (the Aether's next-to-resolve section, D38):
- **Slow** — controller's main phase only, Pending empty.
- **Quick** — whenever only **Physical Traces** (the trace left by Move/Attack/Ascend/Descend) are in Pending.
- **Reactive** — whenever no **Instant Trace** is in Pending.
- **Instant** — whenever no Instant is in Pending; while it sits in Pending, nothing can be actively put into the Aether and `Now` advances automatically until it has resolved — zero response window for anyone, not even another Instant. Triggers are still added and resolve normally. A deliberate simplification that keeps the top speed tier free of unbounded stack/priority-passing complexity.

Speed governs *when* a card/ability may be played; `!`/`~`/`*` still govern *how its Energy is consumed* — independent axes, same as before. Spending Energy reactively still draws from the actor's normal Energy budget, never a separate reactive pool — so answering on the opponent's turn means that Energy had to be held in reserve since the actor's own last turn. Applies generally, for any actor. Full detail: `interaction-stack.md`.

The `x!!Energy` ("double-exhaust") idea remains a proposed-not-adopted alternative — see `history/playtest-variants.md`.

### Defending (D116, D117)
Defending costs **`1~En`** (a Champion or Companion pays **`2~En` while network-bonded** — a value of its own, set at twice the default like its Move and Attack, D117, D134): 1 Energy, and the defender can't activate abilities for the rest of that turn. How Defend runs (a Quick response to the Attack, defenders added bit by bit): `interaction-stack.md` §Combat integration. Since every opponent turn is a separate turn (Neutral A, the other Champion, Neutral B), a creature can defend **once per opponent turn**, as long as it has Energy for it — every Energy held back from its own turn buys one more defence in a later opponent turn. A creature that went all in on its own turn (moved and attacked with everything) can't defend until its next recharge; one that used a `!` ability can't either. A card granting Energy makes a surprise defender. The cost to defending is also damage: each defence takes the attacker's. **Defender** (positive keyword): this creature has `1En: Defend` instead — it can defend any number of attacks per turn while its Energy lasts. `cannot defend` remains an occasional negative keyword. Rejected alternatives and the scenarios that decided it: `history/decisions.md` D15, D116, `history/playtest-variants.md`.

## Champion Energy — the same model as a creature (D9)
The Champion **has an Energy value and spends it directly**, exactly like a creature — one movement/combat/action query for every entity, no gating resource and no second combat model. The Champion's specific action costs (draw/bond/move/abilities) differ from a plain creature's defaults and live in `champions.md`; its **Attack cost matches the generic Actor default** (`3~En`; `6~En` while network-bonded, a value of its own set at twice the default, D49, D116, D134) — same for Companion. **First-turn asymmetry (D114):** Setup raises both Champions to at least the same starting Energy (placeholder 4); the first-move-advantage lever is that Setup is Champion A's first Beginning phase, so Champion A gets no recharge on turn 1 (`setup.md`).

## Energy: one resource for Champion, Companion, Creature and Structure (D58, D87, D88)
**Energy** is the one resource held by Champion, Companion, Creature and Structure alike — Item, Terrain and Remnant hold none (a Terrain's Energy cost climbs to its bonder, D87; a Remnant is what a fallen creature leaves, D110) — each with its own value **explicitly printed on its card data**, fully recharged every eligible Beginning phase (D89, D169, below), no carryover by default. There is no default value — a card's printed number has to be large enough to actually afford its own priciest self-funded ability, since holding Energy and funding an ability from them are the same thing. Values are kept comparable across types on purpose, so a generic effect ("target permanent gains 2 Energy") means the same thing regardless of what it targets.

**Payment is one rule, for every resource, not two separate rules by type:** to pay a cost, climb from the entity itself and stop at the **first** node — inclusive of the entity itself — that holds that resource (full detail: `ancestry.md`). Energy and Health never climb simply because the paying entity is the very first node checked and already holds them; mana climbs because only a Champion or Companion ever hold it, so the walk keeps going until it reaches one. A card may still explicitly name a different payer than this default walk would reach (e.g. "this creature's controller pays 2 Health") — a deliberate override, not something the general rule does on its own.

**Consequence:** a Structure's own Activation-Points-costed ability is funded by the Structure itself, not by whoever bonds/controls it — decoupled from the controlling Champion's own action economy. It's also what lets a Neutral Structure act on its own at all: an Activation-Points-only ability is always self-payable once Neutral, with no bonder required; a mana-inclusive ability still needs a live root supplying the mana. See `neutral-permanents.md`.

**Health is not a third resource alongside mana/Energy.** Whichever permanents have Health (Champion, Companion, Creature, Structure, D10/D24) hold it the same way they hold Energy, so a "pay N Health" ability on one of them is paid by itself — as N damage to itself (D147), self-paid, no climbing — unless a card explicitly names the controller as payer instead (`ancestry.md`).

**Changing Health and Energy (D143, D168, D169).** **Health** is one value, changed by **+X / −X**; what a permanent has taken is its **damage**, which persists and which **heal** removes (never below 0). It falls when its damage reaches its Health, so a temporary +X Health ending can make it fall. **Energy** works the same way (D169): one value, changed by **+X / −X**; what actions use up is its **drain**, which **recharge** removes (never below 0), fully at the start of each of its turns. What it can still spend is its Energy minus its drain. **Power** is never consumed: one value, changed by ±X.

## Summoning sickness (and Haste): 2 Energy on entering (D89, D114, D160)
**Every permanent entering the Island enters drained so that 2 Energy remain** (placeholder, D160) — all of it if its Energy is lower — cast, created by an effect, flickered, however it got there — and recharges at its controller's next Beginning phase. That is all summoning sickness is: the entry Energy is below the Attack cost, so a permanent that wasn't there at the beginning of its controller's turn can't attack yet. It can defend, including in the same turn when it was cast in an opponent's turn, and it can move or use any ability it can pay for with its entry Energy. This includes permanents placed at Setup (D114, `setup.md`). **Haste X** (D114): X Energy remain instead, at most its Energy; plain **Haste** means it enters undrained. Haste is an entry-time keyword — granting it to a permanent already on the Island does nothing (a card that wants that says "gains X Energy" instead); Flicker on a Haste permanent sets its Energy to X again. **A control change leaves Energy untouched** — a stolen creature can act against its previous controller right away.

## Open sub-levers (tuning, not structure)
1. **recharge** — rec refill to max each turn, no carryover.
2. **Champion base Energy** — the baseline total (proposed: 7, tuning) that funds all of draw/bond/move/attack/ability each turn. See `champions.md`.
3. **Champion starting Energy at Setup** (D114) — placeholder 4 for both Champions, real number deferred.
4. **Trace Duration** (D50, D144) — counted in ends of player turns; placeholder about 10, real number deferred.
5. **Entry Energy** (D160) — placeholder 2, kept below the Attack cost; set together with the default action costs (`PLAN.md` Track A item 67).

## Complexity discipline
Total systems: **mana + Energy + terrain network + Pending/Speed + perception**. Mitigation: every Energy cost is a **small integer**, resist per-action special cases, let depth come from combinations (pillar 3, tension #6 in PLAN).

## Invariant vs. mutable
- **Invariant:** mana is a single pool per Champion, plus one pool per Companion on the Island; Champion, Companion, Creature and Structure each have their own private Energy budget, and every Energy cost is reached through the same payment rule (D58, D87); every actor reaches mana through the same query (climb to the first Champion or Companion pool, D74); every permanent entering the Island enters with 0 Energy (D89).
- **Mutable (card-driven):** Energy totals and per-action costs, what refills/carries over, whether defending costs Energy, extra Energy, mana/Energy ability costs — all effects/queries (pillar 5).
