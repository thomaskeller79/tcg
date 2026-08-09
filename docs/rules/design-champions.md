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
- Has **Champion abilities**: activatable signature powers, gated by AP (below) and possibly extra mana cost. Any ability may be marked **instant-speed** (`^`, D23); default is **sorcery-speed** (own Action phase only, stack empty) — no Champion-specific timing rule, same mechanism as any other actor's abilities.
- 🪝 Abilities, stats, and channeling are the same **effects / queries / modifiers** as everything else (pillars 5). A Champion is not a bespoke subsystem — it's a card type with persistent identity.

## The Champion action economy — mana + Action Points (DECIDED, D9 · revised 2026-08-06)
The Champion runs the **same two-resource shape as a creature** — there is no bespoke "Channel" resource. Mechanically it's a special *kind* of creature (a king-like piece: exactly one per side, and the game ends when it dies), not a third action/combat model, per pillar 5's "no bespoke subsystems":

| Resource | Topology | Spent on |
|---|---|---|
| **Mana** | shared pool, however much is bonded & connected (D8) | casting spells, summoning units — **many** actions/turn |
| **Action Points (AP)** | private, refills each turn (same shape as a creature's AP, D10) | the Champion's own actions — draw, bond, move, fight, activate abilities |

**Mana is "summoning sick," under test (2026-08-08).** Mana is a **snapshot taken once per Beginning phase** (`RefreshManaEffect` = the sum of currently-connected/producing bonded terrain at that instant), not a live recomputation — a bond made mid-turn claims the tile permanently and (if immediately reachable) starts producing right away for `Query`-level purposes, but that extra mana isn't *credited to the pool* until the player's *next* Beginning phase. Same "no retroactive unlock this turn" shape as D14's summoning sickness for a freshly-summoned creature's AP. **Default match setup, under test:** both Champions start **pre-bonded to their own home tile** (their starting position, `MatchFactory`'s `bonds` param / a scenario's `bond` line) — 1 mana is already available turn 1 instead of requiring a full Bond-then-wait cycle first. Combined with the realm constraint below, this also means a fresh Champion is *already rooted* from turn 1: its first move costs `2AP` and stays inside its one-tile territory unless it pays `0AP` to Collapse.

**Default AP actions** (baseline example: **7 AP**; the exact numbers, and the full signature-ability list, are still open — this decision fixes the *shape* of the economy, not its content):

| Action | Cost | Notes |
|---|---|---|
| **Draw a card** | `5*AP` | *Replaces* the automatic per-turn draw entirely — no longer a free simultaneous Beginning-phase step. Skippable, but powerful enough that skipping should be rare. |
| **Bond a terrain** | `2*AP` | The D8 "up to once per turn" bonding limit, now expressed via the `*` once-per-turn cost flavor (below) instead of a dedicated Channel. |
| **Move** | `2AP`, only onto a hex that is itself part of the bonded network, while a network exists · `1AP`, unrestricted, once disconnected | **Under test, tightened 2026-08-08** — a hard lock confined to the Champion's own territory, not a cost differential; see the realm constraint below. |
| **Collapse Network** | `0AP` | Drops every current bond outright (not a pause — they're gone, re-bond from scratch). The escape valve for the Move lock above. **Under test, added 2026-08-08.** |
| **Attack** | `5!AP` network-active · `3!AP` if disconnected | Still expensive relative to the 7 AP baseline — keeps "fighting is the rare, costly line." Drain-all (`!`) like a Creature's default attack, so a Champion that attacks ends its turn's other actions. |
| **Defend** (retaliate) | `0*AP` | **Resolved 2026-08-09, D15.** No Champion-specific rule — it defends through the exact same `Query.CanDefend`/`ResolveDefendCost` as any creature: free, but at most once per turn, regardless of what else it spent AP on this turn. Closes the former "Champion retaliation" open question below. |
| **Champion abilities** | typically `2–5AP` | On top of the above; may still carry an additional mana cost. |

**The `*` cost notation.** `x*AP` = spend exactly `x`, but this specific action may be used **at most once per turn**, regardless of leftover or later-refilled AP — distinct from the existing `x!AP` ("drain all remaining AP," see `design-economy.md`). `!` was rejected for Draw/Bond because it drains the *whole* pool, which would make it impossible to draw *and* bond in the same turn — a combo this design wants to keep open.

**Corollary: a cleaner Beginning phase.** Since Draw is now a paid Action-phase choice instead of an automatic step, Beginning phase is left with exactly the two jobs a phase machine should have there — refresh resources (invisible to the players; nothing to decide) and fire beginning-of-turn triggers — with no player-facing action of its own baked in. See `rules-structure.md` §3.

**The tension.** There's no hardcoded "pick one of {bond, act, ability}" anymore — it's an open resource-allocation puzzle: `move + fight`, `move ×3`, `move + draw`, `draw + bond`, `bond + move ×2`, `bond + fight`, ability combos, and more, many of which can be the right call depending on the situation.
- The classic ramp-vs-spend tension **re-emerges on its own**: `Draw (5) + Bond (2)` exactly equals the 7 AP baseline, so "can't do everything" falls out of the numbers rather than being a hardcoded rule.
- **Watch-point:** with these numbers, `Attack (5, network-active) + Bond (2)` also exactly fits — so a Champion that skips its draw could fight *and* grow its economy in the same turn, something the old Channel model explicitly prevented. Whether that's acceptable or the numbers need adjusting is a playtest question.
- **Expectation, not a rule:** because fighting risks Life for no guaranteed return while draw/bond are safe value, combat-involving lines are expected to be comparatively rare by default — consistent with "a Champion's job is to channel, not to fight" (Concept, above), not something enforced by an explicit restriction.

**The realm constraint — REVISED, under test, tightened again 2026-08-08.** The Champion is the **root** of its network; mana flows from a bonded terrain only along an enemy-free path **to the Champion's current tile**. Earlier drafts of this rule (see the superseded paragraphs below) offered a free choice between a cheap disconnecting move and a pricier network-preserving one, then a "stay reachable" version that still let the Champion drift one hex beyond its actual bonded tiles. The current rule closes that: while the network is currently connected (at least one bonded terrain reachable — the same dynamic check D8 uses for producing mana), the Champion is *confined* — `ResolveLegalMoveTargets` only offers hexes that are **themselves already bonded** (`Query.IsBondedAndReachable`), each costing `2AP`. It may reposition anywhere within its own claimed territory, but may never step onto ground it hasn't bonded while still connected. To go anywhere else, it must first use **Collapse Network** (`0AP`, any time, no cost gate) — which **drops every bond outright**, not a reversible pause — after which it moves freely and cheaply, `1AP`, unrestricted, like an ordinary creature. A Champion that never bonded, or is currently fully blockaded by the enemy (D8's *reversible pause*, unaffected by this rule), already counts as "no network" and so is never confined — the lock only bites a Champion that is *actually* drawing mana right now. This yields the "walking channeler": shuffle within your own realm at `2AP` a hex, or spend `0AP` to cut it loose and move cheaply into open territory — a real, deliberate decision rather than a side effect of an ordinary move.

*Superseded paragraph 1 (kept for history, pre-2026-08-08):* "The Champion is the **root** of its network... Bending the leylines to follow costs effort: the pricier `2AP` move keeps the network **active** as the Champion relocates — nothing pauses, the realm effectively moves with it. The cheap `1AP` move (same cost as any creature) lets the network **collapse** instead: the Champion outpaces its own leylines, and every bonded terrain no longer reachable from its new tile is **reversibly** paused — restored the moment a path reconnects (same pause mechanic as enemy blocking, just self-inflicted)." Replaced because it never actually restricted *where* a disconnecting move could go, so "keep the network" was rarely worth the 2AP premium in practice.

*Superseded paragraph 2 (kept for history, 2026-08-08 morning):* the first "lock-in" draft required the destination to merely be *reachable through* bonded, enemy-free cells — in practice this meant any hex adjacent to a bonded cell qualified, not just the bonded cells themselves, so a Champion could still drift one hex outside its actual claimed territory while "staying connected." Replaced same-day after playtesting showed this reads as "I can still move outside of the network" — the current version requires the destination to *be* a bonded cell, full stop.

**Tuning target (not a structural fix):** bonding has **diminishing returns** while abilities **scale with mana**, so the ramp→act crossover is real but should stay *contestable* — pressure pulls it earlier, a greedy engine plan pushes it later.

**Escape valves live in cards, not the base rule.** "Gain extra AP," "this ability costs no AP," "may bond twice," etc. are card effects (pillar 5), same as for any creature.

🪝 Champion AP is a **per-turn resource query** (default budget = 7, tuning) and each action is an **effect** — extra AP, free abilities, "may bond twice," reactive (instant-speed) abilities are all data, exactly like a creature's AP.

**Open — content, not shape:** the Champion's actual signature-ability list, exact AP costs, and how Champion defense/retaliation works in full (see Open questions, below) are not yet designed.

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

*(Resolved questions are cut once closed — the rule lives in the sections above and, for decision-grade calls, in `decisions.md`. Only genuinely open items stay here.)*

## Open questions
1. **Term:** Champion, Channeler, or something else? *(Flavor now leans "channeler" — the defining verb is to channel.)*
2. **Down-leveling:** once a Champion enters a higher level band, may it still be fielded in a lower bracket? (Recommendation leans yes, to preserve access and queue health — not yet decided.)
3. **Level-up conditions:** what kinds of conditions drive leveling (win counts, in-match objectives, specific plays)? — a design task to spec later.
