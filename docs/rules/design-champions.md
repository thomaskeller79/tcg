# Design — Champions (Pillar 7)

*Each player is embodied on the board by a single Champion: an avatar that summons creatures, casts spells, and channels magic — and that **evolves between games** along branching paths. Provisional term "Champion" (candidates: Channeler, Champion; not "Commander"). Less crucial than pillars 1–6, but a core identity and retention anchor.*

**Status:** Design note · **Date:** 2026-07-23

---

## Concept
- Each player **chooses one Champion** before a match (like MTG's Commander in spirit, but as an **on-board avatar**, not a card in the deck).
- In-match the Champion is a **persistent entity on the board** that: summons units, casts spells, channels resources/energy, and uses a set of **Champion abilities**.
- Between matches the Champion **levels up** when designed conditions are met, unlocking growth along **branching paths**.
- The Champion likely shapes deckbuilding identity (which cards/factions your deck may include) — ties into the faction-structure hook.

## Two distinct layers (keep them separate!)
| Layer | Lives where | Contents |
|---|---|---|
| **In-match Champion** | Inside the deterministic core | Board entity: position, HP, abilities available *this match*, resource channeling. Just another (special) unit/card type the engine simulates. |
| **Meta progression** | **Outside** the core, in a meta-layer | Level, XP/conditions met, chosen path, unlocked abilities. Persists across matches. **Mutates the Champion definition between games.** |

> **The core consumes a fully-resolved Champion *loadout* as input.** It never runs progression logic. This preserves determinism, replays, netcode, and testability (see PLAN §6). Progression is a separate system that produces the loadout the core plays with.

## In-match: what a Champion is
- A special **card type / board entity**, one per player, placed at match start on its **home tile** (usually a landmark terrain, but not a special *objective* — see win condition).
- Has stats like a unit (HP, Movement). **It is attackable, and its death loses the game** (D9). There is **no separate Base** — the Champion *is* the objective.
- **Flavor / role.** The world is saturated with mana; only **channelers** can draw it from the land and turn it into magic. A Champion's job is to *channel*, not to fight. It is simultaneously the **economic root** (D8), the **win condition**, and the **most exposed piece** — one entity carrying all three roles is what makes its every decision tense.
- Has **Champion abilities**: activatable signature powers, gated by AP (below) and possibly extra mana cost.
- 🪝 Abilities, stats, and channeling are the same **effects / queries / modifiers** as everything else (pillars 5). A Champion is not a bespoke subsystem — it's a card type with persistent identity.

## The Champion action economy — mana + Action Points (DECIDED, D9 · revised 2026-08-06)
The Champion runs the **same two-resource shape as a creature** — there is no bespoke "Channel" resource. Mechanically it's a special *kind* of creature (a king-like piece: exactly one per side, and the game ends when it dies), not a third action/combat model, per pillar 5's "no bespoke subsystems":

| Resource | Topology | Spent on |
|---|---|---|
| **Mana** | shared pool, however much is bonded & connected (D8) | casting spells, summoning units — **many** actions/turn |
| **Action Points (AP)** | private, refills each turn (same shape as a creature's AP, D10) | the Champion's own actions — draw, bond, move, fight, activate abilities |

**Default AP actions** (baseline example: **7 AP**; the exact numbers, and the full signature-ability list, are still open — this decision fixes the *shape* of the economy, not its content):

| Action | Cost | Notes |
|---|---|---|
| **Draw a card** | `5*AP` | *Replaces* the automatic per-turn draw entirely — no longer a free simultaneous Beginning-phase step. Skippable, but powerful enough that skipping should be rare. |
| **Bond a terrain** | `2*AP` | The D8 "up to once per turn" bonding limit, now expressed via the `*` once-per-turn cost flavor (below) instead of a dedicated Channel. |
| **Move** | `2AP` if the leyline network stays active · `1AP` (same as any creature) if the network is allowed to collapse | The cost differential does the job the old binary Channel choice used to — see the realm constraint below. |
| **Attack** | `5AP` network-active · `3AP` if allowed to collapse | Still expensive relative to the 7 AP baseline — keeps "fighting is the rare, costly line." |
| **Champion abilities** | typically `2–5AP` | On top of the above; may still carry an additional mana cost. |

**The `*` cost notation.** `x*AP` = spend exactly `x`, but this specific action may be used **at most once per turn**, regardless of leftover or later-refilled AP — distinct from the existing `x!AP` ("drain all remaining AP," see `design-economy.md`). `!` was rejected for Draw/Bond because it drains the *whole* pool, which would make it impossible to draw *and* bond in the same turn — a combo this design wants to keep open.

**The tension.** There's no hardcoded "pick one of {bond, act, ability}" anymore — it's an open resource-allocation puzzle: `move + fight`, `move ×3`, `move + draw`, `draw + bond`, `bond + move ×2`, `bond + fight`, ability combos, and more, many of which can be the right call depending on the situation.
- The classic ramp-vs-spend tension **re-emerges on its own**: `Draw (5) + Bond (2)` exactly equals the 7 AP baseline, so "can't do everything" falls out of the numbers rather than being a hardcoded rule.
- **Watch-point:** with these numbers, `Attack (5, network-active) + Bond (2)` also exactly fits — so a Champion that skips its draw could fight *and* grow its economy in the same turn, something the old Channel model explicitly prevented. Whether that's acceptable or the numbers need adjusting is a playtest question.
- **Expectation, not a rule:** because fighting risks Life for no guaranteed return while draw/bond are safe value, combat-involving lines are expected to be comparatively rare by default — consistent with "a Champion's job is to channel, not to fight" (Concept, above), not something enforced by an explicit restriction.

**The realm constraint (reconciled with D8 — reversible pause, not collapse).** The Champion is the **root** of its network; mana flows from a bonded terrain only along an enemy-free path **to the Champion's current tile**. The cheap `1AP` move (network intact) is always safe. The pricier `2AP` move that steps off the network pauses the disconnected terrains' mana — **reversibly**, restored on return (same pause mechanic as enemy blocking, self-inflicted). This yields the "walking channeler": move to the frontier → bond outward → repeat, dragging the realm across the map. *(Harsher "permanent collapse on leaving" is a later dial if relocation proves too cheap.)*

**Tuning target (not a structural fix):** bonding has **diminishing returns** while abilities **scale with mana**, so the ramp→act crossover is real but should stay *contestable* — pressure pulls it earlier, a greedy engine plan pushes it later.

**Escape valves live in cards, not the base rule.** "Gain extra AP," "this ability costs no AP," "may bond twice," etc. are card effects (pillar 5), same as for any creature.

🪝 Champion AP is a **per-turn resource query** (default budget = 7, tuning) and each action is an **effect** — extra AP, free abilities, "may bond twice," reactive (instant-speed) abilities are all data, exactly like a creature's AP.

**Open — content, not shape:** the Champion's actual signature-ability list, exact AP costs, and how Champion defense/retaliation works in full (see Open question 3, below) are not yet designed.

## Meta: progression & paths
- **Leveling:** the Champion gains levels when **conditions** are met across matches (conditions TBD — design task). Not within a single game.
- **Paths:** each Champion has **multiple branching progression paths** (skill-tree-like). A player commits to a path over time, giving each Champion multiple distinct identities.
- **Data-driven:** Champions, their abilities, and their path trees are defined in data, like cards.
- Progression state is **server-authoritative** (anti-cheat), consistent with pillar 6. Offline solo/hotseat play grants **no progression**; online solo vs AI earns progression via **replay verification** (the server independently replays the match's recorded command log rather than trusting a client-reported result); online 1v1 is live-hosted and inherently secure. See `docs/architecture/decisions-architecture.md` A4 for the full mechanism and why a client-local progression mirror was rejected.

## Progression fairness — level bands (DECIDED, D2)
Persistent power progression is the classic route to *veteran-stomps-newbie* and *pay-to-win*, and it is especially corrosive here because hidden-information play (pillar 6) already rewards experience. Resolution:

> **Power is a step function via level bands.** Champions grow within **bands** (arbitrary example: 1–5 / 6–10 / 11–15). Progression is **horizontal within a band** (sidegrades, different playstyles) and takes a **discrete power jump when crossing into the next band**. Matches/tournaments are defined per band ("bring a level 1–5 champion"), so **a champion never faces one outside its band** — vertical growth never creates an unfair match.

Consequences:
- **Three distinct metagames** (one per band) → depth and longevity.
- **Phased launch:** ship band 1 first, open higher bands as the game matures — turns the ~3× balance cost into a release schedule.
- **Milestone retention:** crossing a band boundary is an event.
- **Open — access / down-leveling:** once a champion enters a higher band, is it locked out of lower brackets? Recommendation: allow **down-leveling** (field any champion in any bracket at/below its level, using only that bracket's legal config) to preserve access and keep queues healthy.

## Interactions with other pillars / systems
- **Win condition (RESOLVED, D9):** killing the enemy Champion **wins the game** (Duelyst-style). There is **no separate Base** — the home tile is just the Champion's start location (usually a landmark terrain), not a destructible objective. The Champion is objective + economic root + most-exposed piece in one.
- **Resources (RESOLVED, D8):** the Champion **is** the resource engine — the root node of the terrain network; Bond (its `2*AP` action) is how it bonds.
- **Deckbuilding:** Champion likely gates deck identity (faction/color restriction hook).
- **Asymmetric info:** by default a Champion's identity is probably **known** to both players (it's chosen openly), though its *current abilities/path* could be partially hidden — open question.

## Invariant vs. mutable
- **Invariant:** each player has exactly one Champion; the match core consumes a resolved loadout and does not run progression; progression state is server-authoritative.
- **Mutable (card/path-driven):** a Champion's abilities, stats, AP costs, and path — and how in-match effects modify them.

## Open questions
1. **Term:** Champion, Channeler, or something else? *(Flavor now leans "channeler" — the defining verb is to channel.)*
2. ~~**Win condition**~~ — **RESOLVED (D9):** killing the enemy Champion = loss; no separate Base. No respawn by default.
3. ~~**On-board vulnerability**~~ — **RESOLVED (D9):** the Champion **is attackable**. It is protected only by summoned units (declare-defenders, D4) and by keeping it deep in its own realm — not by rules immunity. **Sub-question, updated (2026-08-06) — Champion retaliation:** goal is **one unified defend/retaliate rule for creatures and Champion alike**, not a Champion-specific special case. Desired shape: a Champion (or creature) that spent its whole turn's resources elsewhere (e.g. drew a card and bonded) should no longer be able to retaliate if forced into a defensive fight, while one that kept AP in reserve (e.g. by moving/fighting at the cheaper, network-collapsing cost instead of drawing or bonding) still can. Note this is narrower than it first looks: D13's existing "undefended = no retaliation" default already covers the case where nothing is declared to defend the Champion at all — the open part is whether the Champion may ever validly declare **itself** as a defender in the first place, and under what resource state. Two candidate mechanisms to playtest (also cross-referenced at D15, since it's the same defend-cost lever):
   - **Option A — defending costs `1AP`.** Reprises D15's V1. **Stricter:** blocks retaliation whenever 0 AP remains, for *any* reason (whatever the AP was spent on).
   - **Option B — using a `!`-costed action forbids defending this turn.** Retaliation is gated on whether a draining (`!`) action was used, not on raw AP remaining. **Looser:** a creature/Champion at 0 AP from only plain (non-`!`) costs could still retaliate if defending itself is free. To reproduce "drew + bonded ⇒ can't retaliate" under this option, Draw and/or Bond would need the `!` flavor instead of `*` — which would then prevent doing both in the same turn (since `!` drains everything), reopening the "which combos fit in one turn" question above. Not resolved here; a genuine fork against the `*`-based proposal, left to playtest.
4. **Reactive abilities & AP:** Champion AP is a your-turn budget. May some Champion abilities be **instant-speed** (D5 priority) on the opponent's turn, and if so do they draw from the same AP pool or a separate reactive budget? *(Recommend: start with all Champion actions on your own turn; add reactive abilities via cards later.)*
5. ~~**Progression philosophy**~~ — **RESOLVED (D2):** level bands; horizontal within a band, step up between bands; play gated by band. Sub-question still open: **down-leveling** (may a higher champion play a lower bracket?).
6. **Level-up conditions:** what kinds of conditions drive leveling (win counts, in-match objectives, specific plays)? — a design task to spec later.
7. ~~**Resource link**~~ — **RESOLVED (D8):** the Champion is the **root node of the terrain connection network** — the economy grows outward from it, so Champion position is an economic decision. See `design-resources-terrain.md`.
8. **Deck identity:** does the chosen Champion restrict which cards/factions the deck may run?
