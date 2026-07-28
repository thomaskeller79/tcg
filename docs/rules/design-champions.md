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
- Has **Champion abilities**: activatable signature powers, gated by the Channel action (below) and possibly extra mana cost.
- 🪝 Abilities, stats, and channeling are the same **effects / queries / modifiers** as everything else (pillars 5). A Champion is not a bespoke subsystem — it's a card type with persistent identity.

## The Champion action economy — the Channel (DECIDED, D9)
The Champion has **two independent economies**, and keeping them separate is the whole point:

| Economy | Budget/turn | Spent on |
|---|---|---|
| **Mana** | however much is bonded & connected (D8) | casting spells, summoning units — **many** actions/turn |
| **Channel** | **exactly one** per turn | the Champion's own action — see below |

Each turn the Champion may spend its single **Channel** on **one** of:
1. **Bond** — add one terrain to its network (this *is* the D8 once/turn bond; the Channel is the bond limiter — one rule, not two). Grows mana.
2. **Act as a creature** — spend the Champion's **AP** this turn to move and/or fight, exactly like a creature (D10); its AP is *only* unlockable by choosing this option (movement realm-restricted, below). The aggressive line: it costs *both* ramp and ability use, structurally enforcing "channelers aren't warriors."
3. **Activate a Champion ability** — a signature power; may carry an **additional mana cost** paid on top.

> The central tension is **bond vs. activate ability**: invest in the economy, or spend it. "Act as a creature" is the rarer, situational third choice (defend, relocate the realm, seize a key hex).

**The realm constraint (reconciled with D8 — reversible pause, not collapse).** The Champion is the **root** of its network; mana flows from a bonded terrain only along an enemy-free path **to the Champion's current tile**. Moving *along* the bonded network is always safe. Stepping *off* it pauses the disconnected terrains' mana — **reversibly**, restored on return (same pause mechanic as enemy blocking, self-inflicted). This yields the "walking channeler": move to the frontier → bond outward → repeat, dragging the realm across the map. *(Harsher "permanent collapse on leaving" is a later dial if relocation proves too cheap.)*

**Tuning target (not a structural fix):** bonding has **diminishing returns** while abilities **scale with mana**, so the ramp→act crossover is real but should stay *contestable* — pressure pulls it earlier, a greedy engine plan pushes it later. Tune abilities so the bond-vs-ability choice is close throughout, not a solved curve.

**Escape valves live in cards, not the base rule.** The strict one-Channel default is the simplest rule that exposes the hook; "gain an extra Channel," "this ability costs no Channel," etc. are card effects (pillar 5).

🪝 The Channel is a **per-turn resource query** (default budget = 1) and each option is an **effect**. → extra Channels, free abilities, "may bond twice," reactive (instant-speed) abilities are all data.

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
- **Resources (RESOLVED, D8):** the Champion **is** the resource engine — the root node of the terrain network; the Channel action is how it bonds.
- **Deckbuilding:** Champion likely gates deck identity (faction/color restriction hook).
- **Asymmetric info:** by default a Champion's identity is probably **known** to both players (it's chosen openly), though its *current abilities/path* could be partially hidden — open question.

## Invariant vs. mutable
- **Invariant:** each player has exactly one Champion; the match core consumes a resolved loadout and does not run progression; progression state is server-authoritative.
- **Mutable (card/path-driven):** a Champion's abilities, stats, channeling, and path — and how in-match effects modify them.

## Open questions
1. **Term:** Champion, Channeler, or something else? *(Flavor now leans "channeler" — the defining verb is to channel.)*
2. ~~**Win condition**~~ — **RESOLVED (D9):** killing the enemy Champion = loss; no separate Base. No respawn by default.
3. ~~**On-board vulnerability**~~ — **RESOLVED (D9):** the Champion **is attackable**. It is protected only by summoned units (declare-defenders, D4) and by keeping it deep in its own realm — not by rules immunity. *(Sub-question: does the Champion have a baseline defense/retaliation, or is it purely fragile? — tuning.)*
4. **Reactive abilities & the Channel:** the one-Channel limit is a your-turn budget. May some Champion abilities be **instant-speed** (D5 priority) on the opponent's turn, and if so do they draw from the same single Channel or a separate reactive budget? *(Recommend: start with all Champion actions on your own turn; add reactive abilities via cards later.)*
5. ~~**Progression philosophy**~~ — **RESOLVED (D2):** level bands; horizontal within a band, step up between bands; play gated by band. Sub-question still open: **down-leveling** (may a higher champion play a lower bracket?).
6. **Level-up conditions:** what kinds of conditions drive leveling (win counts, in-match objectives, specific plays)? — a design task to spec later.
7. ~~**Resource link**~~ — **RESOLVED (D8):** the Champion is the **root node of the terrain connection network** — the economy grows outward from it, so Champion position is an economic decision. See `design-resources-terrain.md`.
8. **Deck identity:** does the chosen Champion restrict which cards/factions the deck may run?
